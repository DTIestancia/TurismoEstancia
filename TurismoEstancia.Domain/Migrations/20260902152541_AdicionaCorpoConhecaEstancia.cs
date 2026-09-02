using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCorpoConhecaEstancia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Corpo",
                table: "ConhecaEstanciaItens",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Corpo",
                table: "ConhecaEstanciaItens");
        }
    }
}
