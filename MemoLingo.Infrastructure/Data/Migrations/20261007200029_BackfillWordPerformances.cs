using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemoLingo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillWordPerformances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // As tentativas das lições nunca atualizavam o WordPerformance; as da prática já
            // eram contabilizadas, por isso apenas as tentativas de sessões de lição são somadas.
            migrationBuilder.Sql("""
                INSERT INTO "WordPerformances" ("UserId", "WordId", "StrengthLevel", "CorrectCount", "WrongCount", "LastReview", "NextReview")
                SELECT ss."UserId",
                       ea."WordId",
                       0,
                       COUNT(*) FILTER (WHERE ea."IsCorrect")::integer,
                       COUNT(*) FILTER (WHERE NOT ea."IsCorrect")::integer,
                       MAX(ea."AnsweredAt"),
                       MAX(ea."AnsweredAt")
                FROM "ExerciseAttempts" ea
                INNER JOIN "StudySessions" ss ON ss."Id" = ea."StudySessionId"
                WHERE ea."WordId" IS NOT NULL
                  AND ss."LessonId" IS NOT NULL
                GROUP BY ss."UserId", ea."WordId"
                ON CONFLICT ("UserId", "WordId") DO UPDATE SET
                    "CorrectCount" = "WordPerformances"."CorrectCount" + EXCLUDED."CorrectCount",
                    "WrongCount" = "WordPerformances"."WrongCount" + EXCLUDED."WrongCount",
                    "LastReview" = GREATEST("WordPerformances"."LastReview", EXCLUDED."LastReview");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
