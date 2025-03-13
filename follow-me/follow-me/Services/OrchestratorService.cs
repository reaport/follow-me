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

        public async Task StartMovementAsync(string carId, string aircraftId)
        {
            Logger.Log("OrchestratorService", "INFO", $"Отправка запроса на начало движения для машины {carId}.");

            // Создаем тело запроса с aircraftId
            var requestBody = new { aircraftId };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // Отправляем запрос с телом
            var response = await _httpClient.PostAsync($"/follow-me/start", jsonContent);
            response.EnsureSuccessStatusCode();

            Logger.Log("OrchestratorService", "INFO", $"Запрос на начало движения для машины {carId} успешно отправлен.");
        }

        public async Task EndMovementAsync(string carId, string aircraftId)
        {
            Logger.Log("OrchestratorService", "INFO", $"Отправка запроса на окончание движения для машины {carId}.");

            // Создаем тело запроса с aircraftId
            var requestBody = new { aircraftId };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // Отправляем запрос с телом
            var response = await _httpClient.PostAsync($"/follow-me/finish", jsonContent);
            response.EnsureSuccessStatusCode();

            Logger.Log("OrchestratorService", "INFO", $"Запрос на окончание движения для машины {carId} успешно отправлен.");
        }
    }
}