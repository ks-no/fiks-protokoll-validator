using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class AddIsAsiceVerifiedAndPayloadMessagesToFiksResponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAsiceVerified",
                table: "FiksResponse",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PayloadErrors",
                table: "FiksResponse",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAsiceVerified",
                table: "FiksResponse");

            migrationBuilder.DropColumn(
                name: "PayloadErrors",
                table: "FiksResponse");
        }
    }
}
