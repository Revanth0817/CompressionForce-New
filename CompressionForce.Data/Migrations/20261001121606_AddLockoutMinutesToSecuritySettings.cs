using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompressionForce.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLockoutMinutesToSecuritySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LockoutMinutes",
                table: "SecuritySettings",
                type: "integer",
                nullable: false,
                defaultValue: 30);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockoutMinutes",
                table: "SecuritySettings");
        }
    }
}
