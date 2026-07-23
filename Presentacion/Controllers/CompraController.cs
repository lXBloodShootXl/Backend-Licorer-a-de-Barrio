using LICORERIA.Core.DTOs;
using LICORERIA.Core.Models;
using LICORERIA.Infraestructura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LICORERIA.Presentacion.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CompraController : ControllerBase
    {
        private readonly LICORERIA_DBContext _context;

        public CompraController(LICORERIA_DBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// US-25: Consulta las compras registradas.
        /// Permite filtrar por rango de fechas y proveedor.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCompras(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string? proveedor)
        {
            var consulta = _context.Compras
                .AsNoTracking()
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.Fecha.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.Fecha.Date <= fechaFin.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(proveedor))
            {
                consulta = consulta.Where(c =>
                    c.NombreProveedor.ToLower()
                        .Contains(proveedor.ToLower()));
            }

            var compras = await consulta
                .OrderByDescending(c => c.Fecha)
                .Select(c => new
                {
                    c.IdCompra,
                    fecha = c.Fecha,
                    proveedor = c.NombreProveedor,
                    total = c.Total,
                    totalProductos = c.Detalles.Sum(d => d.Cantidad),
                    detalles = c.Detalles.Select(d => new
                    {
                        d.IdProducto,
                        producto = d.Producto.Nombre,
                        d.Cantidad,
                        d.CostoUnitario,
                        d.Subtotal
                    })
                })
                .ToListAsync();

            return Ok(new
            {
                totalCompras = compras.Count,
                totalInvertido = compras.Sum(c => c.total),
                compras
            });
        }


        /// <summary>
        /// US-26: Consulta el detalle de una compra específica por su ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompra(int id)
        {
            var compra = await _context.Compras
                .AsNoTracking()
                .Where(c => c.IdCompra == id)
                .Select(c => new
                {
                    c.IdCompra,
                    fecha = c.Fecha,
                    proveedor = c.NombreProveedor,
                    total = c.Total,
                    detalles = c.Detalles.Select(d => new
                    {
                        d.IdProducto,
                        producto = d.Producto.Nombre,
                        d.Cantidad,
                        d.CostoUnitario,
                        d.Subtotal
                    })
                })
                .FirstOrDefaultAsync();

            if (compra == null)
            {
                return NotFound("Compra no encontrada.");
            }

            return Ok(compra);
        }


        /// <summary>
        /// US-24: Registra una compra a proveedor.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegistrarCompra([FromBody] CompraDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.NombreProveedor))
            {
                return BadRequest("El nombre del proveedor es obligatorio.");
            }

            if (request.Productos == null || !request.Productos.Any())
            {
                return BadRequest("La compra debe incluir al menos un producto.");
            }


            var idsProductos = request.Productos.Select(p => p.IdProducto).ToList();
            var productosDb = await _context.Productos
                .Where(p => idsProductos.Contains(p.IdProducto))
                .ToListAsync();

            if (productosDb.Count != idsProductos.Distinct().Count())
            {
                return BadRequest("Uno o más productos no existen en la base de datos.");
            }

            foreach (var item in request.Productos)
            {
                if (item.Cantidad <= 0)
                    return BadRequest($"La cantidad para el producto {item.IdProducto} debe ser mayor a 0.");
                if (item.CostoUnitario <= 0)
                    return BadRequest($"El costo para el producto {item.IdProducto} debe ser mayor a 0.");
            }

            var usuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioClaim == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }
            int idUsuario = int.Parse(usuarioClaim.Value);


            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var compra = new Compra
                {
                    Fecha = DateTime.Now,
                    NombreProveedor = request.NombreProveedor,
                    Total = 0
                };

                decimal totalCompra = 0;

                foreach (var item in request.Productos)
                {
                    var subtotal = item.Cantidad * item.CostoUnitario;
                    totalCompra += subtotal;

                    compra.Detalles.Add(new DetalleCompra
                    {
                        IdProducto = item.IdProducto,
                        Cantidad = item.Cantidad,
                        CostoUnitario = item.CostoUnitario,
                        Subtotal = subtotal
                    });

                    var productoDb = productosDb.First(p => p.IdProducto == item.IdProducto);
                    productoDb.StockActual += item.Cantidad;

                    _context.MovimientosInventario.Add(new MovimientoInventario
                    {
                        IdProducto = item.IdProducto,
                        IdUsuario = idUsuario,
                        Cantidad = item.Cantidad,
                        TipoMovimiento = "ENTRADA",
                        Fecha = DateTime.Now,
                        Observacion = $"Compra a proveedor: {request.NombreProveedor}"
                    });
                }

                compra.Total = totalCompra;
                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(RegistrarCompra), new { id = compra.IdCompra }, compra);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error interno al registrar la compra: {ex.Message}");
            }
        }
    }
}
