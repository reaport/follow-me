using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FollowMe.Utils;

namespace FollowMe.Services
{
    public class OrchestratorService : IOrchestratorService
    {
        private readonly HttpClient _httpClient;

        public OrchestratorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task StartMovementAsync(string carId)
        {
            Logger.Log("OrchestratorService", "INFO", $"Отправка запроса на начало движения для машины {carId}.");

            var response = await _httpClient.PostAsync($"/follow-me/{carId}/start", null);
            response.EnsureSuccessStatusCode();

            Logger.Log("OrchestratorService", "INFO", $"Запрос на начало движения для машины {carId} успешно отправлен.");
        }

        public async Task EndMovementAsync(string carId)
        {
            Logger.Log("OrchestratorService", "INFO", $"Отправка запроса на окончание движения для машины {carId}.");

            var response = await _httpClient.PostAsync($"/follow-me/{carId}/end", null);
            response.EnsureSuccessStatusCode();

            Logger.Log("OrchestratorService", "INFO", $"Запрос на окончание движения для машины {carId} успешно отправлен.");
        }
    }
}