namespace TurismoEstancia.Authorization.Services;

/// <summary>
/// Constantes de perfil. Os perfis são atribuídos via claims
/// (tipo <see cref="TipoClaim"/>), nunca via roles literais.
/// </summary>
public static class Perfis
{
    /// <summary>Tipo da claim que armazena o perfil do usuário.</summary>
    public const string TipoClaim = "Perfil";

    /// <summary>Acesso total ao CMS.</summary>
    public const string Gerenciador = "Gerenciador";

    /// <summary>Acesso apenas a Evento e InscricaoNewsletter.</summary>
    public const string Operador = "Operador";
}
