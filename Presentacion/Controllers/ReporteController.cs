using LICORERIA.Infraestructura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LICORERIA.Presentacion.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReporteController : ControllerBase
    {
        private readonly LICORERIA_DBContext _context;

        public ReporteController(
            LICORERIA_DBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Genera el reporte del inventario.
        /// Muestra productos disponibles, agotados y con stock bajo.
        /// </summary>
        [HttpGet("Inventario")]
        public async Task<IActionResult> ReporteInventario()
        {
            var reporte = await _context.Productos
                .Where(p => p.Activo)
                .Select(p => new
                {
                    p.IdProducto,
                    p.Nombre,
                    p.Categoria,
                    p.CodigoBarras,
                    p.PrecioCompra,
                    p.PrecioVenta,
                    p.StockActual,
                    p.StockMinimo,

                    Estado =
                        p.StockActual == 0
                            ? "Agotado"
                            : p.StockActual <= p.StockMinimo
                                ? "Stock Bajo"
                                : "Disponible"
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return Ok(reporte);
        }
    }
}