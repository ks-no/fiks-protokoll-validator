using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class ModelChangeFiksPayload : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payload",
                table: "FiksResponse");

            migrationBuilder.DropColumn(
                name: "PayloadContent",
                table: "FiksResponse");

            migrationBuilder.CreateTable(
                name: "FiksPayload",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FiksResponseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiksPayload", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiksPayload_FiksResponse_FiksResponseId",
                        column: x => x.FiksResponseId,
                        principalTable: "FiksResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FiksPayload_FiksResponseId",
                table: "FiksPayload",
                column: "FiksResponseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiksPayload");

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                table: "FiksResponse",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayloadContent",
                table: "FiksResponse",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
