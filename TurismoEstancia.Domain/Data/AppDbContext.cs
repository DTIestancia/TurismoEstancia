using Microsoft.EntityFrameworkCore;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.Data;

/// <summary>
/// Contexto de dados do domínio (bounded context principal do portal de turismo).
/// Convenções do padrão: datas GETDATE(), bools default true, enums como string,
/// FKs Restrict/SetNull (Cascade apenas em filhos próprios) e índices únicos.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Arquivo> Arquivos => Set<Arquivo>();
    public DbSet<CategoriaPontoTuristico> CategoriasPontosTuristicos => Set<CategoriaPontoTuristico>();
    public DbSet<PontoTuristico> PontosTuristicos => Set<PontoTuristico>();
    public DbSet<PontoTuristicoMidia> PontoTuristicoMidias => Set<PontoTuristicoMidia>();
    public DbSet<HorarioFuncionamento> HorariosFuncionamento => Set<HorarioFuncionamento>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Slide> Slides => Set<Slide>();
    public DbSet<Estatistica> Estatisticas => Set<Estatistica>();
    public DbSet<GrupoCultural> GruposCulturais => Set<GrupoCultural>();
    public DbSet<PratoTuristico> PratosTuristicos => Set<PratoTuristico>();
    public DbSet<TagCultural> TagsCulturais => Set<TagCultural>();
    public DbSet<ConteudoSite> ConteudosSite => Set<ConteudoSite>();
    public DbSet<ConfiguracaoSite> ConfiguracoesSite => Set<ConfiguracaoSite>();
    public DbSet<Contato> Contatos => Set<Contato>();
    public DbSet<InscricaoNewsletter> InscricoesNewsletter => Set<InscricaoNewsletter>();
    public DbSet<Noticia> Noticias => Set<Noticia>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
    public DbSet<Roteiro> Roteiros => Set<Roteiro>();
    public DbSet<RoteiroItem> RoteiroItens => Set<RoteiroItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureArquivo(modelBuilder);
        ConfigureCategoriaPontoTuristico(modelBuilder);
        ConfigurePontoTuristico(modelBuilder);
        ConfigurePontoTuristicoMidia(modelBuilder);
        ConfigureHorarioFuncionamento(modelBuilder);
        ConfigureEvento(modelBuilder);
        ConfigureSlide(modelBuilder);
        ConfigureEstatistica(modelBuilder);
        ConfigureGrupoCultural(modelBuilder);
        ConfigurePratoTuristico(modelBuilder);
        ConfigureTagCultural(modelBuilder);
        ConfigureConteudoSite(modelBuilder);
        ConfigureConfiguracaoSite(modelBuilder);
        ConfigureContato(modelBuilder);
        ConfigureInscricaoNewsletter(modelBuilder);
        ConfigureNoticia(modelBuilder);
        ConfigureAvaliacao(modelBuilder);
        ConfigureRoteiro(modelBuilder);
        ConfigureRoteiroItem(modelBuilder);
    }

    /// <summary>
    /// Arquivo segue o padrão PrefeituraDigital: colunas Arqu*. O ArquUID é
    /// uniqueidentifier com default NEWID() e será o ROWGUIDCOL (aplicado via
    /// SQL na migração), pré-requisito para o ArquBytes virar FILESTREAM.
    /// </summary>
    private static void ConfigureArquivo(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Arquivo>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ArquId");
            entity.Property(e => e.UID).HasColumnName("ArquUID")
                  .HasColumnType("uniqueidentifier")
                  .HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Nome).HasColumnName("ArquFileName").HasMaxLength(255).IsRequired();
            entity.Property(e => e.ContentType).HasColumnName("ArquContentType").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Size).HasColumnName("ArquSize");
            entity.Property(e => e.Bytes).HasColumnName("ArquBytes").IsRequired();
            entity.Property(e => e.Autor).HasColumnName("ArquAutor").HasMaxLength(150);
            entity.Property(e => e.Origem).HasColumnName("ArquOrigem").HasMaxLength(50);
            entity.Property(e => e.Ativo).HasColumnName("ArquAtivo").HasDefaultValue(true);
            entity.Property(e => e.CriadoEm).HasColumnName("ArquMomento").HasDefaultValueSql("GETDATE()");
        });
    }

    private static void ConfigureCategoriaPontoTuristico(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriaPontoTuristico>(entity =>
        {
            entity.Property(e => e.Chave).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nome).HasMaxLength(150).IsRequired();
            entity.Property(e => e.SubTitulo).HasMaxLength(255);
            entity.Property(e => e.Cor).HasMaxLength(20);
            entity.Property(e => e.Icone).HasMaxLength(50);

            entity.HasIndex(e => e.Chave).IsUnique();
            entity.Property(e => e.ApresentarEmMaravilhas).HasDefaultValue(true);
            entity.Property(e => e.ExibirNoMapa).HasDefaultValue(true);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
        });
    }

    private static void ConfigurePontoTuristico(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PontoTuristico>(entity =>
        {
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.Detalhe).HasMaxLength(4000);
            entity.Property(e => e.Tag).HasMaxLength(100);
            entity.Property(e => e.Icone).HasMaxLength(50);
            entity.Property(e => e.Endereco).HasMaxLength(255);
            entity.Property(e => e.ComoChegar).HasMaxLength(1000);
            entity.Property(e => e.LeftPercent).HasDefaultValue(50);
            entity.Property(e => e.TopPercent).HasDefaultValue(50);
            entity.Property(e => e.ExibirNoMapa).HasDefaultValue(true);
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            entity.HasOne(e => e.Categoria)
                  .WithMany(c => c.PontosTuristicos)
                  .HasForeignKey(e => e.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePontoTuristicoMidia(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PontoTuristicoMidia>(entity =>
        {
            entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20);

            // Filho próprio: Cascade.
            entity.HasOne(e => e.PontoTuristico)
                  .WithMany(p => p.Midias)
                  .HasForeignKey(e => e.PontoTuristicoId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Referência compartilhada (Arquivo): Restrict.
            entity.HasOne(e => e.Arquivo)
                  .WithMany()
                  .HasForeignKey(e => e.ArquivoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // No máximo 1 Capa por ponto (índice único filtrado).
            entity.HasIndex(e => new { e.PontoTuristicoId, e.Tipo })
                  .IsUnique()
                  .HasFilter("[Tipo] = N'Capa'");
        });
    }

    private static void ConfigureHorarioFuncionamento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HorarioFuncionamento>(entity =>
        {
            entity.Property(e => e.DiaSemana).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.HoraInicio).HasColumnType("time");
            entity.Property(e => e.HoraFim).HasColumnType("time");
            entity.Property(e => e.Fechado).HasDefaultValue(false);

            // Filho próprio: Cascade.
            entity.HasOne(e => e.PontoTuristico)
                  .WithMany(p => p.Horarios)
                  .HasForeignKey(e => e.PontoTuristicoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureEvento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Evento>(entity =>
        {
            entity.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(4000);
            entity.Property(e => e.Local).HasMaxLength(200);
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            entity.HasIndex(e => e.DataInicio);
        });
    }

    private static void ConfigureSlide(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Slide>(entity =>
        {
            entity.Property(e => e.Titulo).HasMaxLength(200);
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            entity.HasOne(e => e.Imagem)
                  .WithMany()
                  .HasForeignKey(e => e.ImagemArquivoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEstatistica(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Estatistica>(entity =>
        {
            entity.Property(e => e.Valor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Legenda).HasMaxLength(100);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
        });
    }

    private static void ConfigureGrupoCultural(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GrupoCultural>(entity =>
        {
            entity.Property(e => e.Nome).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(2000);
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            // Referência compartilhada (Arquivo): Restrict.
            entity.HasOne(e => e.Imagem)
                  .WithMany()
                  .HasForeignKey(e => e.ImagemArquivoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigurePratoTuristico(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PratoTuristico>(entity =>
        {
            entity.Property(e => e.Nome).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(2000);
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            // Referência compartilhada (Arquivo): Restrict.
            entity.HasOne(e => e.Imagem)
                  .WithMany()
                  .HasForeignKey(e => e.ImagemArquivoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureTagCultural(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TagCultural>(entity =>
        {
            entity.Property(e => e.Nome).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(2000);
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            // Referência compartilhada (Arquivo): Restrict.
            entity.HasOne(e => e.Imagem)
                  .WithMany()
                  .HasForeignKey(e => e.ImagemArquivoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureConteudoSite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConteudoSite>(entity =>
        {
            entity.Property(e => e.Chave).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            // Texto livre de seção (história, cultura, gastronomia) — sem limite curto.
            entity.Property(e => e.Texto).HasColumnType("nvarchar(max)");

            entity.HasIndex(e => e.Chave).IsUnique();
        });
    }

    private static void ConfigureConfiguracaoSite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConfiguracaoSite>(entity =>
        {
            entity.Property(e => e.Chave).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ValorTexto).HasMaxLength(1000);

            entity.HasIndex(e => e.Chave).IsUnique();

            entity.HasOne(e => e.Arquivo)
                  .WithMany()
                  .HasForeignKey(e => e.ArquivoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureContato(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contato>(entity =>
        {
            entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Rotulo).HasMaxLength(100);
            entity.Property(e => e.Valor).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Icone).HasMaxLength(50);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
        });
    }

    private static void ConfigureInscricaoNewsletter(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InscricaoNewsletter>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Origem).HasMaxLength(50);
            entity.Property(e => e.ConsentimentoLgpd).HasDefaultValue(false);
            entity.Property(e => e.DataInscricao).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    private static void ConfigureNoticia(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Noticia>(entity =>
        {
            entity.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Resumo).HasMaxLength(500);
            entity.Property(e => e.Corpo).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Slug).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DataPublicacao).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Publicada).HasDefaultValue(false);
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            entity.HasIndex(e => e.Slug).IsUnique();

            entity.HasOne(e => e.Imagem)
                  .WithMany()
                  .HasForeignKey(e => e.ImagemArquivoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureAvaliacao(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Avaliacao>(entity =>
        {
            entity.Property(e => e.Nome).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Nota).HasDefaultValue(5);
            entity.Property(e => e.Comentario).HasMaxLength(1000);
            entity.Property(e => e.Data).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Aprovada).HasDefaultValue(false);

            // Filho próprio: Cascade.
            entity.HasOne(e => e.PontoTuristico)
                  .WithMany(p => p.Avaliacoes)
                  .HasForeignKey(e => e.PontoTuristicoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRoteiro(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Roteiro>(entity =>
        {
            entity.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            entity.HasOne(e => e.Imagem)
                  .WithMany()
                  .HasForeignKey(e => e.ImagemArquivoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureRoteiroItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoteiroItem>(entity =>
        {
            entity.Property(e => e.Dia).HasDefaultValue(1);
            entity.Property(e => e.Observacao).HasMaxLength(500);

            // Filho próprio: Cascade.
            entity.HasOne(e => e.Roteiro)
                  .WithMany(r => r.Itens)
                  .HasForeignKey(e => e.RoteiroId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Referência compartilhada (PontoTuristico): Restrict.
            entity.HasOne(e => e.PontoTuristico)
                  .WithMany()
                  .HasForeignKey(e => e.PontoTuristicoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
