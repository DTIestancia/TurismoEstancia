using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Comunicacao.Interfaces;

/// <summary>
/// Serviço da newsletter. E-mail único — reenvio reativa a inscrição.
/// Exclusão = Ativo = false. Exportação em CSV com BOM (Excel).
/// </summary>
public interface IInscricaoNewsletterService
{
    Task<IReadOnlyList<InscricaoNewsletterDto>> ListarAsync(bool incluirInativos = false, CancellationToken ct = default);
    Task<InscricaoNewsletterDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Inscreve ou reativa um e-mail. Lança se o consentimento LGPD não for informado.</summary>
    Task InscreverAsync(string email, string? origem, bool consentimentoLgpd, CancellationToken ct = default);

    /// <summary>Inativa a inscrição (Ativo = false).</summary>
    Task InativarAsync(int id, CancellationToken ct = default);

    /// <summary>Reativa a inscrição (Ativo = true).</summary>
    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Exporta as inscrições ativas em CSV (com BOM UTF-8 para Excel).</summary>
    Task<byte[]> ExportarCsvAsync(CancellationToken ct = default);
}
