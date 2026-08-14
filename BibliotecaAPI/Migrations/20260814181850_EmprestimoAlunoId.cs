using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoBiblioteca.Migrations
{
    /// <inheritdoc />
    public partial class EmprestimoAlunoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emprestimos_Alunos_AlunoId",
                table: "Emprestimos");

            migrationBuilder.AlterColumn<int>(
                name: "AlunoId",
                table: "Emprestimos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Emprestimos_Alunos_AlunoId",
                table: "Emprestimos",
                column: "AlunoId",
                principalTable: "Alunos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emprestimos_Alunos_AlunoId",
                table: "Emprestimos");

            migrationBuilder.AlterColumn<int>(
                name: "AlunoId",
                table: "Emprestimos",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Emprestimos_Alunos_AlunoId",
                table: "Emprestimos",
                column: "AlunoId",
                principalTable: "Alunos",
                principalColumn: "Id");
        }
    }
}
