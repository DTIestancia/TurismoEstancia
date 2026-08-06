using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAnalyticsEventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsEventos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rota = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RefererHost = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SessaoId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Dispositivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Evento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntidadeId = table.Column<int>(type: "int", nullable: true),
                    EntidadeNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsEventos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEventos_Data",
                table: "AnalyticsEventos",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEventos_Tipo_Evento",
                table: "AnalyticsEventos",
                columns: new[] { "Tipo", "Evento" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsEventos");
        }
    }
}
