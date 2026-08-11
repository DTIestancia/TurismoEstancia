using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCapaGaleriaCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CapaArquivoId",
                table: "GaleriaCategorias",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GaleriaCategorias_CapaArquivoId",
                table: "GaleriaCategorias",
                column: "CapaArquivoId");

            migrationBuilder.AddForeignKey(
                name: "FK_GaleriaCategorias_Arquivos_CapaArquivoId",
                table: "GaleriaCategorias",
                column: "CapaArquivoId",
                principalTable: "Arquivos",
                principalColumn: "ArquId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GaleriaCategorias_Arquivos_CapaArquivoId",
                table: "GaleriaCategorias");

            migrationBuilder.DropIndex(
                name: "IX_GaleriaCategorias_CapaArquivoId",
                table: "GaleriaCategorias");

            migrationBuilder.DropColumn(
                name: "CapaArquivoId",
                table: "GaleriaCategorias");
        }
    }
}
