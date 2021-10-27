using Microsoft.EntityFrameworkCore.Migrations;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class AddPayloadFilePath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayloadFilePath",
                table: "TestCases",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayloadFilePath",
                table: "TestCases");
        }
    }
}
