//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using FollowMe.Controllers;
//using FollowMe.Data;
//using FollowMe.Services;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using Xunit;

//namespace FollowMe.Tests
//{
//    public class FollowMeControllerIntegrationTests
//    {
//        private readonly Mock<IGroundControlService> _mockGroundControlService;
//        private readonly FollowMeController _controller;

//        public FollowMeControllerIntegrationTests()
//        {
//            _mockGroundControlService = new Mock<IGroundControlService>();
//            _controller = new FollowMeController(_mockGroundControlService.Object);
//        }

//        [Fact]
//        public async Task GetCar_NoAvailableCars_ReturnsInternalServerError()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.NewGuid(),
//                FollowType = 1,
//                GateNumber = 1,
//                RunawayNumber = 1
//            };

//            // Все машины заняты
//            var cars = new List<Car>
//            {
//                new Car { Id = "", Status = CarStatusEnum.Busy, AccompanimentsCount = 0 },
//                new Car { Id = "", Status = CarStatusEnum.Busy, AccompanimentsCount = 0 }
//            };

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var statusCodeResult = Assert.IsType<ObjectResult>(result);
//            Assert.Equal(500, statusCodeResult.StatusCode);
//        }

//        [Fact(DisplayName = "Обработка невалидного VehicleId: успешная регистрация с третьей попытки")]
//        public async Task GetCar_InvalidVehicleId_SucceedsOnThirdAttempt()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.NewGuid(),
//                FollowType = 1,
//                GateNumber = 1,
//                RunawayNumber = 1
//            };

//            // Настраиваем имитацию метода, который будет возвращать null первые два раза, и валидный VehicleId на третьей попытке
//            _mockGroundControlService
//                .SetupSequence(x => x.RegisterVehicle(It.IsAny<string>()))
//                .ReturnsAsync(new VehicleRegistrationResponse { VehicleId = null }) // 1-я попытка
//                .ReturnsAsync(new VehicleRegistrationResponse { VehicleId = null }) // 2-я попытка
//                .ReturnsAsync(new VehicleRegistrationResponse { VehicleId = "valid-vehicle-id" }); // 3-я попытка

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var response = okResult.Value as dynamic;
//            Assert.Equal("valid-vehicle-id", response.CarId);
//            Assert.Equal("Машина успешно зарегистрирована после повторной попытки.", response.Message);
//        }

//        [Fact]
//        public async Task GetCar_RouteNotFound_ReturnsNotFound()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.NewGuid(),
//                FollowType = 1,
//                GateNumber = 1,
//                RunawayNumber = 1
//            };

//            _mockGroundControlService
//                .Setup(x => x.RegisterVehicle(It.IsAny<string>()))
//                .ReturnsAsync(new VehicleRegistrationResponse
//                {
//                    VehicleId = Guid.NewGuid().ToString()
//                });

//            _mockGroundControlService
//                .Setup(x => x.GetRoute(It.IsAny<string>(), It.IsAny<string>()))
//                .ReturnsAsync(Array.Empty<string>()); // Пустой маршрут

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//            var errorResponse = Assert.IsType<ErrorResponseDto>(notFoundResult.Value);
//            Assert.Equal(404, errorResponse.ErrorCode);
//        }

//        [Fact]
//        public async Task GetCar_MoveRequestFailed_RetriesMovement()
//        {
//            // Arrange
//            var request = new WeNeedFollowMeRequestDto
//            {
//                AirplaneId = Guid.NewGuid(),
//                FollowType = 1,
//                GateNumber = 1,
//                RunawayNumber = 1
//            };

//            _mockGroundControlService
//                .Setup(x => x.RegisterVehicle(It.IsAny<string>()))
//                .ReturnsAsync(new VehicleRegistrationResponse
//                {
//                    VehicleId = Guid.NewGuid().ToString()
//                });

//            _mockGroundControlService
//                .Setup(x => x.GetRoute(It.IsAny<string>(), It.IsAny<string>()))
//                .ReturnsAsync(new[] { "node_1", "node_2" });

//            _mockGroundControlService
//                .SetupSequence(x => x.RequestMove(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//                .ReturnsAsync(-1) // Первый запрос на перемещение неудачный
//                .ReturnsAsync(100); // Второй запрос успешный

//            // Act
//            var result = await _controller.GetCar(request);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            Assert.NotNull(okResult.Value);

//            _mockGroundControlService.Verify(x => x.RequestMove(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
//        }
//    }
//}