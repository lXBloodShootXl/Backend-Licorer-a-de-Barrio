namespace LICORERIA.Core.DTOs
{
    public class MovimientoInventarioDTO
    {
        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public string TipoMovimiento { get; set; }

        public string Observacion { get; set; }
    }
}