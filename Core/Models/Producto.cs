using System.ComponentModel.DataAnnotations;

namespace LICORERIA.Core.Models
{
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }


        [Required]
        public string Nombre { get; set; }


        [Required]
        public string Categoria { get; set; }


        public string? CodigoBarras { get; set; }


        [Required]
        public decimal PrecioCompra { get; set; }


        [Required]
        public decimal PrecioVenta { get; set; }


        [Required]
        public int StockActual { get; set; }


        [Required]
        public int StockMinimo { get; set; }


        public bool Activo { get; set; } = true;


        // Relación con detalles de venta
        public ICollection<DetalleVenta> DetallesVenta { get; set; }
            = new List<DetalleVenta>();
    }
}