using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace LICORERIA.Core.Models
{
    [Index(nameof(ci), IsUnique = true)]
    public class Persona
    {
        [Key]
        public int id_persona { get; set; }
        public string ci { get; set; } = null!;
        public string nombre { get; set; }  = null!;
        public string? apellido_p { get; set; } = null;
        public string? apellido_m { get; set; } = null;
        public bool sexo { get; set; } = true;
        public DateOnly fecha_nacimiento { get; set; }
        public string hashhuella { get; set; } = null!;
        public bool estado { get; set; } = true;
    }
}
