using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoBiblioteca.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDomainModelsFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "SenhaHash",
                value: "$2a$11$ohkyeBgXwphJebuiUHq5fu0rtUJ8xrne4MQrc8Livs2HGW8tYRdv.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "SenhaHash",
                value: "$2a$11$kaOr4lVi/FizMj1GwKQR2ujlE9nmbF65tQ/nX5zHgWZdeE3FHbCHy");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                column: "SenhaHash",
                value: "$2a$11$QQfyZ8TbNMfc.QFLztUN7eqrUG7ICAv89ksSg77WE/Qh7d1FEf6si");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "SenhaHash",
                value: "$2a$11$OMxtwqe8OeEZva3q/mqNSuPbY9ZYgouhkugmgM2KA/rVv4UKYzXGW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "SenhaHash",
                value: "$2a$11$B5J8bp1Gck9tgIypwtPsJODK5DqomfLdE5cvIUVydEnwJN3T/r6he");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                column: "SenhaHash",
                value: "$2a$11$vMLglUwc1mDVTntJLfWepui.3HGM/CZLapif3GW/TjP95DmTKQo3a");
        }
    }
}
