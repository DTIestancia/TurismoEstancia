using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class PaginasDetalheGruposPratosTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "TagsCulturais",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImagemArquivoId",
                table: "TagsCulturais",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "PratosTuristicos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImagemArquivoId",
                table: "PratosTuristicos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "GruposCulturais",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImagemArquivoId",
                table: "GruposCulturais",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagsCulturais_ImagemArquivoId",
                table: "TagsCulturais",
                column: "ImagemArquivoId");

            migrationBuilder.CreateIndex(
                name: "IX_PratosTuristicos_ImagemArquivoId",
                table: "PratosTuristicos",
                column: "ImagemArquivoId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposCulturais_ImagemArquivoId",
                table: "GruposCulturais",
                column: "ImagemArquivoId");

            migrationBuilder.AddForeignKey(
                name: "FK_GruposCulturais_Arquivos_ImagemArquivoId",
                table: "GruposCulturais",
                column: "ImagemArquivoId",
                principalTable: "Arquivos",
                principalColumn: "ArquId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PratosTuristicos_Arquivos_ImagemArquivoId",
                table: "PratosTuristicos",
                column: "ImagemArquivoId",
                principalTable: "Arquivos",
                principalColumn: "ArquId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TagsCulturais_Arquivos_ImagemArquivoId",
                table: "TagsCulturais",
                column: "ImagemArquivoId",
                principalTable: "Arquivos",
                principalColumn: "ArquId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GruposCulturais_Arquivos_ImagemArquivoId",
                table: "GruposCulturais");

            migrationBuilder.DropForeignKey(
                name: "FK_PratosTuristicos_Arquivos_ImagemArquivoId",
                table: "PratosTuristicos");

            migrationBuilder.DropForeignKey(
                name: "FK_TagsCulturais_Arquivos_ImagemArquivoId",
                table: "TagsCulturais");

            migrationBuilder.DropIndex(
                name: "IX_TagsCulturais_ImagemArquivoId",
                table: "TagsCulturais");

            migrationBuilder.DropIndex(
                name: "IX_PratosTuristicos_ImagemArquivoId",
                table: "PratosTuristicos");

            migrationBuilder.DropIndex(
                name: "IX_GruposCulturais_ImagemArquivoId",
                table: "GruposCulturais");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "TagsCulturais");

            migrationBuilder.DropColumn(
                name: "ImagemArquivoId",
                table: "TagsCulturais");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "PratosTuristicos");

            migrationBuilder.DropColumn(
                name: "ImagemArquivoId",
                table: "PratosTuristicos");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "GruposCulturais");

            migrationBuilder.DropColumn(
                name: "ImagemArquivoId",
                table: "GruposCulturais");
        }
    }
}
