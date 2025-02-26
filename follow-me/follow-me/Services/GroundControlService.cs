using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FollowMe.Utils;

namespace FollowMe.Services
{
    public class GroundControlService
    {
        private readonly HttpClient _httpClient;

        public GroundControlService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Регистрация транспорта
        public async Task<VehicleRegistrationResponse> RegisterVehicle(string vehicleType)
        {
            Logger.Log("GroundControlService", "INFO", $"Регистрация транспорта типа {vehicleType}.");

            var response = await _httpClient.PostAsync($"https://ground-control.reaport.ru/register-vehicle/{vehicleType}", null);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VehicleRegistrationResponse>(responseBody);
        }

        // Получение маршрута
        public async Task<string[]> GetRoute(string from, string to)
        {
            Logger.Log("GroundControlService", "INFO", $"Запрос маршрута из {from} в {to}.");

            var request = new { from, to };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://ground-control.reaport.ru/route", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<string[]>(responseBody);
        }

        // Запрос разрешения на перемещение
        public async Task<double> RequestMove(string vehicleId, string vehicleType, string from, string to)
        {
            Logger.Log("GroundControlService", "INFO", $"Запрос на перемещение транспорта {vehicleId} из {from} в {to}.");

            var request = new
            {
                vehicleId,
                vehicleType,
                from,
                to
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://ground-control.reaport.ru/move", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var moveResponse = JsonSerializer.Deserialize<MoveResponse>(responseBody);

                Logger.Log("GroundControlService", "INFO", $"Разрешение на перемещение получено. Расстояние: {moveResponse.Distance}.");
                return moveResponse.Distance;
            }
            else
            {
                Logger.Log("GroundControlService", "ERROR", $"Ошибка при запросе на перемещение: {response.StatusCode}.");
                return -1; // Ошибка или запрет на движение
            }
        }

        // Уведомление о прибытии
        public async Task NotifyArrival(string vehicleId, string vehicleType, string nodeId)
        {
            Logger.Log("GroundControlService", "INFO", $"Уведомление о прибытии транспорта {vehicleId} в узел {nodeId}.");

            var request = new
            {
                vehicleId,
                vehicleType,
                nodeId
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://ground-control.reaport.ru/arrived", content);
            response.EnsureSuccessStatusCode();
        }

        // Отправка сигналов навигации
        public async Task SendNavigationSignal(string vehicleId, string signal)
        {
            Logger.Log("GroundControlService", "INFO", $"Отправка сигнала навигации {signal} для транспорта {vehicleId}.");

            var request = new { Navigate = signal };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://ground-control.reaport.ru/navigate", content);
            response.EnsureSuccessStatusCode();
        }

        // Модели для десериализации ответов
        private class MoveResponse
        {
            public double Distance { get; set; }
        }

        public class VehicleRegistrationResponse
        {
            public string GarrageNodeId { get; set; }
            public string VehicleId { get; set; }
            public Dictionary<string, string> ServiceSpots { get; set; }
        }
    }
}