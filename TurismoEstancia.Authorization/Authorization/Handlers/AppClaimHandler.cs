using Microsoft.AspNetCore.Authorization;

namespace TurismoEstancia.Authorization.Services
{
    /// <summary>
    /// Handler de autorização baseado em claims. Concede acesso quando o usuário
    /// possui uma claim <see cref="Perfis.TipoClaim"/> com o ClaimName exigido
    /// pelo requisito.
    /// </summary>
    public class AppClaimHandler : AuthorizationHandler<ClaimObrigatoria>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ClaimObrigatoria requirement)
        {
            var claim = context.User.FindFirst(
                c => c.Type.Equals(Perfis.TipoClaim, System.StringComparison.Ordinal)
                     && c.Value.Equals(requirement.ClaimName, System.StringComparison.Ordinal));

            if (claim != null)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
