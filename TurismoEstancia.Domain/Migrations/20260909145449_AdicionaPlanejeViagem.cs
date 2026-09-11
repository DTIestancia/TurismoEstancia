using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaPlanejeViagem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanejeCategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ImagemPadraoArquivoId = table.Column<long>(type: "bigint", nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanejeCategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanejeCategorias_Arquivos_ImagemPadraoArquivoId",
                        column: x => x.ImagemPadraoArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "ArquId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlanejeItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Localizacao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Contato = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Site = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ImagemArquivoId = table.Column<long>(type: "bigint", nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanejeItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanejeItens_Arquivos_ImagemArquivoId",
                        column: x => x.ImagemArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "ArquId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlanejeItens_PlanejeCategorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "PlanejeCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanejeAvaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanejeItemId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Nota = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Aprovada = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanejeAvaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanejeAvaliacoes_PlanejeItens_PlanejeItemId",
                        column: x => x.PlanejeItemId,
                        principalTable: "PlanejeItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanejeAvaliacoes_PlanejeItemId",
                table: "PlanejeAvaliacoes",
                column: "PlanejeItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanejeCategorias_ImagemPadraoArquivoId",
                table: "PlanejeCategorias",
                column: "ImagemPadraoArquivoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanejeItens_CategoriaId_Ordem",
                table: "PlanejeItens",
                columns: new[] { "CategoriaId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanejeItens_ImagemArquivoId",
                table: "PlanejeItens",
                column: "ImagemArquivoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanejeAvaliacoes");

            migrationBuilder.DropTable(
                name: "PlanejeItens");

            migrationBuilder.DropTable(
                name: "PlanejeCategorias");
        }
    }
}
