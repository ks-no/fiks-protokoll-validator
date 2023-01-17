using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class AddIsAsiceVerifiedAndPayloadMessagesToFiksResponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAsiceSigned",
                table: "FiksResponse",
                newName: "IsAsiceVerified");

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
                name: "PayloadErrors",
                table: "FiksResponse");

            migrationBuilder.RenameColumn(
                name: "IsAsiceVerified",
                table: "FiksResponse",
                newName: "IsAsiceSigned");
        }
    }
}
