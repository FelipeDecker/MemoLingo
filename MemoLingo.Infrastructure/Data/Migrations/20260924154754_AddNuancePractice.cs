using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MemoLingo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNuancePractice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NuanceExerciseId",
                table: "ExerciseAttempts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SynonymGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Meaning = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CefrLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynonymGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SynonymGroups_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NuanceExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SynonymGroupId = table.Column<int>(type: "integer", nullable: false),
                    TargetWordId = table.Column<int>(type: "integer", nullable: false),
                    SentenceContext = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SentenceTranslation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AcceptedAnswers = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NuanceExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NuanceExercises_SynonymGroups_SynonymGroupId",
                        column: x => x.SynonymGroupId,
                        principalTable: "SynonymGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NuanceExercises_Words_TargetWordId",
                        column: x => x.TargetWordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SynonymGroupItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SynonymGroupId = table.Column<int>(type: "integer", nullable: false),
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    NuanceExplanation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynonymGroupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SynonymGroupItems_SynonymGroups_SynonymGroupId",
                        column: x => x.SynonymGroupId,
                        principalTable: "SynonymGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SynonymGroupItems_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserNuanceProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SynonymGroupId = table.Column<int>(type: "integer", nullable: false),
                    StrengthLevel = table.Column<int>(type: "integer", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    WrongCount = table.Column<int>(type: "integer", nullable: false),
                    ProficiencyScore = table.Column<int>(type: "integer", nullable: false),
                    IsFlaggedForReview = table.Column<bool>(type: "boolean", nullable: false),
                    LastErrorAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReview = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextReview = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNuanceProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNuanceProgresses_SynonymGroups_SynonymGroupId",
                        column: x => x.SynonymGroupId,
                        principalTable: "SynonymGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNuanceProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempts_NuanceExerciseId_AnsweredAt",
                table: "ExerciseAttempts",
                columns: new[] { "NuanceExerciseId", "AnsweredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NuanceExercises_SynonymGroupId",
                table: "NuanceExercises",
                column: "SynonymGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_NuanceExercises_TargetWordId",
                table: "NuanceExercises",
                column: "TargetWordId");

            migrationBuilder.CreateIndex(
                name: "IX_SynonymGroupItems_SynonymGroupId_WordId",
                table: "SynonymGroupItems",
                columns: new[] { "SynonymGroupId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SynonymGroupItems_WordId",
                table: "SynonymGroupItems",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_SynonymGroups_LanguageId_Name",
                table: "SynonymGroups",
                columns: new[] { "LanguageId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNuanceProgresses_SynonymGroupId",
                table: "UserNuanceProgresses",
                column: "SynonymGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNuanceProgresses_UserId_NextReview",
                table: "UserNuanceProgresses",
                columns: new[] { "UserId", "NextReview" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNuanceProgresses_UserId_SynonymGroupId",
                table: "UserNuanceProgresses",
                columns: new[] { "UserId", "SynonymGroupId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseAttempts_NuanceExercises_NuanceExerciseId",
                table: "ExerciseAttempts",
                column: "NuanceExerciseId",
                principalTable: "NuanceExercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseAttempts_NuanceExercises_NuanceExerciseId",
                table: "ExerciseAttempts");

            migrationBuilder.DropTable(
                name: "NuanceExercises");

            migrationBuilder.DropTable(
                name: "SynonymGroupItems");

            migrationBuilder.DropTable(
                name: "UserNuanceProgresses");

            migrationBuilder.DropTable(
                name: "SynonymGroups");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseAttempts_NuanceExerciseId_AnsweredAt",
                table: "ExerciseAttempts");

            migrationBuilder.DropColumn(
                name: "NuanceExerciseId",
                table: "ExerciseAttempts");
        }
    }
}
