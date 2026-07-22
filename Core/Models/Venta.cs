using System.ComponentModel.DataAnnotations;

namespace LICORERIA.Core.Models
{
    public class Venta
    {
        [Key]
        public int IdVenta { get; set; }


        public DateTime Fecha { get; set; }


        public TimeSpan Hora { get; set; }


        public decimal Total { get; set; }


        // Ganancia obtenida de la venta
        public decimal Ganancia { get; set; }



        // Usuario que realizó la venta
        public int IdUsuario { get; set; }

        public Usuario Usuario { get; set; }



        // Productos vendidos
        public ICollection<DetalleVenta> Detalles { get; set; }
            = new List<DetalleVenta>();
    }
}