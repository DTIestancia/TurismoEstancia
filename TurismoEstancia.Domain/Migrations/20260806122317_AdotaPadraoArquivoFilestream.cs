using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AdotaPadraoArquivoFilestream : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Arquivos",
                newName: "ArquFileName");

            migrationBuilder.RenameColumn(
                name: "CriadoEm",
                table: "Arquivos",
                newName: "ArquMomento");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Arquivos",
                newName: "ArquContentType");

            migrationBuilder.RenameColumn(
                name: "Bytes",
                table: "Arquivos",
                newName: "ArquBytes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Arquivos",
                newName: "ArquId");

            migrationBuilder.AddColumn<bool>(
                name: "ArquAtivo",
                table: "Arquivos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ArquAutor",
                table: "Arquivos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArquOrigem",
                table: "Arquivos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ArquSize",
                table: "Arquivos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "ArquUID",
                table: "Arquivos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            // ArquUID é o ROWGUIDCOL da tabela — pré-requisito do FILESTREAM
            // (sem ele o SQL Server recusa criar coluna varbinary(max) FILESTREAM).
            migrationBuilder.Sql("ALTER TABLE [Arquivos] ALTER COLUMN [ArquUID] ADD ROWGUIDCOL;");

            // Backfill do tamanho para as linhas já existentes (o binário já está em ArquBytes).
            migrationBuilder.Sql(
                "UPDATE [Arquivos] SET [ArquSize] = CONVERT(bigint, DATALENGTH([ArquBytes])) WHERE [ArquSize] = 0 AND [ArquBytes] IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove o ROWGUIDCOL antes de dropar a coluna (o SQL não pode
            // referenciar o ArquUID depois que ele já foi removido).
            migrationBuilder.Sql("ALTER TABLE [Arquivos] ALTER COLUMN [ArquUID] DROP ROWGUIDCOL;");

            migrationBuilder.DropColumn(
                name: "ArquAtivo",
                table: "Arquivos");

            migrationBuilder.DropColumn(
                name: "ArquAutor",
                table: "Arquivos");

            migrationBuilder.DropColumn(
                name: "ArquOrigem",
                table: "Arquivos");

            migrationBuilder.DropColumn(
                name: "ArquSize",
                table: "Arquivos");

            migrationBuilder.DropColumn(
                name: "ArquUID",
                table: "Arquivos");

            migrationBuilder.RenameColumn(
                name: "ArquMomento",
                table: "Arquivos",
                newName: "CriadoEm");

            migrationBuilder.RenameColumn(
                name: "ArquFileName",
                table: "Arquivos",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "ArquContentType",
                table: "Arquivos",
                newName: "ContentType");

            migrationBuilder.RenameColumn(
                name: "ArquBytes",
                table: "Arquivos",
                newName: "Bytes");

            migrationBuilder.RenameColumn(
                name: "ArquId",
                table: "Arquivos",
                newName: "Id");
        }
    }
}
