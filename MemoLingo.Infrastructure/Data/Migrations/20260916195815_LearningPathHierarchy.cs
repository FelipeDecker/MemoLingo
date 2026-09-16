using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MemoLingo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class LearningPathHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // As lições antigas pertenciam diretamente ao curso e não podem ser mapeadas
            // para a nova hierarquia; os dados dependentes são removidos antes da reestruturação.
            migrationBuilder.Sql("DELETE FROM \"ExerciseAttempts\";");
            migrationBuilder.Sql("DELETE FROM \"StudySessions\";");
            migrationBuilder.Sql("DELETE FROM \"LessonWords\";");
            migrationBuilder.Sql("DELETE FROM \"Lessons\";");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Courses_CourseId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_CourseId_Position",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "CefrLevel",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ExerciseCount",
                table: "Lessons");

            migrationBuilder.AddColumn<int>(
                name: "PathNodeId",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChallengeId",
                table: "ExerciseAttempts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    WordId = table.Column<int>(type: "integer", nullable: true),
                    SentenceId = table.Column<int>(type: "integer", nullable: true),
                    ExerciseType = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpectedAnswer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OptionsJson = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Challenges_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Challenges_Sentences_SentenceId",
                        column: x => x.SentenceId,
                        principalTable: "Sentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Challenges_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    CefrLevel = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SectionId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Topic = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    GuidebookMarkdown = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Units_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PathNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    NodeType = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    TotalLessons = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PathNodes_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNodeProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PathNodeId = table.Column<int>(type: "integer", nullable: false),
                    CompletedLessonsCount = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPracticedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNodeProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNodeProgresses_PathNodes_PathNodeId",
                        column: x => x.PathNodeId,
                        principalTable: "PathNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNodeProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_PathNodeId_Position",
                table: "Lessons",
                columns: new[] { "PathNodeId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempts_ChallengeId",
                table: "ExerciseAttempts",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_LessonId_Position",
                table: "Challenges",
                columns: new[] { "LessonId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_SentenceId",
                table: "Challenges",
                column: "SentenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_WordId",
                table: "Challenges",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_PathNodes_UnitId_Position",
                table: "PathNodes",
                columns: new[] { "UnitId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_CourseId_Position",
                table: "Sections",
                columns: new[] { "CourseId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_Units_SectionId_Position",
                table: "Units",
                columns: new[] { "SectionId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNodeProgresses_PathNodeId",
                table: "UserNodeProgresses",
                column: "PathNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNodeProgresses_UserId_PathNodeId",
                table: "UserNodeProgresses",
                columns: new[] { "UserId", "PathNodeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseAttempts_Challenges_ChallengeId",
                table: "ExerciseAttempts",
                column: "ChallengeId",
                principalTable: "Challenges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_PathNodes_PathNodeId",
                table: "Lessons",
                column: "PathNodeId",
                principalTable: "PathNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseAttempts_Challenges_ChallengeId",
                table: "ExerciseAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_PathNodes_PathNodeId",
                table: "Lessons");

            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "UserNodeProgresses");

            migrationBuilder.DropTable(
                name: "PathNodes");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_PathNodeId_Position",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseAttempts_ChallengeId",
                table: "ExerciseAttempts");

            migrationBuilder.DropColumn(
                name: "ChallengeId",
                table: "ExerciseAttempts");

            migrationBuilder.DropColumn(
                name: "PathNodeId",
                table: "Lessons");

            migrationBuilder.AddColumn<int>(
                name: "ExerciseCount",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CefrLevel",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Lessons",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "Lessons",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_CourseId_Position",
                table: "Lessons",
                columns: new[] { "CourseId", "Position" });

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Courses_CourseId",
                table: "Lessons",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
