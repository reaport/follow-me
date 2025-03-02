using Microsoft.AspNetCore.Mvc;
using FollowMe.Services;
using FollowMe.Data;
using FollowMe.Utils;
using System.Text.Json;
using System.Threading.Tasks;
using FollowMe.Utils;

namespace FollowMe.Controllers
{
    [ApiController]
    public class FollowMeController : ControllerBase
    {
        private readonly string GarageNode = "garage-node";

        private readonly IGroundControlService _groundControlService;
        private readonly IOrchestratorService _orchestratorService;
        private readonly CarRepository _carRepository;

        public FollowMeController(
            IGroundControlService groundControlService,
            IOrchestratorService orchestratorService,
            CarRepository carRepository)
        {
            _groundControlService = groundControlService;
            _orchestratorService = orchestratorService;
            _carRepository = carRepository;
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

            // Получаем список машин из файла
            var cars = _carRepository.GetAllCars();

            // Поиск доступной машины
            var car = cars.FirstOrDefault(c => c.Status == CarStatusEnum.Available);

            // Если все машины заняты, ждем первую освободившуюся
            var timeToWait = false;
            if (car == null)
            {
                Logger.Log("FollowMeController", "INFO", "Все машины заняты. Ожидание освобождения.");
                timeToWait = true;

                // Ожидаем первую машину, которая вернется в гараж
                car = await WaitForAvailableCarAsync(cars);
                if (car == null)
                {
                    Logger.Log("FollowMeController", "ERROR", "Нет доступных машин.");
                    return StatusCode(500, new ErrorResponseDto { ErrorCode = 500, Message = "Нет доступных машин." });
                }
            }

            // Помечаем машину как занятую
            car.Status = CarStatusEnum.Busy;

            // Регистрируем транспорт
            var registrationResponse = await _groundControlService.RegisterVehicle("follow-me");

            // Проверяем формат VehicleId
            if (!string.IsNullOrEmpty(registrationResponse.VehicleId))
            {
                car.ExternalId = registrationResponse.VehicleId;
            }
            else
            {
                Logger.Log("FollowMeController", "ERROR", $"Неверный формат VehicleId: {registrationResponse.VehicleId}");
                return await HandleInvalidVehicleId(request);
            }

            Logger.Log("FollowMeController", "INFO", $"Машина зарегистрирована. Внешний ID: {car.ExternalId}");

            // Сохраняем обновленные данные о машине в файл
            _carRepository.SaveAllCars(cars);

            // Отправляем OK с данными о машине и временем ожидания
            var response = new { CarId = car.ExternalId, TimeToWait = timeToWait };
            Logger.Log("FollowMeController", "INFO", $"Ответ отправлен: {JsonSerializer.Serialize(response)}");

            // Запускаем асинхронную задачу для обработки маршрута
            _ = ProcessRouteAsync(car.ExternalId, "follow-me", request.NodeFrom.ToString(), request.NodeTo.ToString(), GarageNode);

            return Ok(response);
        }

        private async Task<Car> WaitForAvailableCarAsync(List<Car> cars)
        {
            // Ожидаем, пока одна из машин не станет доступной
            while (true)
            {
                var car = cars.FirstOrDefault(c => c.Status == CarStatusEnum.Available);
                if (car != null)
                {
                    return car;
                }

                await Task.Delay(TimeSpan.FromSeconds(5)); // Проверяем каждые 5 секунд
            }
        }

        private async Task ProcessRouteAsync(string vehicleId, string vehicleType, string nodeFrom, string nodeTo, string garrageNodeId)
        {
            try
            {
                // Логируем начало движения
                Logger.LogAudit(vehicleId, $"Начало движения из {nodeFrom} в {nodeTo}.");

                // Отправляем запрос на начало движения в Orchestrator
                await _orchestratorService.StartMovementAsync(vehicleId);

                Logger.Log("FollowMeController", "INFO", $"Движение из гаража до NodeFrom {vehicleId}.");

                // Движение из гаража до NodeFrom
                await MoveBetweenNodesAsync(vehicleId, vehicleType, garrageNodeId, nodeFrom);

                Logger.Log("FollowMeController", "INFO", $"Движение из NodeFrom до NodeTo {vehicleId}.");

                // Движение из NodeFrom до NodeTo
                await MoveBetweenNodesAsync(vehicleId, vehicleType, nodeFrom, nodeTo);

                // Отправляем запрос на окончание движения в Orchestrator
                await _orchestratorService.EndMovementAsync(vehicleId);

                Logger.Log("FollowMeController", "INFO", $"Движение из NodeTo до гаража {vehicleId}.");

                // Движение из NodeTo до гаража
                await MoveBetweenNodesAsync(vehicleId, vehicleType, nodeTo, garrageNodeId);

                Logger.Log("FollowMeController", "INFO", $"Маршрут для машины {vehicleId} успешно завершен.");

                // Логируем завершение движения
                Logger.LogAudit(vehicleId, $"Завершение движения в {nodeTo}.");
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Logger.Log("FollowMeController", "ERROR", $"Ошибка при обработке маршрута: {ex.Message}");
            }
        }

        private async Task MoveBetweenNodesAsync(string vehicleId, string vehicleType, string from, string to)
        {
            Logger.Log("FollowMeController", "INFO", $"Начало обработки маршрута для машины {vehicleId}.");

            // Получаем маршрут от Вышки наземного движения
            var route = await _groundControlService.GetRoute(from, to);
            if (route == null || route.Length == 0)
            {
                Logger.Log("FollowMeController", "ERROR", "Маршрут не найден.");
            }

            Logger.Log("FollowMeController", "INFO", $"Маршрут получен: {string.Join(" -> ", route)}");

            // Движение из NodeFrom до NodeTo
            for (int i = 0; i < route.Length - 1; i++)
            {
                string fromNode = route[i];
                string toNode = route[i + 1];

                Logger.Log("FollowMeController", "INFO", $"Обработка перемещения из {fromNode} в {toNode}.");

                bool movementSuccessful = await ProcessMovementAsync(vehicleId, vehicleType, fromNode, toNode);
                if (!movementSuccessful)
                {
                    Logger.Log("FollowMeController", "WARNING", $"Перемещение из {fromNode} в {toNode} не удалось. Повторная попытка.");
                    i--; // Повторяем текущий шаг
                }
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
                    var cars = _carRepository.GetAllCars();
                    var car = cars.FirstOrDefault(c => c.Status == CarStatusEnum.Busy);
                    if (car != null)
                    {
                        car.ExternalId = registrationResponse.VehicleId;
                        _carRepository.SaveAllCars(cars); // Сохраняем изменения в файл
                        return Ok(new { CarId = car.ExternalId, Message = "Машина успешно зарегистрирована после повторной попытки." });
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