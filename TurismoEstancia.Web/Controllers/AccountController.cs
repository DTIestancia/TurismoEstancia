using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Authorization.Services;
using TurismoEstancia.IdentityClass.Models;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Controllers;

/// <summary>
/// Autenticação própria do painel (sem auto-registro público — contas são
/// criadas pelo Gerenciador). Rotas fixas casam com o LoginPath configurado
/// em IdentityExtensions (/Identity/Account/Login, /Logout, /AccessDenied).
/// </summary>
public class AccountController : Controller
{
    private readonly SignInManager<Usuario> _signIn;
    private readonly UserManager<Usuario> _userManager;

    public AccountController(SignInManager<Usuario> signIn, UserManager<Usuario> userManager)
    {
        _signIn = signIn;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("Identity/Account/Login")]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await RedirectToAreaHomeAsync();
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Route("Identity/Account/Login")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var usuario = await _userManager.FindByNameAsync(model.Email);
        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var resultado = await _signIn.PasswordSignInAsync(
            usuario, model.Senha, isPersistent: model.Lembrar, lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Determina a área pelo perfil do usuário no banco (o principal da
            // request pode não estar atualizado imediatamente após o sign-in).
            // Com múltiplas claims (Gerenciador + Operador), o maior privilégio vence.
            var claims = await _userManager.GetClaimsAsync(usuario);
            var perfil = claims.FirstOrDefault(c => c.Type == Perfis.TipoClaim && c.Value == Perfis.Gerenciador)?.Value
                ?? claims.FirstOrDefault(c => c.Type == Perfis.TipoClaim)?.Value;
            return perfil == Perfis.Gerenciador
                ? RedirectToAction("Index", "Dashboard", new { area = "Gerenciador" })
                : RedirectToAction("Index", "Dashboard", new { area = "Operador" });
        }

        ModelState.AddModelError(string.Empty, resultado.IsLockedOut
            ? "Conta bloqueada temporariamente por muitas tentativas. Tente novamente em alguns minutos."
            : "E-mail ou senha inválidos.");

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Identity/Account/Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [Route("Identity/Account/AccessDenied")]
    public IActionResult AccessDenied() => View();

    /// <summary>Encaminha para a área conforme o perfil do usuário autenticado.</summary>
    private async Task<IActionResult> RedirectToAreaHomeAsync()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is not null)
        {
            var claims = await _userManager.GetClaimsAsync(usuario);
            if (claims.Any(c => c.Type == Perfis.TipoClaim && c.Value == Perfis.Gerenciador))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Gerenciador" });
            }
        }
        return RedirectToAction("Index", "Dashboard", new { area = "Operador" });
    }
}
