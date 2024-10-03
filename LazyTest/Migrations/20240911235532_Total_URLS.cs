using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LazyTest.Migrations
{
    /// <inheritdoc />
    public partial class Total_URLS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestedUrls",
                table: "TestSite",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalUrls",
                table: "TestSite",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestedUrls",
                table: "TestSite");

            migrationBuilder.DropColumn(
                name: "TotalUrls",
                table: "TestSite");
        }
    }
}
