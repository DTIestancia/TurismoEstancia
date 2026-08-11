using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurismoEstancia.IdentityClass.Models
{
    public partial class AspNetUsers
    {
        public AspNetUsers()
        {
            AspNetUserClaims = new HashSet<AspNetUserClaims>();
            AspNetUserLogins = new HashSet<AspNetUserLogins>();
            AspNetUserRoles = new HashSet<AspNetUserRoles>();
            AspNetUserTokens = new HashSet<AspNetUserTokens>();
        }

        public string Id { get; set; }
        [Display(Name = "USUÁRIO"), MaxLength(256)]
        [Required(ErrorMessage = "Campo {0} obrigatório.")]
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }

        [Display(Name = "Email"), MaxLength(256)]
        [Required(ErrorMessage = "Campo {0} obrigatório.")]
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }

        [DataType(DataType.Password)]
        [StringLength(18, ErrorMessage = "A {0} deve ter no minimo {2} caracteres.", MinimumLength = 6)]
        [RegularExpression(@"^((?=.*[a-z])(?=.*\d)(?=.*[@#$%^&*+=])).+$", ErrorMessage = "Na senha é necessário pelo menos um número, uma letra (maiúscula ou minúscula) e um caracteres especial.")]
        [Display(Name = "Senha"), MaxLength(256)]
        // [Required(ErrorMessage = "Campo {0} obrigatório.")]
        public string PasswordHash { get; set; }

        [DataType(DataType.Password)]
        [StringLength(18, ErrorMessage = "A {0} deve ter no minimo {2} caracteres.", MinimumLength = 6)]
        [Display(Name = "Senha"), MaxLength(256)]
        [Required(ErrorMessage = "Campo {0} obrigatório.")]
        [Compare("PasswordHash", ErrorMessage = "As senhas não coincidem")]
        [NotMapped]
        public string PasswordConfirm { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }

        [Display(Name = "FONE"), MaxLength(15)]
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        [Display(Name = "DOCUMENTO"), MaxLength(20)]
        [Remote("VerificarCpfGerenciadorCnpjExist", "Account", "Gerenciador", AdditionalFields = "Id", HttpMethod = "POST", ErrorMessage = "Documento já cadastrado.")]
        //[Required(ErrorMessage = "Campo {0} obrigatório.")]
        public string CpfCnpj { get; set; }

        [NotMapped]
        [Display(Name = "TIPO")]
        public string TipoUsuario { get; set; }

        [Display(Name = "NOME"), MaxLength(256)]
        //[Required(ErrorMessage = "Campo {0} obrigatório.")]
        public string Nome { get; set; }

        [FromForm]
        public byte[] FotoPerfil { get; set; }

        public virtual ICollection<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual ICollection<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual ICollection<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual ICollection<AspNetUserTokens> AspNetUserTokens { get; set; }
    }
}
