using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class PermiteFotoEmVariasCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GaleriaMidias_CategoriaId_ArquivoId",
                table: "GaleriaMidias",
                columns: new[] { "CategoriaId", "ArquivoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GaleriaMidias_CategoriaId_ArquivoId",
                table: "GaleriaMidias");
        }
    }
}
