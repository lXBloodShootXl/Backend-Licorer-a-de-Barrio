using LICORERIA.Core.Models;
using LICORERIA.Infraestructura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace LICORERIA.Presentacion.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientoInventarioController : ControllerBase
    {

        private readonly LICORERIA_DBContext _context;


        public MovimientoInventarioController(
            LICORERIA_DBContext context)
        {
            _context = context;
        }



        /// <summary>
        /// Consulta movimientos de inventario.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMovimientos()
        {
            var movimientos =
                await _context.MovimientosInventario
                .Include(x => x.Producto)
                .Include(x => x.Usuario)
                .ToListAsync();


            return Ok(movimientos);
        }



        /// <summary>
        /// Consulta movimiento por ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovimiento(
            int id)
        {
            var movimiento =
                await _context.MovimientosInventario
                .Include(x => x.Producto)
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(
                    x => x.IdMovimiento == id);


            if (movimiento == null)
            {
                return NotFound(
                    "Movimiento no encontrado.");
            }


            return Ok(movimiento);
        }
    }
}