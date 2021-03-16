using Microsoft.EntityFrameworkCore.Migrations;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class ChangedKeyOnTestCases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiksRequest_TestCases_TestCaseId",
                table: "FiksRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_FiksResponseTest_TestCases_TestCaseId",
                table: "FiksResponseTest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestCases",
                table: "TestCases");

            migrationBuilder.DropIndex(
                name: "IX_FiksResponseTest_TestCaseId",
                table: "FiksResponseTest");

            migrationBuilder.DropIndex(
                name: "IX_FiksRequest_TestCaseId",
                table: "FiksRequest");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TestCases");

            migrationBuilder.DropColumn(
                name: "TestCaseId",
                table: "FiksResponseTest");

            migrationBuilder.DropColumn(
                name: "TestCaseId",
                table: "FiksRequest");

            migrationBuilder.AddColumn<string>(
                name: "TestCaseTestName",
                table: "FiksResponseTest",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestCaseTestName",
                table: "FiksRequest",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestCases",
                table: "TestCases",
                column: "TestName");

            migrationBuilder.CreateIndex(
                name: "IX_FiksResponseTest_TestCaseTestName",
                table: "FiksResponseTest",
                column: "TestCaseTestName");

            migrationBuilder.CreateIndex(
                name: "IX_FiksRequest_TestCaseTestName",
                table: "FiksRequest",
                column: "TestCaseTestName");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_FiksResponseTest_TestCaseTestName",
                table: "FiksResponseTest");

            migrationBuilder.DropIndex(
                name: "IX_FiksRequest_TestCaseTestName",
                table: "FiksRequest");

            migrationBuilder.DropColumn(
                name: "TestCaseTestName",
                table: "FiksResponseTest");

            migrationBuilder.DropColumn(
                name: "TestCaseTestName",
                table: "FiksRequest");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TestCases",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "TestCaseId",
                table: "FiksResponseTest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TestCaseId",
                table: "FiksRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestCases",
                table: "TestCases",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FiksResponseTest_TestCaseId",
                table: "FiksResponseTest",
                column: "TestCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FiksRequest_TestCaseId",
                table: "FiksRequest",
                column: "TestCaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_FiksRequest_TestCases_TestCaseId",
                table: "FiksRequest",
                column: "TestCaseId",
                principalTable: "TestCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FiksResponseTest_TestCases_TestCaseId",
                table: "FiksResponseTest",
                column: "TestCaseId",
                principalTable: "TestCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
