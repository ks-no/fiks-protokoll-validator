using Microsoft.EntityFrameworkCore.Migrations;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class ReplaceCategoryWithProtocol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "TestCases",
                newName: "Protocol");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Protocol",
                table: "TestCases",
                newName: "Category");
        }
    }
}
