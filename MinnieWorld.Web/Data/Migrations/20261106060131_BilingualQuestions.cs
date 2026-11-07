using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinnieWorld.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class BilingualQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExplanationHi",
                schema: "exam",
                table: "Questions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionAHi",
                schema: "exam",
                table: "Questions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionBHi",
                schema: "exam",
                table: "Questions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionCHi",
                schema: "exam",
                table: "Questions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionDHi",
                schema: "exam",
                table: "Questions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TextHi",
                schema: "exam",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SeedVersion",
                schema: "exam",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 1); // tests already in the database came from version-1 seed files
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExplanationHi",
                schema: "exam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OptionAHi",
                schema: "exam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OptionBHi",
                schema: "exam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OptionCHi",
                schema: "exam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OptionDHi",
                schema: "exam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "TextHi",
                schema: "exam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SeedVersion",
                schema: "exam",
                table: "Exams");
        }
    }
}
