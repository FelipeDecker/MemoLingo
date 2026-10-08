using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemoLingo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionSkipTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExerciseCount",
                table: "StudySessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "StudySessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Markers",
                table: "GrammarTopics",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_SectionId",
                table: "StudySessions",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_Sections_SectionId",
                table: "StudySessions",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_Sections_SectionId",
                table: "StudySessions");

            migrationBuilder.DropIndex(
                name: "IX_StudySessions_SectionId",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "ExerciseCount",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "Markers",
                table: "GrammarTopics");
        }
    }
}
