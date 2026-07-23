using System.ComponentModel.DataAnnotations;

namespace LICORERIA.Core.DTOs
{
    public class ProveedorDTO
    {
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(20)]
        public string Telefono { get; set; }

        [MaxLength(200)]
        public string Direccion { get; set; }

        [MaxLength(500)]
        public string Observaciones { get; set; }
    }
}
