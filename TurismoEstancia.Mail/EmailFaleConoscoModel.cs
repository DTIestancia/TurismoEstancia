using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurismoEstancia.Mail
{
    [NotMapped]
    public class EmailFaleConoscoModel
    {
        [Display(Name = "Nome")]
        [Required(ErrorMessage = "Campo {0} é obrigatório.")]
        public string Nome { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Campo {0} é obrigatório.")]
        public string Email { get; set; }

        [Display(Name = "Destino")]
        //[Required(ErrorMessage = "Campo {0} é obrigatório.")]
        public string Destino { get; set; }

        [Display(Name = "Motivo")]
        [Required(ErrorMessage = "Campo {0} é obrigatório.")]
        public string Motivo { get; set; }

        [Display(Name = "Mensagem")]
        [MaxLength(1000)]
        [Required(ErrorMessage = "Campo {0} é obrigatório.")]
        public string Mensagem { get; set; }

        [Display(Name = "Anexos")]
        public IList<IFormFile> Attachments { get; set; }


    }
}
