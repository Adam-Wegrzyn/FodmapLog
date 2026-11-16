using FodmapLog.Server.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FodmapLog.Server.Handlers
{
    public class ExternalLoginCallbackCommandHandler : IRequestHandler<ExternalLoginCallbackCommand, string?>
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<ExternalLoginCallbackCommandHandler> _logger;

        public ExternalLoginCallbackCommandHandler(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            JwtTokenService jwtTokenService,
            ILogger<ExternalLoginCallbackCommandHandler> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<string?> Handle(ExternalLoginCallbackCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.RemoteError))
            {
                return FailUrl(request.ReturnUrl);
            }

            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return FailUrl(request.ReturnUrl);
                }

                var signInResult = await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider, info.ProviderKey, isPersistent: false);
                IdentityUser user;

                if (signInResult.Succeeded)
                {
                    user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                }
                else
                {
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                        await _userManager.CreateAsync(user);
                        await _userManager.AddLoginAsync(user, info);
                    }
                    await _signInManager.SignInAsync(user, isPersistent: false);
                }

                if (user is not null && !user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtTokenService.GenerateJwtToken(user.Id, user.Email, roles);
                var separator = request.ReturnUrl.Contains('#') ? "&" : "#";
                return $"{request.ReturnUrl}{separator}token={Uri.EscapeDataString(token)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google callback failed after SQL or Identity error.");
                return FailUrl(request.ReturnUrl);
            }
        }

        private static string FailUrl(string returnUrl)
        {
            var errorSeparator = returnUrl.Contains('#') ? "&" : "#";
            return $"{returnUrl}{errorSeparator}error=ExternalLoginFailed";
        }
    }
}
