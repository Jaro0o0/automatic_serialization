using Assesment_Api.Models;

namespace Assesment_Api.Services;

public interface IChocolateService
{
    Task<ChocolateData?> GetDataAsync();
}
