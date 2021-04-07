using Microsoft.EntityFrameworkCore.Migrations;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    public partial class AddedExpectedResponseMessageTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FiksExpectedResponseMessageType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpectedResponseMessageType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TestCaseTestName = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiksExpectedResponseMessageType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiksExpectedResponseMessageType_TestCases_TestCaseTestName",
                        column: x => x.TestCaseTestName,
                        principalTable: "TestCases",
                        principalColumn: "TestName",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FiksExpectedResponseMessageType_TestCaseTestName",
                table: "FiksExpectedResponseMessageType",
                column: "TestCaseTestName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiksExpectedResponseMessageType");
        }
    }
}
