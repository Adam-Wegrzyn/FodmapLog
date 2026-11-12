using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FodmapLog.Server
{
    /// <summary>
    /// Identity /forgotPassword only emails users with EmailConfirmed.
    /// Sign-in does not require confirmation, so new users are created already confirmed.
    /// </summary>
    public sealed class AutoConfirmUserManager : UserManager<IdentityUser>
    {
        public AutoConfirmUserManager(
            IUserStore<IdentityUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<IdentityUser> passwordHasher,
            IEnumerable<IUserValidator<IdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<IdentityUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override Task<IdentityResult> CreateAsync(IdentityUser user)
        {
            user.EmailConfirmed = true;
            return base.CreateAsync(user);
        }

        public override Task<IdentityResult> CreateAsync(IdentityUser user, string password)
        {
            user.EmailConfirmed = true;
            return base.CreateAsync(user, password);
        }
    }
}
