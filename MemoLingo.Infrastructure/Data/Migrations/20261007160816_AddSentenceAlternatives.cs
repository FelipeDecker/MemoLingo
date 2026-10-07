using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemoLingo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSentenceAlternatives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternativeTexts",
                table: "Sentences",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeTranslations",
                table: "Sentences",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternativeTexts",
                table: "Sentences");

            migrationBuilder.DropColumn(
                name: "AlternativeTranslations",
                table: "Sentences");
        }
    }
}
