using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LazyTest.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestSite",
                columns: table => new
                {
                    WebsiteId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SitemapUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSite", x => x.WebsiteId);
                });

            migrationBuilder.CreateTable(
                name: "TestResuts",
                columns: table => new
                {
                    TestUrlId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    ContentLenngth = table.Column<long>(type: "INTEGER", nullable: true),
                    DomCount = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusCode = table.Column<string>(type: "TEXT", nullable: false),
                    Responsetime = table.Column<int>(type: "INTEGER", nullable: false),
                    WebsiteId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResuts", x => x.TestUrlId);
                    table.ForeignKey(
                        name: "FK_TestResuts_TestSite_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "TestSite",
                        principalColumn: "WebsiteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestResuts_WebsiteId",
                table: "TestResuts",
                column: "WebsiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestResuts");

            migrationBuilder.DropTable(
                name: "TestSite");
        }
    }
}
