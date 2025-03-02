//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using FollowMe.Controllers;
//using FollowMe.Data;
//using FollowMe.Services;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using Xunit;
//using FollowMe.Extencion;

//namespace FollowMe.Tests
//{
//    public class FollowMeControllerTests
//    {
//        private readonly Mock<IGroundControlService> _mockGroundControlService;
//        private readonly Mock<IOrchestratorService> _mockOrchestratorService;
//        private readonly FollowMeController _controller;

//        public FollowMeControllerTests()
//        {
//            _mockGroundControlService = new Mock<IGroundControlService>();
//            _mockOrchestratorService = new Mock<IOrchestratorService>();
//            _controller = new FollowMeController(_mockGroundControlService.Object, _mockOrchestratorService.Object);
//        }

//        [Fact(DisplayName = "Неверный AirplaneId - BadRequest")]
//        public async Task GetCar_InvalidAirplaneId_ReturnsBadRequest()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.Empty,
//                NodeFrom = "node_1",
//                NodeTo = "node_2"
//            };

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//            Assert.NotNull(badRequestResult.Value);
//        }

//        [Fact(DisplayName = "Нет маршрута - InternalServerError")]
//        public async Task GetCar_NoRoute_ReturnsInternalServerError()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.NewGuid(),
//                NodeFrom = "node_1",
//                NodeTo = "node_2"
//            };

//            _mockGroundControlService
//                .Setup(x => x.GetRoute(It.IsAny<string>(), It.IsAny<string>()))
//                .ReturnsAsync(Array.Empty<string>());

//            _mockGroundControlService
//                .Setup(x => x.RegisterVehicle(It.IsAny<string>()))
//                .ReturnsAsync(new VehicleRegistrationResponse { VehicleId = "car_123" });

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var statusCodeResult = Assert.IsType<ObjectResult>(result);
//            Assert.Equal(500, statusCodeResult.StatusCode);
//        }

//        [Fact(DisplayName = "Попытка запроса без данных - BadRequest")]
//        public async Task GetCar_EmptyRequest_ReturnsBadRequest()
//        {
//            // Act
//            var result = await _controller.GetCar(null);

//            // Assert
//            Assert.IsType<BadRequestResult>(result);
//        }

//        [Fact(DisplayName = "Валидный запрос - машина доступна - OK")]
//        public async Task GetCar_ValidRequest_CarAvailable_ReturnsOk()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.NewGuid(),
//                NodeFrom = "node_1",
//                NodeTo = "node_2"
//            };

//            var availableCar = new Car { Id = "test_car", Status = CarStatusEnum.Available };
//            ReflectionExtensions.SetCars(new List<Car> { availableCar });

//            _mockGroundControlService
//                .Setup(x => x.RegisterVehicle(It.IsAny<string>()))
//                .ReturnsAsync(new VehicleRegistrationResponse { VehicleId = "car_123" });

//            _mockGroundControlService
//                .Setup(x => x.GetRoute(It.IsAny<string>(), It.IsAny<string>()))
//                .ReturnsAsync(new[] { "node_1", "node_2" });

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            dynamic response = okResult.Value;
//            Assert.NotNull(response);
//            Assert.Equal("car_123", response.CarId);
//            Assert.False(response.TimeToWait);
//        }

//        [Fact(DisplayName = "Все машины заняты, но одна освобождается - OK")]
//        public async Task GetCar_AllCarsBusy_OneBecomesAvailable_ReturnsOk()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.NewGuid(),
//                NodeFrom = "node_1",
//                NodeTo = "node_2"
//            };

//            ReflectionExtensions.SetCars(new List<Car>
//            {
//                new Car { Id = "1", Status = CarStatusEnum.Busy },
//                new Car { Id = "2", Status = CarStatusEnum.Busy },
//                new Car { Id = "3", Status = CarStatusEnum.Busy },
//                new Car { Id = "4", Status = CarStatusEnum.Busy }
//            });

//            _mockGroundControlService
//                .Setup(x => x.RegisterVehicle(It.IsAny<string>()))
//                .ReturnsAsync(new VehicleRegistrationResponse { VehicleId = "car_456" });

//            _mockGroundControlService
//                .Setup(x => x.GetRoute(It.IsAny<string>(), It.IsAny<string>()))
//                .ReturnsAsync(new[] { "node_1", "node_2" });

//            // Симулируем освобождение машины через 1 секунду
//            Task.Run(async () =>
//            {
//                await Task.Delay(1000); // Задержка в 1 секунду для симуляции
//                ReflectionExtensions.SetCars(new List<Car> { new Car { Id = "5", Status = CarStatusEnum.Available } });
//            });

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            dynamic response = okResult.Value;
//            Assert.NotNull(response);
//            Assert.Equal("car_456", response.CarId);
//            Assert.False(response.TimeToWait);
//        }

//        [Fact(DisplayName = "Ошибка регистрации машины - InternalServerError")]
//        public async Task GetCar_RegistrationFailed_ReturnsInternalServerError()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.NewGuid(),
//                NodeFrom = "node_1",
//                NodeTo = "node_2"
//            };

//            _mockGroundControlService
//                .Setup(x => x.RegisterVehicle(It.IsAny<string>()))
//                .ReturnsAsync(new VehicleRegistrationResponse { VehicleId = null });

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var statusCodeResult = Assert.IsType<ObjectResult>(result);
//            Assert.Equal(500, statusCodeResult.StatusCode);
//        }

//        [Fact(DisplayName = "Маршрут успешно обработан - OK")]
//        public async Task GetCar_RouteProcessedSuccessfully_ReturnsOk()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.NewGuid(),
//                NodeFrom = "node_1",
//                NodeTo = "node_2"
//            };

//            var availableCar = new Car { Id = "car_789", Status = CarStatusEnum.Available };
//            ReflectionExtensions.SetCars(new List<Car> { availableCar });

//            _mockGroundControlService
//                .Setup(x => x.RegisterVehicle(It.IsAny<string>()))
//                .ReturnsAsync(new VehicleRegistrationResponse { VehicleId = "car_789" });

//            _mockGroundControlService
//                .Setup(x => x.GetRoute(It.IsAny<string>(), It.IsAny<string>()))
//                .ReturnsAsync(new[] { "node_1", "node_2", "garage-node" });

//            _mockOrchestratorService
//                .Setup(x => x.StartMovementAsync(It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            _mockOrchestratorService
//                .Setup(x => x.EndMovementAsync(It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            dynamic response = okResult.Value;
//            Assert.NotNull(response);
//            Assert.Equal("car_789", response.CarId);
//            Assert.False(response.TimeToWait);
//        }
//    }
//}