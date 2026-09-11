using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaIconePngCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IconeArquivoId",
                table: "CategoriasPontosTuristicos",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasPontosTuristicos_IconeArquivoId",
                table: "CategoriasPontosTuristicos",
                column: "IconeArquivoId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriasPontosTuristicos_Arquivos_IconeArquivoId",
                table: "CategoriasPontosTuristicos",
                column: "IconeArquivoId",
                principalTable: "Arquivos",
                principalColumn: "ArquId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriasPontosTuristicos_Arquivos_IconeArquivoId",
                table: "CategoriasPontosTuristicos");

            migrationBuilder.DropIndex(
                name: "IX_CategoriasPontosTuristicos_IconeArquivoId",
                table: "CategoriasPontosTuristicos");

            migrationBuilder.DropColumn(
                name: "IconeArquivoId",
                table: "CategoriasPontosTuristicos");
        }
    }
}
