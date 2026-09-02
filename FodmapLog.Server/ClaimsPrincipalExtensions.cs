using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace FodmapLog.Server
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Resolves the authenticated user id from JWT <c>sub</c> (preferred) or NameIdentifier.
        /// </summary>
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static string RequireUserId(this ControllerBase controller)
        {
            var userId = controller.User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("Authenticated user id (sub) is missing from the token.");
            }
            return userId;
        }
    }
}
