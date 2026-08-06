using Microsoft.AspNetCore.Authorization;

namespace TurismoEstancia.Authorization.Services;

/// <summary>
/// Handler de autorização baseado em claims. Concede acesso quando o usuário
/// possui uma claim <see cref="Perfis.TipoClaim"/> com o valor exigido pelo requisito.
/// </summary>
public class AppClaimHandler : AuthorizationHandler<ClaimObrigatoria>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClaimObrigatoria requirement)
    {
        if (context.User.HasClaim(c => c.Type == Perfis.TipoClaim && c.Value == requirement.ClaimValue))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
