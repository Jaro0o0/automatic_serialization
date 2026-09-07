using Assesment_Api.Models;
using Assesment_Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

[ApiController]
[Route("serialize/[controller]")]
public class ChocolateController : ControllerBase
{
    private readonly ChocolateService _request;
    private readonly IMemoryCache _cache;
    private readonly string _localDataPath;

    private const string CacheKey = "ChocolateDataKey";

    public ChocolateController(
        ChocolateService request,
        IMemoryCache cache,
        IWebHostEnvironment environment)
    {
        _request = request;
        _cache = cache;
        _localDataPath = System.IO.Path.Combine(
            environment.ContentRootPath,
            "data",
            "data.txt"
        );
    }

    [HttpPost("{dataType}")]
    public async Task<IActionResult> SerializeAndSave(string dataType, [FromBody] SaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
        {
            return BadRequest(new { message = "Provide a file save path." });
        }

        var data = await GetChocolateDataAsync();
        if (data is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Failed to retrieve data from the external API."
            });
        }

        string text = $"Fact: {data.Fact}, Length: {data.Length}";
        await AppendLineAsync(_localDataPath, text);


        string serializedData;

        switch (dataType.ToLowerInvariant())
        {
            // TXT SERIALIZATION
            case "txt":
                serializedData = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                break;

            // CSV SERIALIZATION
            case "csv":
                serializedData = $"Fact,Length{Environment.NewLine}\"{data.Fact.Replace("\"", "\"\"")}\",{data.Length}";
                break;

            // XML SERIALIZATION
            case "xml":
                var serializer = new XmlSerializer(typeof(ChocolateData));
                using (var stringWriter = new StringWriter())
                {
                    serializer.Serialize(stringWriter, data);
                    serializedData = stringWriter.ToString();
                }
                break;

            default:
                return BadRequest(new { message = "Supported formats are: txt, csv, xml." });
        }

        try
        {
            var outputPath = System.IO.Path.IsPathRooted(request.Path)
                ? request.Path
                : System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(_localDataPath)!,
                    request.Path);

            // Gdy request wskazuje lokalny data.txt, wpis został już dopisany powyżej.
            // Nie nadpisujemy go zserializowaną zawartością.
            if (string.Equals(
                System.IO.Path.GetFullPath(outputPath),
                System.IO.Path.GetFullPath(_localDataPath),
                StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new
                {
                    message = "Data has been appended.",
                    path = _localDataPath,
                    format = dataType.ToLowerInvariant()
                });
            }

            var directoryPath = System.IO.Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await System.IO.File.WriteAllTextAsync(outputPath, serializedData, Encoding.UTF8);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException or ArgumentException or NotSupportedException)
        {
            return BadRequest(new { message = $"Unable to save the file to the provided path: {exception.Message}" });
        }

        return Ok(new
        {
            message = "Data has been saved.",
            path = request.Path,
            format = dataType.ToLowerInvariant()
        });
    }

    private async Task<ChocolateData?> GetChocolateDataAsync()
    {
        if (_cache.TryGetValue(CacheKey, out ChocolateData? cachedData))
        {
            return cachedData;
        }

        var data = await _request.GetDataAsync();
        if (data is not null)
        {
            _cache.Set(CacheKey, data, TimeSpan.FromMinutes(5));
        }

        return data;
    }

    private static async Task AppendLineAsync(string path, string text)
    {
        var addNewLineBeforeText = false;

        if (System.IO.File.Exists(path))
        {
            await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (file.Length > 0)
            {
                file.Seek(-1, SeekOrigin.End);
                addNewLineBeforeText = file.ReadByte() != '\n';
            }
        }

        var prefix = addNewLineBeforeText ? Environment.NewLine : string.Empty;
        await System.IO.File.AppendAllTextAsync(path, $"{prefix}{text}{Environment.NewLine}");
    }
}
