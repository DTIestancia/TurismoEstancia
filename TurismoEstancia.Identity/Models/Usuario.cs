using Microsoft.AspNetCore.Identity;

namespace TurismoEstancia.Identity.Models;

/// <summary>
/// Usuário do sistema. O perfil (Gerenciador/Operador) é atribuído via claims.
/// </summary>
public class Usuario : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;
}
