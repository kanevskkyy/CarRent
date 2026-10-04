using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverLicenseImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseImageUrl",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverLicenseImageUrl",
                table: "AspNetUsers");
        }
    }
}
