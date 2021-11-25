using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class ChangeFiksRequestPrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiksResponse_FiksRequest_FiksRequestMessageGuid",
                table: "FiksResponse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FiksRequest",
                table: "FiksRequest");

            migrationBuilder.RenameColumn(
                name: "FiksRequestMessageGuid",
                table: "FiksResponse",
                newName: "FiksRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_FiksResponse_FiksRequestMessageGuid",
                table: "FiksResponse",
                newName: "IX_FiksResponse_FiksRequestId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "FiksRequest",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_FiksRequest",
                table: "FiksRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FiksResponse_FiksRequest_FiksRequestId",
                table: "FiksResponse",
                column: "FiksRequestId",
                principalTable: "FiksRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiksResponse_FiksRequest_FiksRequestId",
                table: "FiksResponse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FiksRequest",
                table: "FiksRequest");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FiksRequest");

            migrationBuilder.RenameColumn(
                name: "FiksRequestId",
                table: "FiksResponse",
                newName: "FiksRequestMessageGuid");

            migrationBuilder.RenameIndex(
                name: "IX_FiksResponse_FiksRequestId",
                table: "FiksResponse",
                newName: "IX_FiksResponse_FiksRequestMessageGuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FiksRequest",
                table: "FiksRequest",
                column: "MessageGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_FiksResponse_FiksRequest_FiksRequestMessageGuid",
                table: "FiksResponse",
                column: "FiksRequestMessageGuid",
                principalTable: "FiksRequest",
                principalColumn: "MessageGuid",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
