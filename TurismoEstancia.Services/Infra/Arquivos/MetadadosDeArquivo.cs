namespace TurismoEstancia.Services.Infra.Arquivos;

/// <summary>
/// O que o endpoint precisa saber de um arquivo <b>sem trazer os bytes</b>
/// (Content-Type, tamanho e data de gravação). É o que permite decidir como servir
/// antes de tocar no binário — e o que compõe o ETag.
/// </summary>
public sealed record MetadadosDeArquivo(string ContentType, long Size, DateTime CriadoEm);

/// <summary>
/// Linha do acervo para relatórios de manutenção: identifica e mede o arquivo
/// <b>sem carregar o binário</b> (a tabela guarda tudo em <c>varbinary(max)</c>;
/// trazer os bytes só para contar órfãos seria carregar o acervo inteiro).
/// </summary>
public sealed record ResumoDeArquivo(long Id, string Nome, string ContentType, long Size, DateTime CriadoEm, string? Autor = null)
{
    /// <summary>True quando o arquivo é uma foto (o que o comando de recompressão trata).</summary>
    public bool EhFoto => Imagens.OtimizadorDeImagem.TiposDeFoto.Contains(ContentType);
}
