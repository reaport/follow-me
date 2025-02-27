using Microsoft.AspNetCore.Mvc;
using FollowMe.Services;
using FollowMe.Data;
using FollowMe.Utils;
using System.Text.Json;

namespace FollowMe.Controllers
{
    [ApiController]
    public class FollowMeController : ControllerBase
    {
        private static List<Car> cars = new List<Car>
        {
            new Car { Id = "", Status = CarStatusEnum.Available, AccompanimentsCount = 0 },
            new Car { Id = "", Status = CarStatusEnum.Available, AccompanimentsCount = 0 },
            new Car { Id = "", Status = CarStatusEnum.Available, AccompanimentsCount = 0 },
            new Car { Id = "", Status = CarStatusEnum.Available, AccompanimentsCount = 0 }
        };

        private readonly IGroundControlService _groundControlService;

        public FollowMeController(IGroundControlService groundControlService)
        {
            _groundControlService = groundControlService;
        }

        [HttpPost("get_car")]
        public async Task<IActionResult> GetCar([FromBody] WeNeedFollowMeRequestDto request)
        {
            Logger.Log("FollowMeController", "INFO", "Запрос на получение машины сопровождения.");

            if (!ModelState.IsValid)
            {
                Logger.Log("FollowMeController", "ERROR", "Неверный формат AirplaneId.");
                return BadRequest(new ErrorResponseDto { ErrorCode = 10, Message = "Invalid AirplaneId" });
            }

            if (request.FollowType < 1 || request.FollowType > 2)
            {
                Logger.Log("FollowMeController", "ERROR", "Неверный тип FollowType.");
                return BadRequest(new ErrorResponseDto { ErrorCode = 21, Message = "Wrong FollowType. It should be [1, 2]" });
            }

            var car = cars.FirstOrDefault(c => c.Status == CarStatusEnum.Available);
            if (car == null)
            {
                car = cars.FirstOrDefault(c => c.Status == CarStatusEnum.InGarage && c.AccompanimentsCount < 5);
                if (car == null)
                {
                    Logger.Log("FollowMeController", "ERROR", "Нет доступных машин.");
                    return StatusCode(500, new ErrorResponseDto { ErrorCode = 500, Message = "Нет доступных машин." });
                }
            }

            car.Status = CarStatusEnum.Busy;
            car.AccompanimentsCount++;

            double timeToWait = 10; // Default time to wait
            if (car.Status == CarStatusEnum.InGarage)
            {
                timeToWait += 8; // Additional time for refueling
            }

            // Регистрируем транспорт
            var registrationResponse = await _groundControlService.RegisterVehicle("follow-me");

            // Проверяем формат VehicleId
            if (!string.IsNullOrEmpty(registrationResponse.VehicleId))
            {
                car.Id = registrationResponse.VehicleId;
            }
            else
            {
                Logger.Log("FollowMeController", "ERROR", $"Неверный формат VehicleId: {registrationResponse.VehicleId}");
                return await HandleInvalidVehicleId(request);
            }

            Logger.Log("FollowMeController", "INFO", $"Машина зарегистрирована. ID: {car.Id}");

            // Получаем маршрут от Вышки наземного движения
            var route = await _groundControlService.GetRoute(request.GateNumber.ToString(), request.RunawayNumber.ToString());
            if (route == null || route.Length == 0)
            {
                Logger.Log("FollowMeController", "ERROR", "Маршрут не найден.");
                return StatusCode(404, new ErrorResponseDto { ErrorCode = 404, Message = "Route not found" });
            }

            Logger.Log("FollowMeController", "INFO", $"Маршрут получен: {string.Join(" -> ", route)}");

            // Отправляем OK с данными о машине и временем ожидания
            var response = new { CarId = car.Id, TimeToWait = timeToWait };
            Logger.Log("FollowMeController", "INFO", $"Ответ отправлен: {JsonSerializer.Serialize(response)}");

            // Запускаем асинхронную задачу для обработки маршрута
            _ = ProcessRouteAsync(car.Id.ToString(), "follow-me", route);

            return Ok(response);
        }

        private async Task ProcessRouteAsync(string vehicleId, string vehicleType, string[] route)
        {
            try
            {
                Logger.Log("FollowMeController", "INFO", $"Начало обработки маршрута для машины {vehicleId}.");

                for (int i = 0; i < route.Length - 1; i++)
                {
                    string from = route[i];
                    string to = route[i + 1];

                    Logger.Log("FollowMeController", "INFO", $"Обработка перемещения из {from} в {to}.");

                    bool movementSuccessful = await ProcessMovementAsync(vehicleId, vehicleType, from, to);
                    if (!movementSuccessful)
                    {
                        Logger.Log("FollowMeController", "WARNING", $"Перемещение из {from} в {to} не удалось. Повторная попытка.");
                        i--; // Повторяем текущий шаг
                    }
                }

                Logger.Log("FollowMeController", "INFO", $"Уведомление о прибытии в узел {route.Last()}.");

                // Уведомляем о прибытии в конечную точку
                await _groundControlService.NotifyArrival(vehicleId, vehicleType, route.Last());

                Logger.Log("FollowMeController", "INFO", $"Маршрут для машины {vehicleId} успешно завершен.");
            }
            catch (Exception ex)
            {
                Logger.Log("FollowMeController", "ERROR", $"Ошибка при обработке маршрута: {ex.Message}");
            }
        }

        private async Task<bool> ProcessMovementAsync(string vehicleId, string vehicleType, string from, string to)
        {
            Logger.Log("FollowMeController", "INFO", $"Запрос на перемещение из {from} в {to}.");

            // Запрашиваем разрешение на движение
            double distance = await _groundControlService.RequestMove(vehicleId, vehicleType, from, to);

            if (distance > 0)
            {
                Logger.Log("FollowMeController", "INFO", $"Разрешение получено. Расстояние: {distance}.");

                // Если разрешение получено, отправляем сигналы навигации
                await _groundControlService.SendNavigationSignal(vehicleId, "follow");

                // Имитируем движение (например, задержку)
                await Task.Delay(TimeSpan.FromSeconds(distance / 10)); // Примерная задержка

                return true; // Перемещение успешно
            }
            else
            {
                Logger.Log("FollowMeController", "WARNING", "Разрешение на движение не получено. Повторная попытка через 5 секунд.");

                // Если разрешение не получено, ждем и повторяем запрос
                await Task.Delay(TimeSpan.FromSeconds(5));
                return false; // Перемещение не удалось
            }
        }

        private async Task<IActionResult> HandleInvalidVehicleId(WeNeedFollowMeRequestDto request)
        {
            const int maxAttempts = 3;
            const int delaySeconds = 30;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                // Логика для обработки невалидного VehicleId
                Logger.Log("FollowMeController", "WARNING", "Попытка повторной регистрации машины...");

                // Повторный запрос
                var registrationResponse = await _groundControlService.RegisterVehicle("follow-me");

                if (!string.IsNullOrEmpty(registrationResponse.VehicleId))
                {
                    // Если получили валидный VehicleId, то возвращаем успешный ответ
                    var car = cars.FirstOrDefault(c => c.Status == CarStatusEnum.Busy);
                    if (car != null)
                    {
                        car.Id = registrationResponse.VehicleId;
                        return Ok(new { CarId = car.Id, Message = "Машина успешно зарегистрирована после повторной попытки." });
                    }
                }

                // Если после повторного запроса VehicleId все еще невалиден, возвращаем ошибку
                Logger.Log("FollowMeController", "ERROR", "Не удалось зарегистрировать машину после повторной попытки.");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds)); // Задержка между попытками
            }
            return StatusCode(500, new ErrorResponseDto { ErrorCode = 500, Message = "Не удалось зарегистрировать машину." });
        }

    }
}