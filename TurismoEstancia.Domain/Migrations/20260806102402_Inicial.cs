using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurismoEstancia.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arquivos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Bytes = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arquivos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasPontosTuristicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SubTitulo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Cor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Icone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApresentarEmMaravilhas = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExibirNoMapa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasPontosTuristicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rotulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Valor = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Icone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contatos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConteudosSite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Chave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConteudosSite", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estatisticas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Valor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Legenda = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estatisticas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Eventos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Local = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eventos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GruposCulturais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposCulturais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InscricoesNewsletter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Origem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConsentimentoLgpd = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DataInscricao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscricoesNewsletter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PratosTuristicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PratosTuristicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagsCulturais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagsCulturais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesSite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Chave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValorTexto = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ArquivoId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesSite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracoesSite_Arquivos_ArquivoId",
                        column: x => x.ArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Noticias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Resumo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Corpo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagemArquivoId = table.Column<long>(type: "bigint", nullable: true),
                    DataPublicacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Publicada = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Noticias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Noticias_Arquivos_ImagemArquivoId",
                        column: x => x.ImagemArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Roteiros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImagemArquivoId = table.Column<long>(type: "bigint", nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roteiros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roteiros_Arquivos_ImagemArquivoId",
                        column: x => x.ImagemArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Slides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImagemArquivoId = table.Column<long>(type: "bigint", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Slides_Arquivos_ImagemArquivoId",
                        column: x => x.ImagemArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PontosTuristicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Detalhe = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Icone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Endereco = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ComoChegar = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LeftPercent = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    TopPercent = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    ExibirNoMapa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PontosTuristicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PontosTuristicos_CategoriasPontosTuristicos_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "CategoriasPontosTuristicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PontoTuristicoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Nota = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Aprovada = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_PontosTuristicos_PontoTuristicoId",
                        column: x => x.PontoTuristicoId,
                        principalTable: "PontosTuristicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorariosFuncionamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PontoTuristicoId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: true),
                    HoraFim = table.Column<TimeOnly>(type: "time", nullable: true),
                    Fechado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosFuncionamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosFuncionamento_PontosTuristicos_PontoTuristicoId",
                        column: x => x.PontoTuristicoId,
                        principalTable: "PontosTuristicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PontoTuristicoMidias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PontoTuristicoId = table.Column<int>(type: "int", nullable: false),
                    ArquivoId = table.Column<long>(type: "bigint", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PontoTuristicoMidias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PontoTuristicoMidias_Arquivos_ArquivoId",
                        column: x => x.ArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PontoTuristicoMidias_PontosTuristicos_PontoTuristicoId",
                        column: x => x.PontoTuristicoId,
                        principalTable: "PontosTuristicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoteiroItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoteiroId = table.Column<int>(type: "int", nullable: false),
                    PontoTuristicoId = table.Column<int>(type: "int", nullable: false),
                    Dia = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoteiroItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoteiroItens_PontosTuristicos_PontoTuristicoId",
                        column: x => x.PontoTuristicoId,
                        principalTable: "PontosTuristicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoteiroItens_Roteiros_RoteiroId",
                        column: x => x.RoteiroId,
                        principalTable: "Roteiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_PontoTuristicoId",
                table: "Avaliacoes",
                column: "PontoTuristicoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesSite_ArquivoId",
                table: "ConfiguracoesSite",
                column: "ArquivoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesSite_Chave",
                table: "ConfiguracoesSite",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConteudosSite_Chave",
                table: "ConteudosSite",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_DataInicio",
                table: "Eventos",
                column: "DataInicio");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosFuncionamento_PontoTuristicoId",
                table: "HorariosFuncionamento",
                column: "PontoTuristicoId");

            migrationBuilder.CreateIndex(
                name: "IX_InscricoesNewsletter_Email",
                table: "InscricoesNewsletter",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_ImagemArquivoId",
                table: "Noticias",
                column: "ImagemArquivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_Slug",
                table: "Noticias",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PontosTuristicos_CategoriaId",
                table: "PontosTuristicos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_PontoTuristicoMidias_ArquivoId",
                table: "PontoTuristicoMidias",
                column: "ArquivoId");

            migrationBuilder.CreateIndex(
                name: "IX_PontoTuristicoMidias_PontoTuristicoId_Tipo",
                table: "PontoTuristicoMidias",
                columns: new[] { "PontoTuristicoId", "Tipo" },
                unique: true,
                filter: "[Tipo] = N'Capa'");

            migrationBuilder.CreateIndex(
                name: "IX_RoteiroItens_PontoTuristicoId",
                table: "RoteiroItens",
                column: "PontoTuristicoId");

            migrationBuilder.CreateIndex(
                name: "IX_RoteiroItens_RoteiroId",
                table: "RoteiroItens",
                column: "RoteiroId");

            migrationBuilder.CreateIndex(
                name: "IX_Roteiros_ImagemArquivoId",
                table: "Roteiros",
                column: "ImagemArquivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Slides_ImagemArquivoId",
                table: "Slides",
                column: "ImagemArquivoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "ConfiguracoesSite");

            migrationBuilder.DropTable(
                name: "Contatos");

            migrationBuilder.DropTable(
                name: "ConteudosSite");

            migrationBuilder.DropTable(
                name: "Estatisticas");

            migrationBuilder.DropTable(
                name: "Eventos");

            migrationBuilder.DropTable(
                name: "GruposCulturais");

            migrationBuilder.DropTable(
                name: "HorariosFuncionamento");

            migrationBuilder.DropTable(
                name: "InscricoesNewsletter");

            migrationBuilder.DropTable(
                name: "Noticias");

            migrationBuilder.DropTable(
                name: "PontoTuristicoMidias");

            migrationBuilder.DropTable(
                name: "PratosTuristicos");

            migrationBuilder.DropTable(
                name: "RoteiroItens");

            migrationBuilder.DropTable(
                name: "Slides");

            migrationBuilder.DropTable(
                name: "TagsCulturais");

            migrationBuilder.DropTable(
                name: "PontosTuristicos");

            migrationBuilder.DropTable(
                name: "Roteiros");

            migrationBuilder.DropTable(
                name: "CategoriasPontosTuristicos");

            migrationBuilder.DropTable(
                name: "Arquivos");
        }
    }
}
