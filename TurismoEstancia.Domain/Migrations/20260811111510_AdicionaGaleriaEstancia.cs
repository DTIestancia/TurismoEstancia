using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaGaleriaEstancia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GaleriaCategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Chave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GaleriaCategorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GaleriaMidias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    ArquivoId = table.Column<long>(type: "bigint", nullable: false),
                    ArquivoThumbId = table.Column<long>(type: "bigint", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GaleriaMidias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GaleriaMidias_Arquivos_ArquivoId",
                        column: x => x.ArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "ArquId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GaleriaMidias_Arquivos_ArquivoThumbId",
                        column: x => x.ArquivoThumbId,
                        principalTable: "Arquivos",
                        principalColumn: "ArquId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GaleriaMidias_GaleriaCategorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "GaleriaCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GaleriaCategorias_Chave",
                table: "GaleriaCategorias",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GaleriaCategorias_Ordem",
                table: "GaleriaCategorias",
                column: "Ordem");

            migrationBuilder.CreateIndex(
                name: "IX_GaleriaMidias_ArquivoId",
                table: "GaleriaMidias",
                column: "ArquivoId");

            migrationBuilder.CreateIndex(
                name: "IX_GaleriaMidias_ArquivoThumbId",
                table: "GaleriaMidias",
                column: "ArquivoThumbId");

            migrationBuilder.CreateIndex(
                name: "IX_GaleriaMidias_CategoriaId",
                table: "GaleriaMidias",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GaleriaMidias");

            migrationBuilder.DropTable(
                name: "GaleriaCategorias");
        }
    }
}
