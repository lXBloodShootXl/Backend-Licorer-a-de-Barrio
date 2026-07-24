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
    public class VentaController : ControllerBase
    {
        private readonly LICORERIA_DBContext _context;


        public VentaController(LICORERIA_DBContext context)
        {
            _context = context;
        }



        /// <summary>
        /// Registra una nueva venta.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegistrarVenta(VentaDTO dto)
        {
            if (dto.Productos == null || !dto.Productos.Any())
                return BadRequest(ApiResponse<object>.Error("Debe agregar productos a la venta."));

            var usuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioClaim == null)
                return Unauthorized(ApiResponse<object>.Error("Usuario no autenticado."));

            int idUsuario = int.Parse(usuarioClaim.Value);

            // Precargar productos y validar antes de la transacción
            var idsProductos = dto.Productos.Select(p => p.IdProducto).ToList();
            var productosDb = await _context.Productos
                .Where(p => idsProductos.Contains(p.IdProducto))
                .ToListAsync();

            foreach (var item in dto.Productos)
            {
                if (item.Cantidad <= 0)
                    return BadRequest(ApiResponse<object>.Error("La cantidad debe ser mayor a cero."));

                var producto = productosDb.FirstOrDefault(p => p.IdProducto == item.IdProducto);
                if (producto == null)
                    return BadRequest(ApiResponse<object>.Error($"No existe el producto con ID {item.IdProducto}."));

                if (producto.StockActual < item.Cantidad)
                    return BadRequest(ApiResponse<object>.Error(
                        $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.StockActual}"));
            }

            Venta? ventaResultado = null;

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    Venta venta = new Venta
                    {
                        Fecha = DateTime.Now.Date,
                        Hora = DateTime.Now.TimeOfDay,
                        IdUsuario = idUsuario,
                        Detalles = new List<DetalleVenta>()
                    };

                    decimal totalVenta = 0;
                    decimal gananciaVenta = 0;

                    foreach (var item in dto.Productos)
                    {
                        var producto = productosDb.First(p => p.IdProducto == item.IdProducto);

                        decimal subtotal = producto.PrecioVenta * item.Cantidad;
                        decimal gananciaProducto = (producto.PrecioVenta - producto.PrecioCompra) * item.Cantidad;

                        totalVenta += subtotal;
                        gananciaVenta += gananciaProducto;
                        producto.StockActual -= item.Cantidad;

                        _context.MovimientosInventario.Add(new MovimientoInventario
                        {
                            IdProducto = producto.IdProducto,
                            IdUsuario = idUsuario,
                            TipoMovimiento = "SALIDA",
                            Cantidad = item.Cantidad,
                            Fecha = DateTime.Now,
                            Observacion = "Venta registrada"
                        });

                        venta.Detalles.Add(new DetalleVenta
                        {
                            IdProducto = producto.IdProducto,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = producto.PrecioVenta,
                            Subtotal = subtotal
                        });
                    }

                    venta.Total = totalVenta;
                    venta.Ganancia = gananciaVenta;

                    _context.Ventas.Add(venta);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    ventaResultado = venta;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            if (ventaResultado == null)
                return StatusCode(500, ApiResponse<object>.Error("Error al registrar la venta."));

            var responseData = new
            {
                idVenta = ventaResultado.IdVenta,
                fecha = ventaResultado.Fecha,
                hora = ventaResultado.Hora,
                total = ventaResultado.Total,
                productos = ventaResultado.Detalles.Select(x => new
                {
                    producto = x.IdProducto,
                    cantidad = x.Cantidad,
                    precioUnitario = x.PrecioUnitario,
                    subtotal = x.Subtotal
                })
            };

            return Ok(ApiResponse<object>.Success(responseData, "Venta registrada correctamente."));
        }




        /// <summary>
        /// Consulta historial de ventas con filtros.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVentas(
    DateTime? fechaInicio,
    DateTime? fechaFin)
        {
            var consulta = _context.Ventas
                .AsNoTracking()
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                consulta = consulta.Where(v =>
                    v.Fecha >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                consulta = consulta.Where(v =>
                    v.Fecha <= fechaFin.Value.Date);
            }

            var ventas = await consulta
                .OrderByDescending(v => v.Fecha)
                .Select(v => new
                {
                    idVenta = v.IdVenta,
                    fecha = v.Fecha,
                    hora = v.Hora,
                    total = v.Total,
                    ganancia = v.Ganancia,
                    usuario = v.Usuario.Nombre,

                    detalles = v.Detalles.Select(d => new
                    {
                        producto = d.Producto.Nombre,
                        cantidad = d.Cantidad,
                        precioUnitario = d.PrecioUnitario,
                        subtotal = d.Subtotal
                    })
                })
                .ToListAsync();

            return Ok(ventas);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenta(int id)
        {
            var venta = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.IdVenta == id)
                .Select(v => new
                {
                    idVenta = v.IdVenta,
                    fecha = v.Fecha,
                    hora = v.Hora,
                    usuario = v.Usuario.Nombre,

                    productos = v.Detalles.Select(d => new
                    {
                        idProducto = d.IdProducto,
                        nombreProducto = d.Producto.Nombre,
                        cantidad = d.Cantidad,
                        precioUnitario = d.PrecioUnitario,
                        subtotal = d.Subtotal
                    }),

                    total = v.Total,
                    ganancia = v.Ganancia
                })
                .FirstOrDefaultAsync();

            if (venta == null)
            {
                return NotFound("Venta no encontrada.");
            }

            return Ok(venta);
        }


        /// <summary>
        /// US-20: Busca un producto por código de barras
        /// o por nombre para agilizar el registro de ventas.
        /// Se debe enviar al menos uno de los dos parámetros.
        /// </summary>
        [HttpGet("BuscarProducto")]
        public async Task<IActionResult> BuscarProductoParaVenta(
    string? codigoBarras,
    string? nombre)
        {
            if (string.IsNullOrWhiteSpace(codigoBarras) &&
                string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(
                    "Debe proporcionar un código de barras o un nombre para buscar.");
            }

            var consulta = _context.Productos
                .AsNoTracking()
                .Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(codigoBarras))
            {
                consulta = consulta.Where(p =>
                    p.CodigoBarras == codigoBarras);
            }
            else
            {
                consulta = consulta.Where(p =>
                    EF.Functions.ILike(p.Nombre, $"%{nombre}%"));
            }

            var productos = await consulta
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    p.IdProducto,
                    p.Nombre,
                    p.Categoria,
                    p.CodigoBarras,
                    p.PrecioVenta,
                    p.StockActual,
                    disponible = p.StockActual > 0
                })
                .ToListAsync();
            if (!productos.Any())
            {
                return NotFound(
                    "No se encontró ningún producto activo con ese criterio.");
            }
            return Ok(productos);
        }

        /// <summary>
        /// US-26: Registra una devolución parcial o total de una venta.
        /// Retorna el stock al inventario y ajusta el total de la venta original.
        /// </summary>
        [HttpPost("{id}/Devolucion")]
        public async Task<IActionResult> RegistrarDevolucion(
            int id,
            [FromBody] List<DevolucionDTO> devoluciones)
        {
            if (devoluciones == null || !devoluciones.Any())
            {
                return BadRequest("Debe especificar al menos un producto a devolver.");
            }

            var usuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioClaim == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }
            int idUsuario = int.Parse(usuarioClaim.Value);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var venta = await _context.Ventas
                        .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                        .FirstOrDefaultAsync(v => v.IdVenta == id);

                    if (venta == null)
                    {
                        return NotFound("La venta especificada no existe.");
                    }

                    foreach (var devolucion in devoluciones)
                    {
                        if (devolucion.CantidadDevuelta <= 0)
                            return BadRequest($"La cantidad a devolver del producto {devolucion.IdProducto} debe ser mayor a cero.");

                        var detalle = venta.Detalles.FirstOrDefault(d => d.IdProducto == devolucion.IdProducto);
                        if (detalle == null)
                            return BadRequest($"El producto {devolucion.IdProducto} no forma parte de esta venta.");

                        if (devolucion.CantidadDevuelta > detalle.Cantidad)
                            return BadRequest($"No puede devolver {devolucion.CantidadDevuelta} unidades del producto {devolucion.IdProducto} porque solo se vendieron {detalle.Cantidad}.");

                        // 1. Ajustar el DetalleVenta
                        detalle.Cantidad -= devolucion.CantidadDevuelta;

                        var dineroDevuelto = devolucion.CantidadDevuelta * detalle.PrecioUnitario;
                        var gananciaDevuelta = devolucion.CantidadDevuelta * (detalle.PrecioUnitario - detalle.Producto.PrecioCompra);

                        detalle.Subtotal -= dineroDevuelto;

                        // 2. Ajustar los totales de Venta
                        venta.Total -= dineroDevuelto;
                        venta.Ganancia -= gananciaDevuelta;

                        // 3. Devolver el producto al inventario
                        detalle.Producto.StockActual += devolucion.CantidadDevuelta;

                        // 4. Registrar Movimiento de Inventario
                        _context.MovimientosInventario.Add(new MovimientoInventario
                        {
                            IdProducto = detalle.IdProducto,
                            IdUsuario = idUsuario,
                            Cantidad = devolucion.CantidadDevuelta,
                            TipoMovimiento = "ENTRADA",
                            Fecha = DateTime.Now,
                            Observacion = $"Devolución de cliente - Venta #{venta.IdVenta}"
                        });
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        mensaje = "Devolución registrada exitosamente.",
                        ventaActualizada = new
                        {
                            venta.IdVenta,
                            venta.Total,
                            venta.Ganancia
                        }
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Error interno al procesar la devolución: {ex.Message}");
                }
            });
        }
    }
}