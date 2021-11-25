using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class AddFiksRequestPayload : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiksRequest_FiksPayload_CustomPayloadFileId",
                table: "FiksRequest");

            migrationBuilder.CreateTable(
                name: "FiksRequestPayload",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiksRequestPayload", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_FiksRequest_FiksRequestPayload_CustomPayloadFileId",
                table: "FiksRequest",
                column: "CustomPayloadFileId",
                principalTable: "FiksRequestPayload",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiksRequest_FiksRequestPayload_CustomPayloadFileId",
                table: "FiksRequest");

            migrationBuilder.DropTable(
                name: "FiksRequestPayload");

            migrationBuilder.AddForeignKey(
                name: "FK_FiksRequest_FiksPayload_CustomPayloadFileId",
                table: "FiksRequest",
                column: "CustomPayloadFileId",
                principalTable: "FiksPayload",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
