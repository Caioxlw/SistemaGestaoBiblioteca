using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaGestaoBiblioteca.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacoesETabelasFinais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlunoId = table.Column<int>(type: "integer", nullable: false),
                    Mensagem = table.Column<string>(type: "text", nullable: false),
                    DataNotificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Lida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_AlunoId",
                table: "Notificacoes",
                column: "AlunoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificacoes");
        }
    }
}
