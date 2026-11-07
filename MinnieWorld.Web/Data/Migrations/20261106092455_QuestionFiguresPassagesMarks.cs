using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinnieWorld.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class QuestionFiguresPassagesMarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Figure",
                schema: "exam",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Marks",
                schema: "exam",
                table: "Questions",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "Passage",
                schema: "exam",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PassageHi",
                schema: "exam",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Figure",
                schema: "exam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Marks",
                schema: "exam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Passage",
                schema: "exam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PassageHi",
                schema: "exam",
                table: "Questions");
        }
    }
}
