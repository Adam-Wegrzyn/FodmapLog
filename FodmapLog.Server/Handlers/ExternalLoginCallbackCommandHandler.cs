using FodmapLog.Server.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FodmapLog.Server.Handlers
{
    public class ExternalLoginCallbackCommandHandler:IRequestHandler<ExternalLoginCallbackCommand, string?>
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtTokenService _jwtTokenService;

        public ExternalLoginCallbackCommandHandler(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager, JwtTokenService jwtTokenService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;

        }
        public async Task<string?> Handle(ExternalLoginCallbackCommand request, CancellationToken cancellationToken)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // Handle error, maybe redirect with an error message
                return $"{request.ReturnUrl}?error=ExternalLoginFailed";
            }

            // Try to sign in the user with this external login provider
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            IdentityUser user;

            if (signInResult.Succeeded)
            {
                user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            }
            else
            {
                // If the user does not have an account, create one
                var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
                user = await _userManager.FindByEmailAsync(email);
                
                if (user == null)
                {
                    user = new IdentityUser { UserName = email, Email = email };
                    await _userManager.CreateAsync(user);
                    await _userManager.AddLoginAsync(user, info);
                }
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            // Generate JWT
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenService.GenerateJwtToken(user.Id, user.Email, roles);

            // Redirect to frontend with token (e.g., as a query parameter or fragment)
            return $"{request.ReturnUrl}?token={token}";
        }
    }
 
}
