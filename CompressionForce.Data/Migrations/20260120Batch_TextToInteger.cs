using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace CompressionForce.Data.Migrations
{
    public partial class _20260120Batch_TextToInteger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure non-numeric text values are converted to 0 before changing column type
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""BatchSize"" = '0' WHERE ""BatchSize"" !~ '^[0-9]+$'");
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""BatchNumber"" = '0' WHERE ""BatchNumber"" !~ '^[0-9]+$'");
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""ProdusedQty"" = '0' WHERE ""ProdusedQty"" !~ '^[0-9]+$'");
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""LeftQty"" = '0' WHERE ""LeftQty"" !~ '^[0-9]+$'");
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""GoodQty"" = '0' WHERE ""GoodQty"" !~ '^[0-9]+$'");
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""RejectionQty"" = '0' WHERE ""RejectionQty"" !~ '^[0-9]+$'");
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""S2GoodQty"" = '0' WHERE ""S2GoodQty"" !~ '^[0-9]+$'");
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""S2RejectionQty"" = '0' WHERE ""S2RejectionQty"" !~ '^[0-9]+$'");
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""TabletQty"" = '0' WHERE ""TabletQty"" !~ '^[0-9]+$'");
            migrationBuilder.Sql(@"UPDATE ""Batches"" SET ""BatchQty"" = '0' WHERE ""BatchQty"" !~ '^[0-9]+$'");

            // Alter column types from text to integer
            migrationBuilder.AlterColumn<int>(
                name: "BatchSize",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BatchNumber",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProdusedQty",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LeftQty",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GoodQty",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RejectionQty",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "S2GoodQty",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "S2RejectionQty",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TabletQty",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BatchQty",
                table: "Batches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert columns back to text
            migrationBuilder.AlterColumn<string>(
                name: "BatchSize",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BatchNumber",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProdusedQty",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LeftQty",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GoodQty",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RejectionQty",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "S2GoodQty",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "S2RejectionQty",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TabletQty",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BatchQty",
                table: "Batches",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
