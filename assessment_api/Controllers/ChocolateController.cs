using Assesment_Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

[ApiController]
[Route("serialize/[controller]")]
public class ChocolateController : ControllerBase
{
    private readonly ChocolateService _request;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "ChocolateDataKey";

    public ChocolateController(

        ChocolateService request,
        IMemoryCache cache)
    {
        _request = request;
        _cache = cache;
    }

    [HttpPost("{dataType}")]
    public async Task<IActionResult> MakeTxt(string dataType, [FromBody] SaveRequest request)
    {
        string jsonString;

        // Sprawdzamy cache
        if (!_cache.TryGetValue(CacheKey, out jsonString))
        {
            // Jeśli nie ma danych w cache,
            // pobieramy je z API
            var data = await _request.GetDataAsync();

            // Serializacja do JSON
            jsonString = JsonSerializer.Serialize(data);

            // Zapisujemy dane do cache na 5 minut
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(CacheKey, jsonString, cacheOptions);
        }

        // Przy KAŻDYM requestcie dopisujemy dane
        // do pliku w nowej linii
        string Path = request.path;

        await System.IO.File.AppendAllTextAsync(
            Path,
            jsonString + Environment.NewLine
        );

        return Ok(new
        {
            message = "Dane zostały przetworzone.",
            dane = jsonString
        });
    }
}


//     // CSV_SERIALZITION
//     [HttpPost("csv")]
//     public async Task<IActionResult> GetCsv()
//     {
//                 string jsonString;

//             if (!_cache.TryGetValue(CacheKey, out jsonString))
//             {
//                 var data = await _request.GetDataAsync();

//                 jsonString = JsonSerializer.Serialize(data);

//                 var cacheOptions = new MemoryCacheEntryOptions()
//                     .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

//                 _cache.Set(CacheKey, jsonString, cacheOptions);
//             }

//             var dataObject = JsonSerializer.Deserialize<ChocolateData>(jsonString);

//             string csv = $"Fact,Length{Environment.NewLine}" +
//                         $"\"{dataObject?.Fact}\",{dataObject?.Length}";

//             return File(
//                 System.Text.Encoding.UTF8.GetBytes(csv),
//                 "text/csv",
//                 "chocolate.csv"
//             );
//     }


//     // XML_SERIALIZATION
//     [HttpPost("xml")]
//     public async Task<IActionResult> GetXml()
//     {
//         string? jsonString;

//         if (!_cache.TryGetValue(CacheKey, out jsonString))
//         {
//             var data = await _request.GetDataAsync();

//             jsonString = JsonSerializer.Serialize(data);

//             var cacheOptions = new MemoryCacheEntryOptions()
//                 .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

//             _cache.Set(CacheKey, jsonString, cacheOptions);
//         }

//         var dataObject = JsonSerializer.Deserialize<ChocolateData>(jsonString);

//         var serializer = new XmlSerializer(typeof(ChocolateData));

//         using var stringWriter = new StringWriter();

//         serializer.Serialize(stringWriter, dataObject);

//         string xml = stringWriter.ToString();

//         return File(
//             System.Text.Encoding.UTF8.GetBytes(xml),
//             "application/xml",
//             "chocolate.xml"
//         );
//     }
// }

