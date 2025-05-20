using FodmapLog.Server.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }
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
    

    [HttpGet("external-login")]
        public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string returnUrl = "/")
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { ReturnUrl = returnUrl });
            var properties = HttpContext.RequestServices
                .GetRequiredService<SignInManager<IdentityUser>>()
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/", string remoteError = null)
        {
            var url = await _mediator.Send(new ExternalLoginCallbackCommand(returnUrl, remoteError));

            return Redirect(url);
        }

        //  [HttpPost("register")]
        //    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        //    {
        //        // Implement your registration logic here:
        //        var result = await _mediator.Send(new RegisterC   ommand(registerDto.Email, registerDto.Password));
        //        //var user = new IdentityUser { UserName = registerDto.Email, Email = registerDto.Email };
        //        //var result = await _userManager.CreateAsync(user, registerDto.Password);

        //        if (!result.Succeeded)
        //        {
        //            return BadRequest(result.Errors);
        //        }

        //        // Optionally create a token or just return success
        //        return Ok(new { message = "User created successfully" });
        //    }
        //}
    }
}
