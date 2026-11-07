using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinnieWorld.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExamPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Price",
                schema: "exam",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Tests seeded before prices existed: keep Mock Test 1 free as a taster, the rest cost ₹10.
            migrationBuilder.Sql("UPDATE exam.Exams SET Price = 10 WHERE Slug LIKE 'hp-tet-tgt-arts-%' AND Slug <> 'hp-tet-tgt-arts-mock-1'");

            migrationBuilder.CreateTable(
                name: "Purchases",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    UpiRef = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchases_Exams_ExamId",
                        column: x => x.ExamId,
                        principalSchema: "exam",
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Purchases_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "exam",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ExamId",
                schema: "exam",
                table: "Purchases",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_Status_CreatedAt",
                schema: "exam",
                table: "Purchases",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_StudentId_ExamId",
                schema: "exam",
                table: "Purchases",
                columns: new[] { "StudentId", "ExamId" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_UpiRef",
                schema: "exam",
                table: "Purchases",
                column: "UpiRef",
                unique: true,
                filter: "[Status] <> 'Rejected'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Purchases",
                schema: "exam");

            migrationBuilder.DropColumn(
                name: "Price",
                schema: "exam",
                table: "Exams");
        }
    }
}
