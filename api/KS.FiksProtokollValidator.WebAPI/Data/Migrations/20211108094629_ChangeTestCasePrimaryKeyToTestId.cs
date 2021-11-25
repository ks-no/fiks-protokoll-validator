using Microsoft.EntityFrameworkCore.Migrations;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class ChangeTestCasePrimaryKeyToTestId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiksExpectedResponseMessageType_TestCases_TestCaseTestName",
                table: "FiksExpectedResponseMessageType");

            migrationBuilder.DropForeignKey(
                name: "FK_FiksRequest_TestCases_TestCaseTestName",
                table: "FiksRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_FiksResponseTest_TestCases_TestCaseTestName",
                table: "FiksResponseTest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestCases",
                table: "TestCases");

            migrationBuilder.DropIndex(
                name: "IX_TestCases_TestName",
                table: "TestCases");

            migrationBuilder.RenameColumn(
                name: "TestCaseTestName",
                table: "FiksResponseTest",
                newName: "TestCaseTestId");

            migrationBuilder.RenameIndex(
                name: "IX_FiksResponseTest_TestCaseTestName",
                table: "FiksResponseTest",
                newName: "IX_FiksResponseTest_TestCaseTestId");

            migrationBuilder.RenameColumn(
                name: "TestCaseTestName",
                table: "FiksRequest",
                newName: "TestCaseTestId");

            migrationBuilder.RenameIndex(
                name: "IX_FiksRequest_TestCaseTestName",
                table: "FiksRequest",
                newName: "IX_FiksRequest_TestCaseTestId");

            migrationBuilder.RenameColumn(
                name: "TestCaseTestName",
                table: "FiksExpectedResponseMessageType",
                newName: "TestCaseTestId");

            migrationBuilder.RenameIndex(
                name: "IX_FiksExpectedResponseMessageType_TestCaseTestName",
                table: "FiksExpectedResponseMessageType",
                newName: "IX_FiksExpectedResponseMessageType_TestCaseTestId");

            migrationBuilder.AlterColumn<string>(
                name: "TestName",
                table: "TestCases",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "TestId",
                table: "TestCases",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CustomPayloadFileId",
                table: "FiksRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestCases",
                table: "TestCases",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_TestId",
                table: "TestCases",
                column: "TestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiksRequest_CustomPayloadFileId",
                table: "FiksRequest",
                column: "CustomPayloadFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_FiksExpectedResponseMessageType_TestCases_TestCaseTestId",
                table: "FiksExpectedResponseMessageType",
                column: "TestCaseTestId",
                principalTable: "TestCases",
                principalColumn: "TestId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FiksRequest_FiksPayload_CustomPayloadFileId",
                table: "FiksRequest",
                column: "CustomPayloadFileId",
                principalTable: "FiksPayload",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FiksRequest_TestCases_TestCaseTestId",
                table: "FiksRequest",
                column: "TestCaseTestId",
                principalTable: "TestCases",
                principalColumn: "TestId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FiksResponseTest_TestCases_TestCaseTestId",
                table: "FiksResponseTest",
                column: "TestCaseTestId",
                principalTable: "TestCases",
                principalColumn: "TestId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiksExpectedResponseMessageType_TestCases_TestCaseTestId",
                table: "FiksExpectedResponseMessageType");

            migrationBuilder.DropForeignKey(
                name: "FK_FiksRequest_FiksPayload_CustomPayloadFileId",
                table: "FiksRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_FiksRequest_TestCases_TestCaseTestId",
                table: "FiksRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_FiksResponseTest_TestCases_TestCaseTestId",
                table: "FiksResponseTest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestCases",
                table: "TestCases");

            migrationBuilder.DropIndex(
                name: "IX_TestCases_TestId",
                table: "TestCases");

            migrationBuilder.DropIndex(
                name: "IX_FiksRequest_CustomPayloadFileId",
                table: "FiksRequest");

            migrationBuilder.DropColumn(
                name: "TestId",
                table: "TestCases");

            migrationBuilder.DropColumn(
                name: "CustomPayloadFileId",
                table: "FiksRequest");

            migrationBuilder.RenameColumn(
                name: "TestCaseTestId",
                table: "FiksResponseTest",
                newName: "TestCaseTestName");

            migrationBuilder.RenameIndex(
                name: "IX_FiksResponseTest_TestCaseTestId",
                table: "FiksResponseTest",
                newName: "IX_FiksResponseTest_TestCaseTestName");

            migrationBuilder.RenameColumn(
                name: "TestCaseTestId",
                table: "FiksRequest",
                newName: "TestCaseTestName");

            migrationBuilder.RenameIndex(
                name: "IX_FiksRequest_TestCaseTestId",
                table: "FiksRequest",
                newName: "IX_FiksRequest_TestCaseTestName");

            migrationBuilder.RenameColumn(
                name: "TestCaseTestId",
                table: "FiksExpectedResponseMessageType",
                newName: "TestCaseTestName");

            migrationBuilder.RenameIndex(
                name: "IX_FiksExpectedResponseMessageType_TestCaseTestId",
                table: "FiksExpectedResponseMessageType",
                newName: "IX_FiksExpectedResponseMessageType_TestCaseTestName");

            migrationBuilder.AlterColumn<string>(
                name: "TestName",
                table: "TestCases",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestCases",
                table: "TestCases",
                column: "TestName");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_TestName",
                table: "TestCases",
                column: "TestName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FiksExpectedResponseMessageType_TestCases_TestCaseTestName",
                table: "FiksExpectedResponseMessageType",
                column: "TestCaseTestName",
                principalTable: "TestCases",
                principalColumn: "TestName",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FiksRequest_TestCases_TestCaseTestName",
                table: "FiksRequest",
                column: "TestCaseTestName",
                principalTable: "TestCases",
                principalColumn: "TestName",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FiksResponseTest_TestCases_TestCaseTestName",
                table: "FiksResponseTest",
                column: "TestCaseTestName",
                principalTable: "TestCases",
                principalColumn: "TestName",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
