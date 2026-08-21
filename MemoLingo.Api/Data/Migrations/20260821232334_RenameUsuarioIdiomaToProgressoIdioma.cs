using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemoLingo.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameUsuarioIdiomaToProgressoIdioma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioIdiomas");

            migrationBuilder.CreateTable(
                name: "ProgressosIdioma",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    IdiomaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    XpTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCursoAtivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalPalavrasAprendidas = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalLicoesConcluidas = table.Column<int>(type: "INTEGER", nullable: false),
                    OfensivaAtualDias = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressosIdioma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressosIdioma_Idiomas_IdiomaId",
                        column: x => x.IdiomaId,
                        principalTable: "Idiomas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgressosIdioma_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosIdioma_IdiomaId",
                table: "ProgressosIdioma",
                column: "IdiomaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosIdioma_UsuarioId_IdiomaId",
                table: "ProgressosIdioma",
                columns: new[] { "UsuarioId", "IdiomaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressosIdioma");

            migrationBuilder.CreateTable(
                name: "UsuarioIdiomas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdiomaId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioIdiomas_IdiomaId",
                table: "UsuarioIdiomas",
                column: "IdiomaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioIdiomas_UsuarioId_IdiomaId",
                table: "UsuarioIdiomas",
                columns: new[] { "UsuarioId", "IdiomaId" },
                unique: true);
        }
    }
}
