using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Services.Conteudo.Interfaces;

/// <summary>Serviço de contatos do rodapé (endereço, telefones, redes sociais).</summary>
public interface IContatoService
{
    Task<IReadOnlyList<ContatoDto>> ListarAsync(TipoContato? tipo = null, CancellationToken ct = default);
    Task<ContatoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(ContatoDto dto, CancellationToken ct = default);

    /// <summary>Oculta o contato do rodapé (Ativo = false).</summary>
    Task OcultarAsync(int id, CancellationToken ct = default);

    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Exclusão definitiva do contato.</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
