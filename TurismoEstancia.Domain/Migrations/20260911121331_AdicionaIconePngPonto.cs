using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaIconePngPonto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IconeArquivoId",
                table: "PontosTuristicos",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PontosTuristicos_IconeArquivoId",
                table: "PontosTuristicos",
                column: "IconeArquivoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PontosTuristicos_Arquivos_IconeArquivoId",
                table: "PontosTuristicos",
                column: "IconeArquivoId",
                principalTable: "Arquivos",
                principalColumn: "ArquId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PontosTuristicos_Arquivos_IconeArquivoId",
                table: "PontosTuristicos");

            migrationBuilder.DropIndex(
                name: "IX_PontosTuristicos_IconeArquivoId",
                table: "PontosTuristicos");

            migrationBuilder.DropColumn(
                name: "IconeArquivoId",
                table: "PontosTuristicos");
        }
    }
}
