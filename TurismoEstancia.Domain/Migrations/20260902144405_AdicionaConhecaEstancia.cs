using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaConhecaEstancia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConhecaEstanciaItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Categoria = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImagemArquivoId = table.Column<long>(type: "bigint", nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConhecaEstanciaItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConhecaEstanciaItens_Arquivos_ImagemArquivoId",
                        column: x => x.ImagemArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "ArquId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConhecaEstanciaItens_Categoria_Ordem",
                table: "ConhecaEstanciaItens",
                columns: new[] { "Categoria", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_ConhecaEstanciaItens_ImagemArquivoId",
                table: "ConhecaEstanciaItens",
                column: "ImagemArquivoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConhecaEstanciaItens");
        }
    }
}
