using System.ComponentModel.DataAnnotations;

namespace LICORERIA.Core.Models
{
    public class Usuario
    {

        [Key]
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string UsuarioLogin { get; set; }
        public string Password { get; set; }
        public DateTime UltimoAcceso { get; set; }

    }
}