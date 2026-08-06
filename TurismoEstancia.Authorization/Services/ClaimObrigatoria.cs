using Microsoft.AspNetCore.Authorization;

namespace TurismoEstancia.Authorization.Services;

/// <summary>
/// Requisito de autorização: o usuário precisa possuir a claim de perfil
/// com o valor exigido (ex.: "Gerenciador", "Operador").
/// </summary>
public class ClaimObrigatoria : IAuthorizationRequirement
{
    public ClaimObrigatoria(string claimValue) => ClaimValue = claimValue;

    /// <summary>Valor da claim de perfil exigida.</summary>
    public string ClaimValue { get; }
}
