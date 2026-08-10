namespace TurismoEstancia.Web.Models;

/// <summary>
/// Chave conhecida do sistema exibida nos seletores de Criar/Editar do
/// Gerenciador (Configurações, Conteúdos e Categorias), para o usuário
/// escolher em vez de digitar a chave de memória.
/// </summary>
public class ChaveDisponivelViewModel
{
    public required string Chave { get; init; }
    public required string Nome { get; init; }

    /// <summary>True quando a configuração espera arquivo (PDF/vídeo/logo). Só usado em Configurações.</summary>
    public bool EhArquivo { get; init; }

    /// <summary>
    /// True quando a chave já está cadastrada no sistema (por outro registro) e
    /// deve aparecer desabilitada no select para evitar chave duplicada.
    /// </summary>
    public bool EmUso { get; init; }

    /// <summary>Nome do grupo (optgroup) quando o select é organizado por seção (ex.: tipos de contato).</summary>
    public string? Grupo { get; init; }

    /// <summary>True quando o valor é um id de arquivo (imagem) em vez de texto livre — a tela vira upload.</summary>
    public bool EhImagem { get; init; }
}
