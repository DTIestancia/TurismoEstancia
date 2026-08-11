using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.IdentityClass.Models
{
    public class Usuario : IdentityUser
    {

        [Display(Name = "Documento")]
        [Required(ErrorMessage = "Campo {0} requerido.")]
        [Remote("VerificarCpfCnpjExist", "Account", "Gerenciador", HttpMethod = "POST", ErrorMessage = "Documento já cadastrado.")]
        public string CpfCnpj { get; set; }
        public string Nome { get; set; }
        public byte[] FotoPerfil { get; set; }

    }
}
