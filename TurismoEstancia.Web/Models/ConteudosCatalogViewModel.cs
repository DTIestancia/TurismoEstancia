namespace TurismoEstancia.Web.Models;

/// <summary>
/// Catálogo de textos do portal agrupado por seção do site, para o Gerenciador
/// editar sem precisar decorar as chaves (ex.: a seção "Nossa Cidade" usa as
/// chaves <c>historia-*</c>). Mostra também as chaves ainda não cadastradas
/// (que hoje caem no texto padrão do protótipo) com atalho para criá-las.
/// </summary>
public class ConteudosCatalogViewModel
{
    public IReadOnlyList<SecaoTextos> Secoes { get; set; } = Array.Empty<SecaoTextos>();

    public sealed class SecaoTextos
    {
        public required string Titulo { get; init; }
        public required string Descricao { get; init; }
        public required IReadOnlyList<ItemTexto> Itens { get; init; }
    }

    public sealed class ItemTexto
    {
        public required string Chave { get; init; }
        public required string Nome { get; init; }
        public string? Descricao { get; init; }

        /// <summary>Texto salvo no banco (null quando a chave ainda não foi cadastrada).</summary>
        public string? Texto { get; init; }

        /// <summary>Id do registro no banco (null quando a chave ainda não foi cadastrada).</summary>
        public int? Id { get; init; }

        public bool Cadastrado => Id is not null;
    }
}
