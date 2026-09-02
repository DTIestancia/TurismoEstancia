namespace TurismoEstancia.Domain.Models;

/// <summary>Tipos de mídia de um ponto turístico.</summary>
public enum TipoMidia
{
    /// <summary>Imagem principal (no máximo uma por ponto).</summary>
    Capa,

    /// <summary>Pictograma/ícone ilustrado.</summary>
    Pictograma,

    /// <summary>Foto extra da galeria do ponto.</summary>
    Galeria
}

/// <summary>Dias da semana para horários de funcionamento.</summary>
public enum DiaSemana
{
    Domingo,
    Segunda,
    Terca,
    Quarta,
    Quinta,
    Sexta,
    Sabado
}

/// <summary>Tipo de valor de uma configuração de site.</summary>
public enum TipoConfiguracao
{
    /// <summary>Valor armazenado como texto (ex.: título do site, meta descrição).</summary>
    Texto,

    /// <summary>Valor armazenado como arquivo (ex.: guia PDF, vídeo institucional).</summary>
    Arquivo
}

/// <summary>Abas da seção "Conheça Estância" da home.</summary>
public enum CategoriaConhecaEstancia
{
    /// <summary>Aba História.</summary>
    Historia,

    /// <summary>Aba Cultura.</summary>
    Cultura,

    /// <summary>Aba Gastronomia.</summary>
    Gastronomia,

    /// <summary>Aba Experiências.</summary>
    Experiencias
}

/// <summary>Tipos de contato exibidos no rodapé.</summary>
public enum TipoContato
{
    /// <summary>Endereço físico.</summary>
    Endereco,

    /// <summary>Telefone/emergência (190, SAMU, SMTT, CIT...).</summary>
    Telefone,

    /// <summary>Link de rede social (Instagram, Facebook, YouTube, WhatsApp).</summary>
    RedesSocial,

    /// <summary>E-mail de contato (renderizado com ícone de envelope e link mailto).</summary>
    Email
}
