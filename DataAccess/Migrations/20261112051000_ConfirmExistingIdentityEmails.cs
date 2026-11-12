using DataAccess;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(FodmapLogDbContext))]
    [Migration("20261112051000_ConfirmExistingIdentityEmails")]
    public partial class ConfirmExistingIdentityEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Identity /forgotPassword skips send when EmailConfirmed = 0. Login already
            // allows unconfirmed accounts; mark existing users confirmed so reset mail works.
            migrationBuilder.Sql("UPDATE [AspNetUsers] SET [EmailConfirmed] = 1 WHERE [EmailConfirmed] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
