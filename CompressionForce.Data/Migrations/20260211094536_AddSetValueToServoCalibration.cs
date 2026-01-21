using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompressionForce.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSetValueToServoCalibration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SetValue",
                table: "ServoCalibrations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SetValue",
                table: "ServoCalibrations");
        }
    }
}
