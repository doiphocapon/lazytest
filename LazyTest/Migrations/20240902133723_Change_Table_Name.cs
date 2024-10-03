using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LazyTest.Migrations
{
    /// <inheritdoc />
    public partial class Change_Table_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestResuts_TestSite_WebsiteId",
                table: "TestResuts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestResuts",
                table: "TestResuts");

            migrationBuilder.RenameTable(
                name: "TestResuts",
                newName: "TestResult");

            migrationBuilder.RenameIndex(
                name: "IX_TestResuts_WebsiteId",
                table: "TestResult",
                newName: "IX_TestResult_WebsiteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestResult",
                table: "TestResult",
                column: "TestUrlId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestResult_TestSite_WebsiteId",
                table: "TestResult",
                column: "WebsiteId",
                principalTable: "TestSite",
                principalColumn: "WebsiteId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestResult_TestSite_WebsiteId",
                table: "TestResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestResult",
                table: "TestResult");

            migrationBuilder.RenameTable(
                name: "TestResult",
                newName: "TestResuts");

            migrationBuilder.RenameIndex(
                name: "IX_TestResult_WebsiteId",
                table: "TestResuts",
                newName: "IX_TestResuts_WebsiteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestResuts",
                table: "TestResuts",
                column: "TestUrlId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestResuts_TestSite_WebsiteId",
                table: "TestResuts",
                column: "WebsiteId",
                principalTable: "TestSite",
                principalColumn: "WebsiteId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
