using FodmapLog.Server.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FodmapLog.Server.Handlers
{
    public class LoginCommandHandler: IRequestHandler<LoginCommand, string?>
    {
        UserManager<IdentityUser> _userManager;
        SignInManager<IdentityUser> _signInManager;
        private JwtTokenService _jwtTokenService;

        public LoginCommandHandler(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            JwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<string?> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null) return null;
            var result = await _signInManager.CheckPasswordSignInAsync(user, command.Password, false);
            if (!result.Succeeded) return null;
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenService.GenerateJwtToken(user.Id, user.Email, roles);
            return token;
        }
    }
}
