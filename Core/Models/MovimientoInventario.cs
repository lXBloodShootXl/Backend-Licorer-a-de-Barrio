using System.ComponentModel.DataAnnotations;

namespace LICORERIA.Core.Models
{
    public class MovimientoInventario
    {
        [Key]
        public int IdMovimiento { get; set; }

        public int IdProducto { get; set; }

        public Producto Producto { get; set; }


        public int IdUsuario { get; set; }

        public Usuario Usuario { get; set; }


        public int Cantidad { get; set; }

        public int StockAnterior { get; set; }

        public int StockNuevo { get; set; }


        public string TipoMovimiento { get; set; }

        public string Observacion { get; set; }


        public DateTime Fecha { get; set; }
    }
}