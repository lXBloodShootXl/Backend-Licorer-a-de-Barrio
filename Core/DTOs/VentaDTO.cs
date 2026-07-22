namespace LICORERIA.Core.DTOs
{
    public class VentaDTO
    {
        public List<DetalleVentaDTO> Productos { get; set; }
    }
    public class DetalleVentaDTO
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }

    }
}