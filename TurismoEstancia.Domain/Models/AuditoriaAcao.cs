namespace TurismoEstancia.Domain.Models;

public class AuditoriaAcao
{
    public long Id { get; set; }
    public DateTime DataHoraUtc { get; set; }
    public string Tipo { get; set; } = null!;
    public string Area { get; set; } = null!;
    public string? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public string? Perfis { get; set; }
    public string? Rota { get; set; }
    public string? MetodoHttp { get; set; }
    public int? StatusHttp { get; set; }
    public long? DuracaoMs { get; set; }
    public string? EnderecoIp { get; set; }
    public string? Navegador { get; set; }
    public string? Correlacao { get; set; }
    public string? Entidade { get; set; }
    public string? EntidadeId { get; set; }
    public string? ValoresAnterioresJson { get; set; }
    public string? ValoresNovosJson { get; set; }
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
}
