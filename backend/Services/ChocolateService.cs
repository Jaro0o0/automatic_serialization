using Assesment_Api.Models;

namespace Assesment_Api.Services
{
    


    public class ChocolateService : IChocolateService
    {
        private readonly HttpClient _httpClient;


        public ChocolateService(HttpClient httpClient ){

            _httpClient = httpClient;
        }


        public async Task<ChocolateData?> GetDataAsync()
        {
            // Serwis zajmuje się logiką pobierania danych
            return await _httpClient.GetFromJsonAsync<ChocolateData>("https://catfact.ninja/fact");
            
        }

    }

}
