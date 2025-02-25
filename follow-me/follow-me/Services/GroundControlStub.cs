﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FollowMe.Utils;

namespace FollowMe.Services
{
    public class GroundControlStubService : IGroundControlService
    {
        public Task<VehicleRegistrationResponse> RegisterVehicle(string vehicleType)
        {
            Logger.Log("GroundControlStubService", "INFO", $"Регистрация транспорта типа {vehicleType}.");

            var response = new VehicleRegistrationResponse
            {
                GarrageNodeId = "garrage_follow-me_1",
                VehicleId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
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

            var route = new[] { "node_1", "node_2", "node_3" };
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