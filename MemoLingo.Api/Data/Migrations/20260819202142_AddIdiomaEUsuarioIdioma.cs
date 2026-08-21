using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MemoLingo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdiomaEUsuarioIdioma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Usuarios",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "IdiomaMaternoId",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Idiomas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Idiomas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioIdiomas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    IdiomaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioIdiomas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioIdiomas_Idiomas_IdiomaId",
                        column: x => x.IdiomaId,
                        principalTable: "Idiomas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioIdiomas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Idiomas",
                columns: new[] { "Id", "Codigo", "Nome" },
                values: new object[,]
                {
                    { 1, "en", "Inglês" },
                    { 2, "pt", "Português" },
                    { 3, "es", "Espanhol" },
                    { 4, "it", "Italiano" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdiomaMaternoId",
                table: "Usuarios",
                column: "IdiomaMaternoId");

            migrationBuilder.CreateIndex(
                name: "IX_Idiomas_Codigo",
                table: "Idiomas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioIdiomas_IdiomaId",
                table: "UsuarioIdiomas",
                column: "IdiomaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioIdiomas_UsuarioId_IdiomaId",
                table: "UsuarioIdiomas",
                columns: new[] { "UsuarioId", "IdiomaId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Idiomas_IdiomaMaternoId",
                table: "Usuarios",
                column: "IdiomaMaternoId",
                principalTable: "Idiomas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Idiomas_IdiomaMaternoId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "UsuarioIdiomas");

            migrationBuilder.DropTable(
                name: "Idiomas");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_IdiomaMaternoId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IdiomaMaternoId",
                table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Usuarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
