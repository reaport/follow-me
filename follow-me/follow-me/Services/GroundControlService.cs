using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FollowMe.Utils;
using FollowMe.Data;

namespace FollowMe.Services
{
    /// <summary>
    /// Реализация сервиса для взаимодействия с системой управления наземным движением.
    /// </summary>
    public class GroundControlService : IGroundControlService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Конструктор сервиса. Внедряет HttpClient через DI.
        /// </summary>
        /// <param name="httpClient">HTTP-клиент.</param>
        public GroundControlService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Регистрирует транспортное средство в системе.
        /// </summary>
        /// <param name="vehicleType">Тип транспортного средства.</param>
        /// <returns>Ответ с данными о регистрации.</returns>
        public async Task<VehicleRegistrationResponse?> RegisterVehicle(string vehicleType)
        {
            Logger.Log("GroundControlService", "INFO", $"Регистрация машины типа {vehicleType}.");

            var response = await _httpClient.PostAsync($"/register-vehicle/{vehicleType}", null);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Logger.Log("GroundControlService", "INFO", $"Машина зарегистрированна. Ответ: {responseBody}.");
            return JsonSerializer.Deserialize<VehicleRegistrationResponse?>(responseBody);
        }

        /// <summary>
        /// Получает маршрут между двумя узлами.
        /// </summary>
        /// <param name="from">Начальный узел.</param>
        /// <param name="to">Конечный узел.</param>
        /// <returns>Массив узлов маршрута.</returns>
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

        /// <summary>
        /// Запрашивает разрешение на перемещение транспортного средства.
        /// </summary>
        /// <param name="vehicleId">Идентификатор транспортного средства.</param>
        /// <param name="vehicleType">Тип транспортного средства.</param>
        /// <param name="from">Начальный узел.</param>
        /// <param name="to">Конечный узел.</param>
        /// <param name="withAirplane">Идентификатор самолета (если требуется).</param>
        /// <returns>Расстояние для перемещения.</returns>
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

        /// <summary>
        /// Уведомляет систему о прибытии транспортного средства в узел.
        /// </summary>
        /// <param name="vehicleId">Идентификатор транспортного средства.</param>
        /// <param name="vehicleType">Тип транспортного средства.</param>
        /// <param name="nodeId">Идентификатор узла.</param>
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