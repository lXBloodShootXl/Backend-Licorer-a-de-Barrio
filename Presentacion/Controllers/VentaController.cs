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
    }
}