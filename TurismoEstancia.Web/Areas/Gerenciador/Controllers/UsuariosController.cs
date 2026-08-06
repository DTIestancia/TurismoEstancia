using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurismoEstancia.Authorization.Services;
using TurismoEstancia.Identity.Data;
using TurismoEstancia.Identity.Models;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class UsuariosController : PainelController
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IdentityContext _identity;

    public UsuariosController(IServiceProvider services, UserManager<Usuario> userManager, IdentityContext identity)
        : base(services)
    {
        _userManager = userManager;
        _identity = identity;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Usuários";
        var usuarios = await _identity.Users.AsNoTracking().OrderBy(u => u.NomeCompleto).ToListAsync(ct);

        var itens = new List<UsuarioItemViewModel>();
        foreach (var u in usuarios)
        {
            var claims = await _userManager.GetClaimsAsync(u);
            itens.Add(new UsuarioItemViewModel
            {
                Usuario = u,
                Perfil = claims.FirstOrDefault(c => c.Type == Perfis.TipoClaim)?.Value ?? "—"
            });
        }

        return View(itens);
    }

    public IActionResult Criar() => View(new CriarUsuarioViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarUsuarioViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var usuario = new Usuario
        {
            UserName = model.Email,
            Email = model.Email,
            NomeCompleto = model.NomeCompleto,
            EmailConfirmed = true
        };

        var resultado = await _userManager.CreateAsync(usuario, model.Senha);
        if (!resultado.Succeeded)
        {
            foreach (var erro in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, erro.Description);
            }
            return View(model);
        }

        // Perfil via claim (nunca roles literais).
        var perfil = model.Perfil == Perfis.Operador ? Perfis.Operador : Perfis.Gerenciador;
        await _userManager.AddClaimAsync(usuario, new System.Security.Claims.Claim(Perfis.TipoClaim, perfil));

        TempData["PainelOk"] = $"Usuário {model.Email} criado com perfil {perfil}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null)
        {
            TempData["PainelErro"] = "Usuário não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        if (usuario.Id == _userManager.GetUserId(User))
        {
            TempData["PainelErro"] = "Você não pode excluir o próprio usuário.";
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _userManager.DeleteAsync(usuario);
        TempData[resultado.Succeeded ? "PainelOk" : "PainelErro"] =
            resultado.Succeeded ? "Usuário excluído." : "Não foi possível excluir o usuário.";

        return RedirectToAction(nameof(Index));
    }
}
