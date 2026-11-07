using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinnieWorld.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentAccessAndSamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AllAccessUntil",
                schema: "exam",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAllAccess",
                schema: "exam",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                schema: "exam",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                schema: "exam",
                table: "Students",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Existing students get their own security stamp (they'll be asked to log in again once).
            migrationBuilder.Sql("UPDATE exam.Students SET SecurityStamp = REPLACE(CONVERT(nvarchar(36), NEWID()), '-', '')");

            // Full papers are paid; free practice moves to the new sample tests.
            migrationBuilder.Sql("UPDATE exam.Exams SET Price = 10 WHERE Slug = 'hp-tet-tgt-arts-mock-1' AND Price = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllAccessUntil",
                schema: "exam",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "HasAllAccess",
                schema: "exam",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                schema: "exam",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                schema: "exam",
                table: "Students");
        }
    }
}
