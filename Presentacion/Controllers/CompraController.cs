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

            // Validar que los productos existan y tengan precios razonables
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

            // Iniciar transacción (aunque no estamos actualizando stock ahora, es buena práctica)
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
