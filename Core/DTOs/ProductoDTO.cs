namespace LICORERIA.Core.DTOs
{
    public class ProductoDTO
    {
        public string Nombre { get; set; }

        public string Categoria { get; set; }

        public string? CodigoBarras { get; set; }

        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }
    }
}
