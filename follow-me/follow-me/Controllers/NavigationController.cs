using Microsoft.AspNetCore.Mvc;
using System;
using FollowMe.Data;
using FollowMe.Utils;

namespace FollowMeM.Controllers
{
    [ApiController]
    public class NavigationController : ControllerBase
    {
        [HttpPost("navigate")]
        public IActionResult Navigate([FromBody] NavigationRequestDto request)
        {
            Logger.Log("NavigationController", "INFO", $"Запрос на навигацию: {request.Navigate}.");

            if (!ModelState.IsValid)
            {
                Logger.Log("NavigationController", "ERROR", "Неверный формат запроса.");
                return BadRequest(new ErrorResponseDto { ErrorCode = 30, Message = "Invalid Navigate" });
            }

            if (request.Navigate != "follow" && request.Navigate != "right" && request.Navigate != "left" && request.Navigate != "stop")
            {
                Logger.Log("NavigationController", "ERROR", "Неверное значение Navigate.");
                return BadRequest(new ErrorResponseDto { ErrorCode = 31, Message = "Wrong Navigate. It should be [follow, right, left, stop]" });
            }

            // Здесь логика отправки сигналов в самолет
            // Например, отправка через MQ или другой механизм

            Logger.Log("NavigationController", "INFO", "Сигнал навигации успешно обработан.");

            return NoContent();
        }
    }
}