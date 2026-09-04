using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaGestaoBiblioteca.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDomainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Livros",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Livros",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Editora",
                table: "Livros",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Localizacao",
                table: "Livros",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NomeUsuario = table.Column<string>(type: "text", nullable: false),
                    Acao = table.Column<string>(type: "text", nullable: false),
                    Entidade = table.Column<string>(type: "text", nullable: false),
                    EntidadeId = table.Column<int>(type: "integer", nullable: true),
                    Detalhes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LivroId = table.Column<int>(type: "integer", nullable: false),
                    AlunoId = table.Column<int>(type: "integer", nullable: false),
                    DataReserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Livros_LivroId",
                        column: x => x.LivroId,
                        principalTable: "Livros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: false),
                    Perfil = table.Column<int>(type: "integer", nullable: false),
                    AlunoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Alunos",
                columns: new[] { "Id", "Email", "Matricula", "Nome" },
                values: new object[] { 1, "aluno@smartlib.com", "2026001", "Aluno Exemplo" });

            migrationBuilder.InsertData(
                table: "Autores",
                columns: new[] { "Id", "DataNascimento", "Nacionalidade", "Nome" },
                values: new object[] { 1, new DateTime(1892, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Britânico", "J.R.R. Tolkien" });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "AlunoId", "Email", "Nome", "Perfil", "SenhaHash" },
                values: new object[,]
                {
                    { 1, null, "admin@smartlib.com", "Administrador", 0, "$2a$11$OMxtwqe8OeEZva3q/mqNSuPbY9ZYgouhkugmgM2KA/rVv4UKYzXGW" },
                    { 2, null, "biblio@smartlib.com", "Bibliotecário Chefe", 1, "$2a$11$B5J8bp1Gck9tgIypwtPsJODK5DqomfLdE5cvIUVydEnwJN3T/r6he" }
                });

            migrationBuilder.InsertData(
                table: "Livros",
                columns: new[] { "Id", "AnoPublicacao", "AutorId", "Categoria", "Descricao", "Editora", "Isbn", "Localizacao", "Quantidade", "Titulo" },
                values: new object[,]
                {
                    { 1, 1954, 1, "Fantasia", "Fantasia épica.", "HarperCollins", "9780007136599", "A1", 0, "O Senhor dos Anéis" },
                    { 2, 2003, 1, "Tecnologia", "Livro de java", "OReilly", "9781234567897", "T1", 5, "Head First Java" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "AlunoId", "Email", "Nome", "Perfil", "SenhaHash" },
                values: new object[] { 3, 1, "aluno@smartlib.com", "Aluno Exemplo", 2, "$2a$11$vMLglUwc1mDVTntJLfWepui.3HGM/CZLapif3GW/TjP95DmTKQo3a" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_AlunoId",
                table: "Reservas",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_LivroId",
                table: "Reservas",
                column: "LivroId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AlunoId",
                table: "Usuarios",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditoria");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Livros",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Livros",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Autores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Livros");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Livros");

            migrationBuilder.DropColumn(
                name: "Editora",
                table: "Livros");

            migrationBuilder.DropColumn(
                name: "Localizacao",
                table: "Livros");
        }
    }
}
