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
        public async Task<IActionResult> RegistrarVenta(
            VentaDTO dto)
        {

            if (dto.Productos == null ||
                !dto.Productos.Any())
            {
                return BadRequest(
                    "Debe agregar productos a la venta.");
            }


            var usuarioClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier);


            if (usuarioClaim == null)
            {
                return Unauthorized(
                    "Usuario no autenticado.");
            }


            int idUsuario =
                int.Parse(usuarioClaim.Value);



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

                if (item.Cantidad <= 0)
                {
                    return BadRequest(
                        "La cantidad debe ser mayor a cero.");
                }



                var producto =
                    await _context.Productos
                    .FirstOrDefaultAsync(
                        x => x.IdProducto == item.IdProducto);



                if (producto == null)
                {
                    return BadRequest(
                        $"No existe el producto con ID {item.IdProducto}.");
                }



                // US-06: Validar stock disponible
                if (producto.StockActual < item.Cantidad)
                {
                    return BadRequest(
                        $"Stock insuficiente para {producto.Nombre}. " +
                        $"Disponible: {producto.StockActual}");
                }



                decimal subtotal =
                    producto.PrecioVenta *
                    item.Cantidad;



                decimal gananciaProducto =
                    (producto.PrecioVenta -
                     producto.PrecioCompra)
                     *
                     item.Cantidad;



                totalVenta += subtotal;

                gananciaVenta += gananciaProducto;



                // Actualización de stock
                producto.StockActual -= item.Cantidad;

                _context.MovimientosInventario.Add(
                new MovimientoInventario
                {
                    IdProducto = producto.IdProducto,

                    IdUsuario = idUsuario,

                    TipoMovimiento = "SALIDA",

                    Cantidad = item.Cantidad,

                    Fecha = DateTime.Now,

                    Observacion = "Venta registrada"
                });

                venta.Detalles.Add(
                    new DetalleVenta
                    {
                        IdProducto =
                            producto.IdProducto,

                        Cantidad =
                            item.Cantidad,

                        PrecioUnitario =
                            producto.PrecioVenta,

                        Subtotal =
                            subtotal
                    });
            }



            // US-09: Guardar ganancia automática
            venta.Total = totalVenta;

            venta.Ganancia = gananciaVenta;



            _context.Ventas.Add(venta);


            await _context.SaveChangesAsync();



            return Ok(new
            {
                mensaje = "Venta registrada correctamente.",

                idVenta = venta.IdVenta,

                fecha = venta.Fecha,

                hora = venta.Hora,

                total = venta.Total,

                productos = venta.Detalles.Select(x => new
                {
                    producto = x.IdProducto,
                    cantidad = x.Cantidad,
                    precioUnitario = x.PrecioUnitario,
                    subtotal = x.Subtotal
                })
            });
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
                .Include(x => x.Detalles)
                .ThenInclude(x => x.Producto)
                .Include(x => x.Usuario)
                .AsQueryable();


            if (fechaInicio.HasValue)
            {
                consulta = consulta.Where(x =>
                    x.Fecha >= fechaInicio.Value.Date);
            }


            if (fechaFin.HasValue)
            {
                consulta = consulta.Where(x =>
                    x.Fecha <= fechaFin.Value.Date);
            }


            var ventas = await consulta
                .OrderByDescending(x => x.Fecha)
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


        /// <summary>
        /// US-18: Consulta el detalle de una venta con el
        /// subtotal de cada producto y el total de la venta.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenta(int id)
        {

            var venta =
                await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(
                    v => v.IdVenta == id);


            if (venta == null)
            {
                return NotFound(
                    "Venta no encontrada.");
            }


            return Ok(new
            {
                idVenta  = venta.IdVenta,
                fecha    = venta.Fecha,
                hora     = venta.Hora,
                usuario  = venta.Usuario.Nombre,

                // US-18: Desglose de subtotales por producto
                productos = venta.Detalles.Select(d => new
                {
                    idProducto     = d.IdProducto,
                    nombreProducto = d.Producto.Nombre,
                    cantidad       = d.Cantidad,
                    precioUnitario = d.PrecioUnitario,

                    // Subtotal calculado automáticamente
                    subtotal       = d.Subtotal
                }),

                // US-18: Total calculado automáticamente
                total    = venta.Total,
                ganancia = venta.Ganancia
            });
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
                    "Debe proporcionar un código de barras" +
                    " o un nombre para buscar.");
            }


            var consulta = _context.Productos
                .Where(p => p.Activo)
                .AsQueryable();


            // Búsqueda por código de barras (exacta)
            if (!string.IsNullOrWhiteSpace(codigoBarras))
            {
                consulta = consulta.Where(p =>
                    p.CodigoBarras == codigoBarras);
            }
            // Búsqueda por nombre (parcial)
            else if (!string.IsNullOrWhiteSpace(nombre))
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(nombre));
            }


            var productos = await consulta
                .Select(p => new
                {
                    p.IdProducto,
                    p.Nombre,
                    p.Categoria,
                    p.CodigoBarras,
                    p.PrecioVenta,
                    p.StockActual,

                    // Indica si hay stock para vender
                    disponible = p.StockActual > 0
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();


            if (!productos.Any())
            {
                return NotFound(
                    "No se encontró ningún producto activo" +
                    " con ese criterio de búsqueda.");
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
        }
    }
}