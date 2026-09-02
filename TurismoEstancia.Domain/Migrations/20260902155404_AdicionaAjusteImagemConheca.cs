using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAjusteImagemConheca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImagemPosicaoX",
                table: "ConhecaEstanciaItens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ImagemPosicaoY",
                table: "ConhecaEstanciaItens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ImagemZoom",
                table: "ConhecaEstanciaItens",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemPosicaoX",
                table: "ConhecaEstanciaItens");

            migrationBuilder.DropColumn(
                name: "ImagemPosicaoY",
                table: "ConhecaEstanciaItens");

            migrationBuilder.DropColumn(
                name: "ImagemZoom",
                table: "ConhecaEstanciaItens");
        }
    }
}
