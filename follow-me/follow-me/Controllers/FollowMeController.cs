using Microsoft.AspNetCore.Mvc;
using FollowMe.Services;
using FollowMe.Data;
using FollowMe.Utils;
using System.Text.Json;
using System.Threading.Tasks;

namespace FollowMe.Controllers
{
    [ApiController]
    public class FollowMeController : ControllerBase
    {
        private readonly IGroundControlService _groundControlService;
        private readonly IOrchestratorService _orchestratorService;
        private readonly CarRepository _carRepository;

        /// <summary>
        /// Конструктор контроллера. Внедряет зависимости через DI.
        /// </summary>
        /// <param name="groundControlService">Сервис для взаимодействия с системой управления.</param>
        /// <param name="orchestratorService">Сервис для взаимодействия с оркестратором.</param>
        /// <param name="carRepository">Репозиторий для работы с машинами.</param>
        public FollowMeController(
            IGroundControlService groundControlService,
            IOrchestratorService orchestratorService,
            CarRepository carRepository)
        {
            _groundControlService = groundControlService;
            _orchestratorService = orchestratorService;
            _carRepository = carRepository;
        }

        /// <summary>
        /// Обрабатывает запрос на получение машины сопровождения.
        /// </summary>
        /// <param name="request">Данные запроса, включая AirplaneId, NodeFrom, NodeTo и IsTakeoff.</param>
        /// <returns>Данные о машине и времени ожидания или ошибку.</returns>
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

                // Возвращаем ответ сразу, не дожидаясь освобождения машины
                car = cars.FirstOrDefault();
                var immediateResponse = new { CarId = car!.ExternalId, TimeToWait = timeToWait };
                Logger.Log("FollowMeController", "INFO", $"Ответ отправлен: {JsonSerializer.Serialize(immediateResponse)}");
                return Ok(immediateResponse);
            }

            // Ожидаем первую машину, которая вернется в гараж
            car = await WaitForAvailableCarAsync(cars);
            if (car == null)
            {
                Logger.Log("FollowMeController", "ERROR", "Нет доступных машин.");
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 500, Message = "Нет доступных машин." });
            }

            // Помечаем машину как занятую
            car.Status = CarStatusEnum.Busy;

            // Сохраняем обновленные данные о машине в файл
            _carRepository.SaveAllCars(cars);

            // Отправляем OK с данными о машине и временем ожидания
            var response = new { CarId = car.ExternalId, TimeToWait = timeToWait };
            Logger.Log("FollowMeController", "INFO", $"Ответ отправлен: {JsonSerializer.Serialize(response)}");

            // Запускаем асинхронную задачу для обработки маршрута
            _ = ProcessRouteAsync(car.ExternalId, "follow-me", request.NodeFrom.ToString(), request.NodeTo.ToString(), car.CurrentNode, request.AirplaneId.ToString(), request.IsTakeoff);

            return Ok(response);
        }

        /// <summary>
        /// Ожидает, пока одна из машин не станет доступной.
        /// </summary>
        /// <param name="cars">Список машин.</param>
        /// <returns>Доступная машина или null, если все машины заняты.</returns>
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

                // Проверяем каждые 5 секунд
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// Обрабатывает маршрут для машины сопровождения.
        /// </summary>
        /// <param name="vehicleId">Идентификатор машины.</param>
        /// <param name="vehicleType">Тип машины.</param>
        /// <param name="nodeFrom">Начальный узел.</param>
        /// <param name="nodeTo">Конечный узел.</param>
        /// <param name="garrageNodeId">Идентификатор гаража.</param>
        /// <param name="aircraftId">Идентификатор самолета.</param>
        /// <param name="isTakeoff">Флаг, указывающий, является ли движение взлетом.</param>
        private async Task ProcessRouteAsync(string vehicleId, string vehicleType, string nodeFrom, string nodeTo, string garrageNodeId, string aircraftId, bool isTakeoff)
        {
            try
            {
                Logger.Log("FollowMeController", "INFO", $"Движение из гаража до NodeFrom {vehicleId}.");

                // Движение из гаража до NodeFrom
                await MoveBetweenNodesAsync(vehicleId, vehicleType, garrageNodeId, nodeFrom, "");

                Logger.LogAudit(vehicleId, $"Начало движения из {nodeFrom} в {nodeTo}.");

                // Отправляем запрос на начало движения в Orchestrator с aircraftId
                await RetryAsync(async () =>
                {
                    await _orchestratorService.StartMovementAsync(vehicleId, aircraftId);
                });

                Logger.Log("FollowMeController", "INFO", $"Движение из NodeFrom до NodeTo {vehicleId}.");

                // Движение из NodeFrom до NodeTo
                await MoveBetweenNodesAsync(vehicleId, vehicleType, nodeFrom, nodeTo, aircraftId);

                // Отправляем запрос на окончание движения в Orchestrator с aircraftId и isTakeoff
                await RetryAsync(async () =>
                {
                    await _orchestratorService.EndMovementAsync(vehicleId, aircraftId, isTakeoff, nodeTo);
                });

                Logger.Log("FollowMeController", "INFO", $"Движение из NodeTo до гаража {vehicleId}.");

                // Движение из NodeTo до гаража
                await MoveBetweenNodesAsync(vehicleId, vehicleType, nodeTo, garrageNodeId, "");

                Logger.Log("FollowMeController", "INFO", $"Маршрут для машины {vehicleId} успешно завершен.");


                Logger.LogAudit(vehicleId, $"Завершение движения в {nodeTo}.");

                // Возвращаем машину в гараж и сбрасываем её состояние
                var cars = _carRepository.GetAllCars();
                var car = cars.FirstOrDefault(c => c.ExternalId == vehicleId);
                if (car != null)
                {
                    car.Status = CarStatusEnum.Available;
                    car.CurrentNode = garrageNodeId;
                    _carRepository.SaveAllCars(cars);
                    Logger.Log("FollowMeController", "INFO", $"Машина {vehicleId} возвращена в гараж и доступна для новых задач.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("FollowMeController", "ERROR", $"Ошибка при обработке маршрута: {ex.Message}");
            }
        }

        /// <summary>
        /// Обрабатывает перемещение машины между узлами.
        /// </summary>
        /// <param name="vehicleId">Идентификатор машины.</param>
        /// <param name="vehicleType">Тип машины.</param>
        /// <param name="from">Начальный узел.</param>
        /// <param name="to">Конечный узел.</param>
        /// <param name="withAirplane">Идентификатор самолета.</param>
        private async Task MoveBetweenNodesAsync(string vehicleId, string vehicleType, string from, string to, string withAirplane)
        {
            Logger.Log("FollowMeController", "INFO", $"Начало обработки маршрута для машины {vehicleId}.");

            // Получаем маршрут от Вышки наземного движения
            var route = await _groundControlService.GetRoute(from, to);
            if (route == null || route.Length == 0)
            {
                Logger.Log("FollowMeController", "ERROR", "Маршрут не найден.");
            }

            Logger.Log("FollowMeController", "INFO", $"Маршрут получен: {string.Join(" -> ", route!)}");

            // Движение из NodeFrom до NodeTo
            for (int i = 0; i < route!.Length - 1; i++)
            {
                string fromNode = route[i];
                string toNode = route[i + 1];

                Logger.Log("FollowMeController", "INFO", $"Обработка перемещения из {fromNode} в {toNode}.");

                bool movementSuccessful = await ProcessMovementAsync(vehicleId, vehicleType, fromNode, toNode, withAirplane);
                if (!movementSuccessful)
                {
                    Logger.Log("FollowMeController", "WARNING", $"Перемещение из {fromNode} в {toNode} не удалось. Повторная попытка.");
                    i--;
                }
                else
                {
                    // Обновляем текущее местоположение машины
                    var cars = _carRepository.GetAllCars();
                    var car = cars.FirstOrDefault(c => c.ExternalId == vehicleId);
                    if (car != null)
                    {
                        car.CurrentNode = toNode;
                        _carRepository.SaveAllCars(cars);
                    }
                }
            }
        }

        /// <summary>
        /// Обрабатывает перемещение машины между двумя узлами.
        /// </summary>
        /// <param name="vehicleId">Идентификатор машины.</param>
        /// <param name="vehicleType">Тип машины.</param>
        /// <param name="from">Начальный узел.</param>
        /// <param name="to">Конечный узел.</param>
        /// <param name="withAirplane">Идентификатор самолета.</param>
        /// <returns>True, если перемещение успешно, иначе False.</returns>
        private async Task<bool> ProcessMovementAsync(string vehicleId, string vehicleType, string from, string to, string withAirplane)
        {
            Logger.Log("FollowMeController", "INFO", $"Запрос на перемещение из {from} в {to}.");

            // Запрашиваем разрешение на движение
            double distance = await _groundControlService.RequestMove(vehicleId, vehicleType, from, to, withAirplane);

            if (distance > 0)
            {
                Logger.Log("FollowMeController", "INFO", $"Разрешение получено. Расстояние: {distance}.");

                // Рассчитываем время ожидания в зависимости от расстояния. Скорость 25 ед/с
                int delayMilliseconds = (int)(distance / 25 * 1000);
                Logger.Log("FollowMeController", "INFO", $"Машина {vehicleId} движется из {from} в {to}. Время в пути: {delayMilliseconds / 1000} сек.");

                await Task.Delay(delayMilliseconds);

                return true;
            }
            else
            {
                Logger.Log("FollowMeController", "WARNING", "Разрешение на движение не получено. Повторная попытка через 5 секунд.");

                // Если разрешение не получено, ждем и повторяем запрос
                await Task.Delay(TimeSpan.FromSeconds(5));
                return false;
            }
        }

        /// <summary>
        /// Повторяет выполнение асинхронной задачи в случае ошибки.
        /// </summary>
        /// <param name="action">Асинхронная задача.</param>
        /// <param name="maxAttempts">Максимальное количество попыток.</param>
        /// <param name="delaySeconds">Задержка между попытками в секундах.</param>
        private async Task RetryAsync(Func<Task> action, int maxAttempts = 3, int delaySeconds = 10)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts)
                    {
                        Logger.Log("FollowMeController", "ERROR", $"Ошибка после {maxAttempts} попыток: {ex.Message}");
                    }

                    Logger.Log("FollowMeController", "WARNING", $"Попытка {attempt} не удалась. Ошибка: {ex.Message}. Повтор через {delaySeconds} секунд.");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }
    }
}