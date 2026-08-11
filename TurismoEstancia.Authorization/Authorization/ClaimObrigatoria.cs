using Microsoft.AspNetCore.Authorization;

namespace TurismoEstancia.Authorization.Services
{
    /// <summary>
    /// Requisito de autorização: o usuário precisa possuir a claim de perfil
    /// (tipo <see cref="Perfis.TipoClaim"/>) com o ClaimName exigido
    /// (ex.: <see cref="Perfis.Gerenciador"/>, <see cref="Perfis.Operador"/>).
    /// </summary>
    public class ClaimObrigatoria : IAuthorizationRequirement
    {
        public string ClaimName { get; }
        public ClaimObrigatoria(string claimName)
        {
            ClaimName = claimName;
        }
    }
}
