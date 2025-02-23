using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using FollowMe.Data;
using FollowMe.Services;

namespace FollowMe.Controllers
{
    [ApiController]
    public class FollowMeController : ControllerBase
    {
        private static List<Car> cars = new List<Car>
        {
            new Car { Id = Guid.NewGuid(), Status = CarStatusEnum.Available, AccompanimentsCount = 0 },
            new Car { Id = Guid.NewGuid(), Status = CarStatusEnum.Available, AccompanimentsCount = 0 },
            new Car { Id = Guid.NewGuid(), Status = CarStatusEnum.Available, AccompanimentsCount = 0 },
            new Car { Id = Guid.NewGuid(), Status = CarStatusEnum.Available, AccompanimentsCount = 0 },
            new Car { Id = Guid.NewGuid(), Status = CarStatusEnum.Available, AccompanimentsCount = 0 },
            new Car { Id = Guid.NewGuid(), Status = CarStatusEnum.InGarage, AccompanimentsCount = 0 }
        };

        private readonly GroundControlService _groundControlService;

        public FollowMeController(GroundControlService groundControlService)
        {
            _groundControlService = groundControlService;
        }

        [HttpPost("get_car")]
        public async Task<IActionResult> GetCar([FromBody] WeNeedFollowMeRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto{ ErrorCode = 10, Message = "Invalid AirplaneId" });
            }

            if (request.FollowType < 1 || request.FollowType > 2)
            {
                return BadRequest(new ErrorResponseDto { ErrorCode = 21, Message = "Wrong FollowType. It should be [1, 2]" });
            }

            var car = cars.FirstOrDefault(c => c.Status == CarStatusEnum.Available);
            if (car == null)
            {
                car = cars.FirstOrDefault(c => c.Status == CarStatusEnum.InGarage && c.AccompanimentsCount < 5);
                if (car == null)
                {
                    return StatusCode(500, new ErrorResponseDto { ErrorCode = 500, Message = "InternalServerError" });
                }
            }

            car.Status = CarStatusEnum.Busy;
            car.AccompanimentsCount++;

            double timeToWait = 10; // Default time to wait
            if (car.Status == CarStatusEnum.InGarage)
            {
                timeToWait += 8; // Additional time for refueling
            }

            // Получаем маршрут от Вышки наземного движения
            var route = await _groundControlService.GetRoute(request.GateNumber.ToString(), request.RunawayNumber.ToString());
            if (route == null || route.Length == 0)
            {
                return StatusCode(404, new ErrorResponseDto { ErrorCode = 404, Message = "Route not found" });
            }

            // Выполняем движение по маршруту
            await MoveAlongRoute(car.Id, route);

            return Ok(new { CarId = car.Id, TimeToWait = timeToWait });
        }

        private async Task MoveAlongRoute(Guid carId, string[] route)
        {
            for (int i = 0; i < route.Length - 1; i++)
            {
                string from = route[i];
                string to = route[i + 1];

                // Запрашиваем разрешение на движение
                double distance = await _groundControlService.RequestMove(carId.ToString(), from, to);

                if (distance > 0)
                {
                    // Если разрешение получено, отправляем сигналы навигации
                    await _groundControlService.SendNavigationSignal(carId.ToString(), "follow");

                    // Имитируем движение (например, задержку)
                    await Task.Delay(TimeSpan.FromSeconds(distance / 10)); // Примерная задержка
                }
                else
                {
                    // Если разрешение не получено, ждем и повторяем запрос
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    i--; // Повторяем текущий шаг
                }
            }

            // Уведомляем о прибытии в конечную точку
            await _groundControlService.NotifyArrival(carId.ToString(), route.Last());
        }
    }
}