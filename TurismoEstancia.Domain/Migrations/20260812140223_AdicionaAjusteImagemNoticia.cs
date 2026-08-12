using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAjusteImagemNoticia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImagemPosicaoX",
                table: "Noticias",
                type: "int",
                nullable: false,
                defaultValue: 50);

            migrationBuilder.AddColumn<int>(
                name: "ImagemPosicaoY",
                table: "Noticias",
                type: "int",
                nullable: false,
                defaultValue: 50);

            migrationBuilder.AddColumn<int>(
                name: "ImagemZoom",
                table: "Noticias",
                type: "int",
                nullable: false,
                defaultValue: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemPosicaoX",
                table: "Noticias");

            migrationBuilder.DropColumn(
                name: "ImagemPosicaoY",
                table: "Noticias");

            migrationBuilder.DropColumn(
                name: "ImagemZoom",
                table: "Noticias");
        }
    }
}
