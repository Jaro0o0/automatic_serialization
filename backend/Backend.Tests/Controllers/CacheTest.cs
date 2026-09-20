using Assesment_Api.Models;
using Assesment_Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Moq;

public class ChocolateControllerTests
{
    [Fact]
    public async Task SerializeAndSave_ShouldUseCache_OnSecondRequest()
    {
        // Arrange
        var chocolateData = new ChocolateData
        {
            Fact = "Chocolate is delicious",
            Length = 20
        };

        // Mock Service
        var serviceMock = new Mock<IChocolateService>();

        serviceMock
            .Setup(x => x.GetDataAsync())
            .ReturnsAsync(chocolateData);

        // MemoryCache
        var cache = new MemoryCache(new MemoryCacheOptions());

        // Mock  Environment
        var environmentMock = new Mock<IWebHostEnvironment>();

        var testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(testRoot, "data"));

        environmentMock
            .Setup(x => x.ContentRootPath)
            .Returns(testRoot);

        // Controller
        var controller = new ChocolateController(
            serviceMock.Object,
            cache,
            environmentMock.Object);

        var request = new SaveRequest
        {
            Path = Path.Combine(
                testRoot,
                "test-cache.txt")
        };

        try
        {
            // Act
            // First call – will fetch data from the service
            await controller.SerializeAndSave("txt", request);

            // Second call – should retrieve data from the cache.
            await controller.SerializeAndSave("txt", request);





            // Assert
            serviceMock.Verify(
                x => x.GetDataAsync(),
                Times.Once);
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }
}

