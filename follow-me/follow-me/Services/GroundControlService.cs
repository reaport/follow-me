using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FollowMe.Utils;
using FollowMe.Data;

namespace FollowMe.Services
{
    public class GroundControlService : IGroundControlService
    {
        private readonly HttpClient _httpClient;

        public GroundControlService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<VehicleRegistrationResponse?> RegisterVehicle(string vehicleType)
        {
            Logger.Log("GroundControlService", "INFO", $"Регистрация машины типа {vehicleType}.");

            var response = await _httpClient.PostAsync($"/register-vehicle/{vehicleType}", null);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Logger.Log("GroundControlService", "INFO", $"Машина зарегистрированна. Ответ: {responseBody}.");
            return JsonSerializer.Deserialize<VehicleRegistrationResponse?>(responseBody);
        }

        public async Task<string[]> GetRoute(string from, string to)
        {
            Logger.Log("GroundControlService", "INFO", $"Запрос маршрута из {from} в {to}.");

            var request = new { from, to, type = "follow-me" };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/route", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Logger.Log("GroundControlService", "INFO", $"Маршрут получен. Ответ: {responseBody}.");
            return JsonSerializer.Deserialize<string[]>(responseBody);
        }

        public async Task<double> RequestMove(string vehicleId, string vehicleType, string from, string to, string withAirplane)
        {
            Logger.Log("GroundControlService", "INFO", $"Запрос на передвижение маишны {vehicleId} из {from} в {to}.");

            var request = new
            {
                vehicleId,
                vehicleType,
                from,
                to,
                withAirplane
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/move", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var moveResponse = JsonSerializer.Deserialize<MoveResponse>(responseBody);
                Logger.Log("GroundControlService", "INFO", $"Передвижение разрешено. Дистанция: {moveResponse!.Distance}.");
                return moveResponse.Distance;
            }
            else
            {
                Logger.Log("GroundControlService", "ERROR", $"Запрос на перемещение отклонен с ошибкой: {response.StatusCode}.");
                return -1;
            }
        }

        public async Task NotifyArrival(string vehicleId, string vehicleType, string nodeId)
        {
            Logger.Log("GroundControlService", "INFO", $"Отправка уведобления о прибытиии машины {vehicleId} в узел {nodeId}.");

            var request = new
            {
                vehicleId,
                vehicleType,
                nodeId
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/arrived", content);
            response.EnsureSuccessStatusCode();
            Logger.Log("GroundControlService", "INFO", "Машина доставлена.");
        }
    }
}