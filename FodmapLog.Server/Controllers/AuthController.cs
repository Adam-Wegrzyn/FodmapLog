using Core.Interfaces;
using FodmapLog.Server;
using FodmapLog.Server.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IFodmapLogService _fodmapLogService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IMediator mediator,
            UserManager<IdentityUser> userManager,
            IFodmapLogService fodmapLogService,
            ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _userManager = userManager;
            _fodmapLogService = fodmapLogService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _mediator.Send(new LoginCommand(loginDto.Email, loginDto.Password));
            if (token == null)
            {
                return Unauthorized();
            }
            return Ok(new { token });
        }

        [AllowAnonymous]
        [HttpGet("external-login")]
        public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string returnUrl = "/")
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { ReturnUrl = returnUrl });
            var properties = HttpContext.RequestServices
                .GetRequiredService<SignInManager<IdentityUser>>()
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/", string remoteError = null)
        {
            var url = await _mediator.Send(new ExternalLoginCallbackCommand(returnUrl, remoteError));
            return Redirect(url);
        }

        [Authorize]
        [EnableRateLimiting("auth")]
        [HttpPost("deleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto, CancellationToken cancellationToken)
        {
            var userId = this.RequireUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                if (string.IsNullOrWhiteSpace(dto?.Password)
                    || !await _userManager.CheckPasswordAsync(user, dto.Password))
                {
                    return Unauthorized();
                }
            }
            else if (!string.Equals(dto?.Confirm, "DELETE", StringComparison.Ordinal))
            {
                return BadRequest(new { error = "Confirm DELETE to remove a passwordless account." });
            }

            await _fodmapLogService.DeleteAllLogsForUser(userId, cancellationToken);
            var deleted = await _userManager.DeleteAsync(user);
            if (!deleted.Succeeded)
            {
                _logger.LogError("Failed to delete Identity user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("Deleted account {UserId}", userId);
            return NoContent();
        }
    }
}
