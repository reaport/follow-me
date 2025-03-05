using System.Collections.Generic;
using System.Threading.Tasks;
using FollowMe.Utils;

namespace FollowMe.Services
{
    public class GroundControlStubService : IGroundControlService
    {
        public Task<VehicleRegistrationResponse> RegisterVehicle(string vehicleType)
        {
            Logger.Log("GroundControlStubService", "INFO", $"Регистрация транспорта типа {vehicleType}.");

            var vehicleId = Guid.NewGuid().ToString();

            var response = new VehicleRegistrationResponse
            {
                GarrageNodeId = "garrage_follow-me_1",
                VehicleId = vehicleId,
                ServiceSpots = new Dictionary<string, string>
                {
                    { "airplane_parking_1", "airplane_parking_1_follow-me_1" },
                    { "airplane_parking_2", "airplane_parking_2_follow-me_1" }
                }
            };

            Logger.Log("GroundControlStub", "INFO", $"Заглушка: Зарегистрирован транспорт с ID {response.VehicleId}.");
            return Task.FromResult(response);
        }

        public Task<string[]> GetRoute(string from, string to)
        {
            Logger.Log("GroundControlStub", "INFO", $"Запрос маршрута из {from} в {to}.");

            string[] route;
            if (from == "garage-node")
                route = new[]{ "garage-node", "node_1", "from-node" };
            else if (to == "garage-node")
                route = new[] { "to-node", "node_3", "garage-node" };
            else
                route = new[] { "from-node", "node_2", "to-node" };
            Logger.Log("GroundControlStub", "INFO", $"Заглушка: Маршрут: {string.Join(" -> ", route)}.");
            return Task.FromResult(route);
        }

        public Task<double> RequestMove(string vehicleId, string vehicleType, string from, string to)
        {
            Logger.Log("GroundControlStub", "INFO", $"Запрос на перемещение транспорта {vehicleId} из {from} в {to}.");

            double distance = 100.0; // Примерное расстояние
            Logger.Log("GroundControlStub", "INFO", $"Заглушка: Разрешение на перемещение получено. Расстояние: {distance}.");
            return Task.FromResult(distance);
        }

        public Task NotifyArrival(string vehicleId, string vehicleType, string nodeId)
        {
            Logger.Log("GroundControlStub", "INFO", $"Уведомление о прибытии транспорта {vehicleId} в узел {nodeId}.");
            Logger.Log("GroundControlStub", "INFO", "Заглушка: Уведомление успешно отправлено.");
            return Task.CompletedTask;
        }

        public Task SendNavigationSignal(string vehicleId, string signal)
        {
            Logger.Log("GroundControlStub", "INFO", $"Отправка сигнала навигации {signal} для транспорта {vehicleId}.");
            Logger.Log("GroundControlStub", "INFO", "Заглушка: Сигнал навигации успешно отправлен.");
            return Task.CompletedTask;
        }
    }
}