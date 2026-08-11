using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.IdentityClass.Models
{
    public partial class AspNetUserRoles
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        [Display(Name = "Regras do Usuário")]
        public virtual AspNetRoles Role { get; set; }
        public virtual AspNetUsers User { get; set; }
    }
}
