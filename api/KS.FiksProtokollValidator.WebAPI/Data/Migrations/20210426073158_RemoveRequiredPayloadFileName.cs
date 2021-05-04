using Microsoft.EntityFrameworkCore.Migrations;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class RemoveRequiredPayloadFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PayloadFileName",
                table: "TestCases",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PayloadFileName",
                table: "TestCases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
