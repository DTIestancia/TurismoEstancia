using System.ComponentModel.DataAnnotations;
using TurismoEstancia.Identity.Models;

namespace TurismoEstancia.Web.Models;

/// <summary>Usuário + perfil (claim) para a listagem.</summary>
public class UsuarioItemViewModel
{
    public Usuario Usuario { get; set; } = null!;
    public string Perfil { get; set; } = "—";
}

/// <summary>Formulário de criação de usuário do painel (sem auto-registro).</summary>
public class CriarUsuarioViewModel
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail (login)")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Perfil de acesso")]
    public string Perfil { get; set; } = "Operador";
}
