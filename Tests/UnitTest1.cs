using System.Security.Claims;
using Xunit;

namespace Tests
{
    public class UserIdClaimResolutionTests
    {
        private const string SubClaimType = "sub";

        [Fact]
        public void Prefers_sub_claim_over_nameidentifier()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(SubClaimType, "user-from-sub"),
                new Claim(ClaimTypes.NameIdentifier, "user-from-nameid")
            }, authenticationType: "Test");

            var principal = new ClaimsPrincipal(identity);
            var userId = principal.FindFirst(SubClaimType)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Assert.Equal("user-from-sub", userId);
        }

        [Fact]
        public void Falls_back_to_nameidentifier_when_sub_missing()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-from-nameid")
            }, authenticationType: "Test");

            var principal = new ClaimsPrincipal(identity);
            var userId = principal.FindFirst(SubClaimType)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Assert.Equal("user-from-nameid", userId);
        }
    }
}
