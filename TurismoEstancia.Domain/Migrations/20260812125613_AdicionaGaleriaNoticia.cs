using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaGaleriaNoticia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GaleriaCategoriaId",
                table: "Noticias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_GaleriaCategoriaId",
                table: "Noticias",
                column: "GaleriaCategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Noticias_GaleriaCategorias_GaleriaCategoriaId",
                table: "Noticias",
                column: "GaleriaCategoriaId",
                principalTable: "GaleriaCategorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Noticias_GaleriaCategorias_GaleriaCategoriaId",
                table: "Noticias");

            migrationBuilder.DropIndex(
                name: "IX_Noticias_GaleriaCategoriaId",
                table: "Noticias");

            migrationBuilder.DropColumn(
                name: "GaleriaCategoriaId",
                table: "Noticias");
        }
    }
}
