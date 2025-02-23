using Microsoft.AspNetCore.Mvc;
using System;
using FollowMe.Data;

namespace FollowMeM.Controllers
{
    [ApiController]
    public class NavigationController : ControllerBase
    {
        [HttpPost("navigate")]
        public IActionResult Navigate([FromBody] NavigationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto { ErrorCode = 30, Message = "Invalid Navigate" });
            }

            if (request.Navigate != "follow" && request.Navigate != "right" && request.Navigate != "left" && request.Navigate != "stop")
            {
                return BadRequest(new ErrorResponseDto { ErrorCode = 31, Message = "Wrong Navigate. It should be [follow, right, left, stop]" });
            }

            // Здесь логика отправки сигналов в самолет
            // Например, отправка через MQ или другой механизм

            return NoContent();
        }
    }
}