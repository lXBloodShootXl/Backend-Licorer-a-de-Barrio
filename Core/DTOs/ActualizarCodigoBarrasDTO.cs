namespace LICORERIA.Core.DTOs
{
    /// <summary>
    /// US-19: DTO para registrar o actualizar únicamente
    /// el código de barras de un producto.
    /// </summary>
    public class ActualizarCodigoBarrasDTO
    {
        public string? CodigoBarras { get; set; }
    }
}
