using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MemoLingo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRelativePronounExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelativePronounExerciseId",
                table: "ExerciseAttempts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RelativePronounExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    ExerciseType = table.Column<int>(type: "integer", nullable: false),
                    Usage = table.Column<int>(type: "integer", nullable: false),
                    CefrLevel = table.Column<int>(type: "integer", nullable: false),
                    Sentence = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Translation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Answers = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShownPronouns = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AlternativeSentences = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelativePronounExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelativePronounExercises_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempts_RelativePronounExerciseId_AnsweredAt",
                table: "ExerciseAttempts",
                columns: new[] { "RelativePronounExerciseId", "AnsweredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RelativePronounExercises_LanguageId_ExerciseType",
                table: "RelativePronounExercises",
                columns: new[] { "LanguageId", "ExerciseType" });

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseAttempts_RelativePronounExercises_RelativePronounEx~",
                table: "ExerciseAttempts",
                column: "RelativePronounExerciseId",
                principalTable: "RelativePronounExercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseAttempts_RelativePronounExercises_RelativePronounEx~",
                table: "ExerciseAttempts");

            migrationBuilder.DropTable(
                name: "RelativePronounExercises");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseAttempts_RelativePronounExerciseId_AnsweredAt",
                table: "ExerciseAttempts");

            migrationBuilder.DropColumn(
                name: "RelativePronounExerciseId",
                table: "ExerciseAttempts");
        }
    }
}
