using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class CriarAuditoriaAcoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataHoraUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UsuarioNome = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Perfis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Rota = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetodoHttp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StatusHttp = table.Column<int>(type: "int", nullable: true),
                    DuracaoMs = table.Column<long>(type: "bigint", nullable: true),
                    EnderecoIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Navegador = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Correlacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Entidade = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EntidadeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ValoresAnterioresJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresNovosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sucesso = table.Column<bool>(type: "bit", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Area_DataHoraUtc",
                table: "Auditorias",
                columns: new[] { "Area", "DataHoraUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_DataHoraUtc",
                table: "Auditorias",
                column: "DataHoraUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Tipo_DataHoraUtc",
                table: "Auditorias",
                columns: new[] { "Tipo", "DataHoraUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_UsuarioId_DataHoraUtc",
                table: "Auditorias",
                columns: new[] { "UsuarioId", "DataHoraUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditorias");
        }
    }
}
