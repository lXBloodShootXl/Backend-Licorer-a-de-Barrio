using System.ComponentModel.DataAnnotations;

namespace LICORERIA.Core.Models
{
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(20)]
        public string Telefono { get; set; }

        [MaxLength(200)]
        public string Direccion { get; set; }

        [MaxLength(500)]
        public string Observaciones { get; set; }

        public bool Activo { get; set; } = true;
    }
}
