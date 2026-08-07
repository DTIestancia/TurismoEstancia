using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Comunicacao.Interfaces;

namespace TurismoEstancia.Services.Comunicacao.Services;

/// <summary>Implementação do serviço da newsletter.</summary>
public class InscricaoNewsletterService : IInscricaoNewsletterService
{
    private readonly AppDbContext _db;

    public InscricaoNewsletterService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<InscricaoNewsletter, InscricaoNewsletterDto>> ToDto =
        i => new InscricaoNewsletterDto
        {
            Id = i.Id,
            Email = i.Email,
            Origem = i.Origem,
            ConsentimentoLgpd = i.ConsentimentoLgpd,
            DataInscricao = i.DataInscricao,
            Ativo = i.Ativo
        };

    public async Task<IReadOnlyList<InscricaoNewsletterDto>> ListarAsync(bool incluirInativos = false, CancellationToken ct = default) =>
        await _db.InscricoesNewsletter.AsNoTracking()
            .Where(i => incluirInativos || i.Ativo)
            .OrderByDescending(i => i.DataInscricao)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<InscricaoNewsletterDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.InscricoesNewsletter.AsNoTracking()
            .Where(i => i.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task InscreverAsync(string email, string? origem, bool consentimentoLgpd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Informe um e-mail válido.");

        if (!consentimentoLgpd)
            throw new InvalidOperationException("É necessário consentir com a LGPD para receber a newsletter.");

        var existente = await _db.InscricoesNewsletter.FirstOrDefaultAsync(i => i.Email == email.Trim(), ct);

        if (existente is null)
        {
            _db.InscricoesNewsletter.Add(new InscricaoNewsletter
            {
                Email = email.Trim(),
                Origem = origem,
                ConsentimentoLgpd = true,
                Ativo = true
            });
        }
        else
        {
            // Reenvio reativa em vez de duplicar.
            existente.Ativo = true;
            existente.ConsentimentoLgpd = true;
            existente.Origem = origem;
            existente.DataInscricao = DateTime.Now;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task InativarAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.InscricoesNewsletter.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException("Inscrição não encontrada.");
        entidade.Ativo = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReativarAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.InscricoesNewsletter.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException("Inscrição não encontrada.");
        entidade.Ativo = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<byte[]> ExportarCsvAsync(CancellationToken ct = default)
    {
        var inscricoes = await _db.InscricoesNewsletter.AsNoTracking()
            .Where(i => i.Ativo)
            .OrderByDescending(i => i.DataInscricao)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Email;Origem;DataInscricao");
        foreach (var i in inscricoes)
            sb.AppendLine($"{i.Email};{i.Origem};{i.DataInscricao:dd/MM/yyyy HH:mm}");

        // BOM UTF-8 para o Excel abrir corretamente com acentuação.
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return bytes;
    }

    public async Task<IReadOnlyList<string>> ListarEmailsAtivosAsync(CancellationToken ct = default) =>
        await _db.InscricoesNewsletter.AsNoTracking()
            .Where(i => i.Ativo && i.ConsentimentoLgpd)
            .OrderBy(i => i.Email)
            .Select(i => i.Email)
            .ToListAsync(ct);
}
