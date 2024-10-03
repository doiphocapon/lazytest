using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LazyTest.Migrations
{
    /// <inheritdoc />
    public partial class Edit_Response_Time : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Responsetime",
                table: "TestResult",
                newName: "ResponseTime");

            migrationBuilder.AlterColumn<double>(
                name: "ResponseTime",
                table: "TestResult",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResponseTime",
                table: "TestResult",
                newName: "Responsetime");

            migrationBuilder.AlterColumn<int>(
                name: "Responsetime",
                table: "TestResult",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");
        }
    }
}
