using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FollowMe.Services
{
    public class GroundControlService
    {
        private readonly HttpClient _httpClient;

        public GroundControlService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string[]> GetRoute(string from, string to)
        {
            var request = new { from, to };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://ground-control.reaport.ru/route", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<string[]>(responseBody);
        }

        public async Task<double> RequestMove(string vehicleId, string from, string to)
        {
            var request = new { vehicleId, vehicleType = "follow-me", from, to };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://ground-control.reaport.ru/move", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var moveResponse = JsonSerializer.Deserialize<MoveResponse>(responseBody);
                return moveResponse.Distance;
            }
            else
            {
                return -1; // Ошибка или запрет на движение
            }
        }

        public async Task NotifyArrival(string vehicleId, string nodeId)
        {
            var request = new { vehicleId, vehicleType = "follow-me", nodeId };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://ground-control.reaport.ru/arrived", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendNavigationSignal(string vehicleId, string signal)
        {
            var request = new { Navigate = signal };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://ground-control.reaport.ru/navigate", content);
            response.EnsureSuccessStatusCode();
        }

        private class MoveResponse
        {
            public double Distance { get; set; }
        }
    }
}