using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Infra.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Services.Turismo.Services;

/// <summary>Implementação do serviço de ponto turístico.</summary>
public class PontoTuristicoService : IPontoTuristicoService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public PontoTuristicoService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<PontoTuristico, PontoTuristicoDto>> ToDto =
        p => new PontoTuristicoDto
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            Detalhe = p.Detalhe,
            Tag = p.Tag,
            Icone = p.Icone,
            CategoriaId = p.CategoriaId,
            CategoriaNome = p.Categoria != null ? p.Categoria.Nome : null,
            CategoriaCor = p.Categoria != null ? p.Categoria.Cor : null,
            CategoriaIcone = p.Categoria != null ? p.Categoria.Icone : null,
            CategoriaApresentarEmMaravilhas = p.Categoria != null && p.Categoria.ApresentarEmMaravilhas,
            Endereco = p.Endereco,
            ComoChegar = p.ComoChegar,
            LeftPercent = p.LeftPercent,
            TopPercent = p.TopPercent,
            ExibirNoMapa = p.ExibirNoMapa,
            Ordem = p.Ordem,
            Ativo = p.Ativo
        };

    public async Task<IReadOnlyList<PontoTuristicoDto>> ListarAsync(bool apenasAtivos = true, CancellationToken ct = default)
    {
        var query = _db.PontosTuristicos.AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => !apenasAtivos || p.Ativo);

        var dtoList = await query
            .OrderBy(p => p.Categoria!.Ordem)
            .ThenBy(p => p.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

        await CarregarMidiasAsync(dtoList.Select(d => d.Id), dtoList, ct);
        return dtoList;
    }

    public async Task<IReadOnlyList<PontoTuristicoDto>> ListarParaMapaAsync(CancellationToken ct = default) =>
        await _db.PontosTuristicos.AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.Ativo && p.ExibirNoMapa)
            .OrderBy(p => p.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<PontoTuristicoDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.PontosTuristicos.AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

        if (dto is null) return null;

        dto.Midias = await _db.PontoTuristicoMidias.AsNoTracking()
            .Where(m => m.PontoTuristicoId == id)
            .OrderBy(m => m.Ordem)
            .Select(m => new PontoTuristicoMidiaDto
            {
                Id = m.Id,
                PontoTuristicoId = m.PontoTuristicoId,
                ArquivoId = m.ArquivoId,
                ArquivoNome = m.Arquivo != null ? m.Arquivo.Nome : null,
                Tipo = m.Tipo,
                Ordem = m.Ordem
            })
            .ToListAsync(ct);

        dto.Horarios = await _db.HorariosFuncionamento.AsNoTracking()
            .Where(h => h.PontoTuristicoId == id)
            .OrderBy(h => h.DiaSemana)
            .Select(h => new HorarioFuncionamentoDto
            {
                Id = h.Id,
                PontoTuristicoId = h.PontoTuristicoId,
                DiaSemana = h.DiaSemana,
                HoraInicio = h.HoraInicio,
                HoraFim = h.HoraFim,
                Fechado = h.Fechado
            })
            .ToListAsync(ct);

        return dto;
    }

    public async Task SalvarAsync(PontoTuristicoDto dto, IFormFile? capa, IFormFile? pictograma, IEnumerable<IFormFile> galeria, CancellationToken ct = default)
    {
        var arquivosParaExcluir = new List<long>();

        if (dto.Id == 0)
        {
            var novo = new PontoTuristico
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Detalhe = dto.Detalhe,
                Tag = dto.Tag,
                Icone = dto.Icone,
                CategoriaId = dto.CategoriaId,
                Endereco = dto.Endereco,
                ComoChegar = dto.ComoChegar,
                LeftPercent = dto.LeftPercent,
                TopPercent = dto.TopPercent,
                ExibirNoMapa = dto.ExibirNoMapa,
                Ordem = dto.Ordem,
                Ativo = true
            };

            _db.PontosTuristicos.Add(novo);
            await _db.SaveChangesAsync(ct);
            dto.Id = novo.Id;

            await SalvarMidiasAsync(novo.Id, capa, pictograma, galeria, arquivosParaExcluir, ct);
            await SalvarHorariosAsync(novo.Id, dto.Horarios, ct);
        }
        else
        {
            var entidade = await _db.PontosTuristicos.FirstOrDefaultAsync(p => p.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Ponto turístico não encontrado.");

            entidade.Nome = dto.Nome;
            entidade.Descricao = dto.Descricao;
            entidade.Detalhe = dto.Detalhe;
            entidade.Tag = dto.Tag;
            entidade.Icone = dto.Icone;
            entidade.CategoriaId = dto.CategoriaId;
            entidade.Endereco = dto.Endereco;
            entidade.ComoChegar = dto.ComoChegar;
            entidade.LeftPercent = dto.LeftPercent;
            entidade.TopPercent = dto.TopPercent;
            entidade.ExibirNoMapa = dto.ExibirNoMapa;
            entidade.Ordem = dto.Ordem;

            await SalvarMidiasAsync(dto.Id, capa, pictograma, galeria, arquivosParaExcluir, ct);
            await SalvarHorariosAsync(dto.Id, dto.Horarios, ct);
        }

        await _db.SaveChangesAsync(ct);

        // Só remove arquivos antigos DEPOIS de commitar a remoção das mídias,
        // senão a checagem de referência (no banco) impediria a exclusão.
        foreach (var id in arquivosParaExcluir)
            await _arquivos.ExcluirAsync(id, ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PontosTuristicos.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Ponto turístico não encontrado.");
        entidade.Ativo = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReativarAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PontosTuristicos.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Ponto turístico não encontrado.");
        entidade.Ativo = true;
        await _db.SaveChangesAsync(ct);
    }

    private async Task CarregarMidiasAsync(IEnumerable<int> ids, IReadOnlyList<PontoTuristicoDto> dtoList, CancellationToken ct)
    {
        var midias = await _db.PontoTuristicoMidias.AsNoTracking()
            .Where(m => ids.Contains(m.PontoTuristicoId))
            .OrderBy(m => m.Ordem)
            .ToListAsync(ct);

        foreach (var dto in dtoList)
        {
            dto.Midias = midias
                .Where(m => m.PontoTuristicoId == dto.Id)
                .Select(m => new PontoTuristicoMidiaDto
                {
                    Id = m.Id,
                    PontoTuristicoId = m.PontoTuristicoId,
                    ArquivoId = m.ArquivoId,
                    Tipo = m.Tipo,
                    Ordem = m.Ordem
                })
                .ToList();
        }
    }

    private async Task SalvarMidiasAsync(int pontoId, IFormFile? capa, IFormFile? pictograma, IEnumerable<IFormFile> galeria, List<long> arquivosParaExcluir, CancellationToken ct)
    {
        var novas = new List<(TipoMidia Tipo, IFormFile Arquivo)>();
        if (capa is { Length: > 0 }) novas.Add((TipoMidia.Capa, capa));
        if (pictograma is { Length: > 0 }) novas.Add((TipoMidia.Pictograma, pictograma));
        if (galeria is not null)
        {
            foreach (var arquivo in galeria.Where(f => f.Length > 0))
                novas.Add((TipoMidia.Galeria, arquivo));
        }

        foreach (var (tipo, arquivo) in novas)
        {
            // Capa/Pictograma são únicos: a mídia anterior do mesmo tipo é substituída.
            // Galeria é cumulativa: apenas adiciona, sem remover fotos existentes.
            PontoTuristicoMidia? anterior = null;
            if (tipo is TipoMidia.Capa or TipoMidia.Pictograma)
            {
                anterior = await _db.PontoTuristicoMidias
                    .FirstOrDefaultAsync(m => m.PontoTuristicoId == pontoId && m.Tipo == tipo, ct);
            }

            var arquivoId = await _arquivos.SalvarAsync(arquivo, ct);
            _db.PontoTuristicoMidias.Add(new PontoTuristicoMidia
            {
                PontoTuristicoId = pontoId,
                ArquivoId = arquivoId,
                Tipo = tipo,
                Ordem = (await _db.PontoTuristicoMidias.CountAsync(m => m.PontoTuristicoId == pontoId, ct)) + 1
            });

            if (anterior is not null)
            {
                var antigoId = anterior.ArquivoId;
                _db.PontoTuristicoMidias.Remove(anterior);
                arquivosParaExcluir.Add(antigoId);
            }
        }
    }

    private async Task SalvarHorariosAsync(int pontoId, List<HorarioFuncionamentoDto> horarios, CancellationToken ct)
    {
        var atuais = await _db.HorariosFuncionamento
            .Where(h => h.PontoTuristicoId == pontoId)
            .ToListAsync(ct);

        // Remove horários que não vieram no formulário.
        var idsRecebidos = horarios.Select(h => h.Id).ToHashSet();
        foreach (var atual in atuais.Where(h => !idsRecebidos.Contains(h.Id)))
            _db.HorariosFuncionamento.Remove(atual);

        foreach (var dto in horarios)
        {
            if (dto.Id > 0)
            {
                var existente = atuais.FirstOrDefault(h => h.Id == dto.Id);
                if (existente is not null)
                {
                    existente.DiaSemana = dto.DiaSemana;
                    existente.HoraInicio = dto.Fechado ? null : dto.HoraInicio;
                    existente.HoraFim = dto.Fechado ? null : dto.HoraFim;
                    existente.Fechado = dto.Fechado;
                }
            }
            else
            {
                _db.HorariosFuncionamento.Add(new HorarioFuncionamento
                {
                    PontoTuristicoId = pontoId,
                    DiaSemana = dto.DiaSemana,
                    HoraInicio = dto.Fechado ? null : dto.HoraInicio,
                    HoraFim = dto.Fechado ? null : dto.HoraFim,
                    Fechado = dto.Fechado
                });
            }
        }
    }
}
