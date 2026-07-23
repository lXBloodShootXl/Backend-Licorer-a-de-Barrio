using System.Collections.Generic;

namespace LICORERIA.Core.DTOs
{
    public class CompraDTO
    {
        public string NombreProveedor { get; set; }
        public List<DetalleCompraDTO> Productos { get; set; } = new List<DetalleCompraDTO>();
    }

    public class DetalleCompraDTO
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}
