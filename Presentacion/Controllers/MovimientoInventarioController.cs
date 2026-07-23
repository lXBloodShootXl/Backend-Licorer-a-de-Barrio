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
        /// US-23: Consulta el historial de movimientos de inventario
        /// (entradas y salidas) con filtros opcionales.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMovimientos(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idProducto,
            string? tipoMovimiento)
        {
            var consulta = _context.MovimientosInventario
                .Include(x => x.Producto)
                .Include(x => x.Usuario)
                .AsQueryable();


            if (fechaInicio.HasValue)
            {
                consulta = consulta.Where(x =>
                    x.Fecha.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                consulta = consulta.Where(x =>
                    x.Fecha.Date <= fechaFin.Value.Date);
            }

            if (idProducto.HasValue)
            {
                consulta = consulta.Where(x =>
                    x.IdProducto == idProducto.Value);
            }

            if (!string.IsNullOrWhiteSpace(tipoMovimiento))
            {
                consulta = consulta.Where(x =>
                    x.TipoMovimiento.ToUpper() == tipoMovimiento.ToUpper());
            }


            var movimientos = await consulta
                .OrderByDescending(x => x.Fecha)
                .Select(m => new
                {
                    m.IdMovimiento,
                    m.Fecha,
                    m.TipoMovimiento,
                    m.Cantidad,
                    m.Observacion,
                    producto = m.Producto.Nombre,
                    usuario  = m.Usuario.Nombre
                })
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