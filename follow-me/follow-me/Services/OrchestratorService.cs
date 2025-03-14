using System.Text;
using System.Text.Json;
using FollowMe.Utils;

namespace FollowMe.Services
{
    /// <summary>
    /// Реализация сервиса оркестрации для управления движением машины.
    /// </summary>
    public class OrchestratorService : IOrchestratorService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="OrchestratorService"/>.
        /// </summary>
        /// <param name="httpClient">HTTP-клиент для выполнения запросов.</param>
        public OrchestratorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Инициирует начало движения машины с указанным идентификатором машины и самолета.
        /// </summary>
        /// <param name="carId">Идентификатор машины.</param>
        /// <param name="aircraftId">Идентификатор самолета.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task StartMovementAsync(string carId, string aircraftId)
        {
            Logger.Log("OrchestratorService", "INFO", $"Отправка запроса на начало движения для машины {carId}.");

            var requestBody = new { aircraft_id = aircraftId };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"/followme/start", jsonContent);
            response.EnsureSuccessStatusCode();

            Logger.Log("OrchestratorService", "INFO", $"Запрос на начало движения для машины {carId} успешно отправлен.");
        }

        /// <summary>
        /// Завершает движение машины с указанным идентификатором машины, самолета и флагом взлета.
        /// </summary>
        /// <param name="carId">Идентификатор машины.</param>
        /// <param name="aircraftId">Идентификатор самолета.</param>
        /// <param name="isTakeoff">Флаг, указывающий, связан ли запрос с взлетом.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task EndMovementAsync(string carId, string aircraftId, bool isTakeoff)
        {
            Logger.Log("OrchestratorService", "INFO", $"Отправка запроса на окончание движения для машины {carId}.");

            var requestBody = new { aircraft_id = aircraftId, is_takeoff = isTakeoff };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"/followme/finish", jsonContent);
            response.EnsureSuccessStatusCode();

            Logger.Log("OrchestratorService", "INFO", $"Запрос на окончание движения для машины {carId} успешно отправлен.");
        }
    }
}