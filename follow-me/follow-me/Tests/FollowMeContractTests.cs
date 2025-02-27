using System;
using System.Threading.Tasks;
using FollowMe.Controllers;
using FollowMe.Data;
using FollowMe.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FollowMe.Tests
{
    public class FollowMeControllerValidationTests
    {
        private readonly Mock<IGroundControlService> _mockGroundControlService;
        private readonly FollowMeController _controller;

        public FollowMeControllerValidationTests()
        {
            _mockGroundControlService = new Mock<IGroundControlService>();
            _controller = new FollowMeController(_mockGroundControlService.Object);
        }

        [Fact(DisplayName = "Невалидный AirplaneId - BadRequest")]
        public async Task GetCar_InvalidAirplaneId_ReturnsBadRequest()
        {
            // Arrange
            var request = new WeNeedFollowMeRequestDto
            {
                AirplaneId = Guid.Empty, // Невалидный AirplaneId
                FollowType = 1,
                GateNumber = 1,
                RunawayNumber = 1
            };

            // Добавляем ошибку в ModelState, чтобы ModelState.IsValid вернул false
            _controller.ModelState.AddModelError("AirplaneId", "Invalid AirplaneId");

            // Act
            var result = await _controller.GetCar(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDto>(badRequestResult.Value);
            Assert.Equal(10, errorResponse.ErrorCode);
        }

        [Fact(DisplayName = "Невалидный FollowType - BadRequest")]
        public async Task GetCar_InvalidFollowType_ReturnsBadRequest()
        {
            // Arrange
            var request = new WeNeedFollowMeRequestDto
            {
                AirplaneId = Guid.NewGuid(),
                FollowType = 3, // Невалидный FollowType
                GateNumber = 1,
                RunawayNumber = 1
            };

            // Act
            var result = await _controller.GetCar(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDto>(badRequestResult.Value);
            Assert.Equal(21, errorResponse.ErrorCode);
        }

        [Fact(DisplayName = "Валидный запрос - OK")]
        public async Task GetCar_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new WeNeedFollowMeRequestDto
            {
                AirplaneId = Guid.NewGuid(),
                FollowType = 1,
                GateNumber = 1,
                RunawayNumber = 1
            };

            _mockGroundControlService
                .Setup(x => x.RegisterVehicle(It.IsAny<string>()))
                .ReturnsAsync(new VehicleRegistrationResponse
                {
                    VehicleId = Guid.NewGuid().ToString()
                });

            _mockGroundControlService
                .Setup(x => x.GetRoute(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[] { "node_1", "node_2" });

            // Act
            var result = await _controller.GetCar(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}