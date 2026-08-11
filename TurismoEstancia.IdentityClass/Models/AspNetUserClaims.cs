using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurismoEstancia.IdentityClass.Models
{
    public partial class AspNetUserClaims
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ClaimType { get; set; }
        [Display(Name = "Sistema do usuário")]
        public string ClaimValue { get; set; }
        public virtual AspNetUsers User { get; set; }
        [NotMapped]
        public virtual string Icone { get; set; }
        [NotMapped]
        public virtual bool Enabled { get; set; }

    }
}
