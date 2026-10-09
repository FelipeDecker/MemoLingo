using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MemoLingo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrepositionExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrepositionExerciseId",
                table: "ExerciseAttempts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrepositionExercises",
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
                    ShownPrepositions = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AlternativeSentences = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrepositionExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrepositionExercises_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempts_PrepositionExerciseId_AnsweredAt",
                table: "ExerciseAttempts",
                columns: new[] { "PrepositionExerciseId", "AnsweredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PrepositionExercises_LanguageId_ExerciseType",
                table: "PrepositionExercises",
                columns: new[] { "LanguageId", "ExerciseType" });

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseAttempts_PrepositionExercises_PrepositionExerciseId",
                table: "ExerciseAttempts",
                column: "PrepositionExerciseId",
                principalTable: "PrepositionExercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseAttempts_PrepositionExercises_PrepositionExerciseId",
                table: "ExerciseAttempts");

            migrationBuilder.DropTable(
                name: "PrepositionExercises");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseAttempts_PrepositionExerciseId_AnsweredAt",
                table: "ExerciseAttempts");

            migrationBuilder.DropColumn(
                name: "PrepositionExerciseId",
                table: "ExerciseAttempts");
        }
    }
}
