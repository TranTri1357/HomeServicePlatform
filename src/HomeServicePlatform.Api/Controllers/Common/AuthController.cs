using System.Security.Claims;
using HomeServicePlatform.Application.Modules.Identity.Commands.ChangePassword;
using HomeServicePlatform.Application.Modules.Identity.Commands.Login;
using HomeServicePlatform.Application.Modules.Identity.Commands.Logout;
using HomeServicePlatform.Application.Modules.Identity.Commands.RefreshToken;
using HomeServicePlatform.Application.Modules.Identity.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HomeServicePlatform.Api.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            if (command == null)
                return BadRequest("Dữ liệu đổi mật khẩu không được để trống.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                return Unauthorized();

            var securedCommand = command;
            securedCommand.UserId = userId;

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }
    }
}
