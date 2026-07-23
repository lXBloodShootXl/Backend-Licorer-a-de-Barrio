namespace LICORERIA.Core.DTOs
{
    /// <summary>
    /// US-17: DTO para actualizar únicamente los precios
    /// de compra y venta de un producto.
    /// </summary>
    public class ActualizarPreciosDTO
    {
        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }
    }
}
