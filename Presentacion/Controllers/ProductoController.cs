using LICORERIA.Core.DTOs;
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
    public class ProductoController : ControllerBase
    {

        private readonly LICORERIA_DBContext _context;


        public ProductoController(
            LICORERIA_DBContext context)
        {
            _context = context;
        }



        /// <summary>
        /// Registra un nuevo producto.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearProducto(
            ProductoDTO dto)
        {

            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
               string.IsNullOrWhiteSpace(dto.Categoria))
            {
                return BadRequest(
                    "Nombre y categoría son obligatorios.");
            }



            if (dto.PrecioCompra <= 0 ||
               dto.PrecioVenta <= 0)
            {
                return BadRequest(
                    "Los precios deben ser mayores a cero.");
            }



            if (dto.StockActual < 0 ||
               dto.StockMinimo < 0)
            {
                return BadRequest(
                    "El stock no puede ser negativo.");
            }



            if (dto.PrecioVenta < dto.PrecioCompra)
            {
                return BadRequest(
                    "El precio de venta no puede ser menor al precio de compra.");
            }



            if (!string.IsNullOrEmpty(dto.CodigoBarras))
            {
                bool existe =
                    await _context.Productos
                    .AnyAsync(x =>
                        x.CodigoBarras == dto.CodigoBarras);


                if (existe)
                {
                    return BadRequest(
                        "Ya existe un producto con ese código de barras.");
                }
            }



            Producto producto = new Producto
            {
                Nombre = dto.Nombre,

                Categoria = dto.Categoria,

                CodigoBarras = dto.CodigoBarras,

                PrecioCompra = dto.PrecioCompra,

                PrecioVenta = dto.PrecioVenta,

                StockActual = dto.StockActual,

                StockMinimo = dto.StockMinimo
            };



            _context.Productos.Add(producto);


            await _context.SaveChangesAsync();



            return CreatedAtAction(
                nameof(GetProducto),
                new
                {
                    id = producto.IdProducto
                },
                producto);
        }





        /// <summary>
        /// Consulta productos registrados.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var productos =
                await _context.Productos
                .Where(x => x.Activo)
                .ToListAsync();


            return Ok(productos);
        }
        /// <summary>
        /// Consulta productos desactivados.
        /// </summary>
        [HttpGet("Desactivados")]
        public async Task<IActionResult> GetProductosDesactivados()
        {
            var productos =
                await _context.Productos
                .Where(x => !x.Activo)
                .ToListAsync();


            return Ok(productos);
        }

        /// <summary>
        /// Consulta producto por id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProducto(
            int id)
        {

            var producto =
                await _context.Productos
                .FirstOrDefaultAsync(
                    x => x.IdProducto == id);



            if (producto == null)
            {
                return NotFound(
                    "Producto no encontrado.");
            }

            return Ok(producto);
        }

        /// <summary>
        /// Busca productos por nombre, categoría o código de barras.
        /// </summary>
        [HttpGet("Buscar")]
        public async Task<IActionResult> BuscarProductos(
            string? nombre,
            string? categoria,
            string? codigoBarras)
        {
            var consulta = _context.Productos
                .Where(x => x.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                consulta = consulta.Where(x =>
                    x.Nombre.Contains(nombre));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                consulta = consulta.Where(x =>
                    x.Categoria.Contains(categoria));
            }

            if (!string.IsNullOrWhiteSpace(codigoBarras))
            {
                consulta = consulta.Where(x =>
                    x.CodigoBarras == codigoBarras);
            }

            var productos = await consulta.ToListAsync();

            return Ok(productos);
        }

        /// <summary>
        /// Actualiza un producto.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProducto(
        int id,
        ProductoDTO dto)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(x => x.IdProducto == id);

            if (producto == null)
            {
                return NotFound("Producto no encontrado.");
            }

            producto.Nombre = dto.Nombre;
            producto.Categoria = dto.Categoria;
            producto.CodigoBarras = dto.CodigoBarras;
            producto.PrecioCompra = dto.PrecioCompra;
            producto.PrecioVenta = dto.PrecioVenta;
            producto.StockActual = dto.StockActual;
            producto.StockMinimo = dto.StockMinimo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Producto actualizado correctamente.",
                producto
            });
        }

        /// <summary>
        /// US-17: Actualiza únicamente los precios de compra
        /// y venta de un producto.
        /// </summary>
        [HttpPatch("{id}/Precios")]
        public async Task<IActionResult> ActualizarPrecios(
            int id,
            ActualizarPreciosDTO dto)
        {

            if (dto.PrecioCompra <= 0 ||
                dto.PrecioVenta <= 0)
            {
                return BadRequest(
                    "Los precios deben ser mayores a cero.");
            }


            if (dto.PrecioVenta < dto.PrecioCompra)
            {
                return BadRequest(
                    "El precio de venta no puede ser menor" +
                    " al precio de compra.");
            }


            var producto =
                await _context.Productos
                .FirstOrDefaultAsync(
                    x => x.IdProducto == id && x.Activo);


            if (producto == null)
            {
                return NotFound(
                    "Producto no encontrado o está inactivo.");
            }


            producto.PrecioCompra = dto.PrecioCompra;
            producto.PrecioVenta  = dto.PrecioVenta;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    "Precios actualizados correctamente.",
                idProducto   = producto.IdProducto,
                nombre       = producto.Nombre,
                precioCompra = producto.PrecioCompra,
                precioVenta  = producto.PrecioVenta
            });
        }


        /// <summary>
        /// US-19: Registra o actualiza el código de barras
        /// de un producto existente.
        /// </summary>
        [HttpPatch("{id}/CodigoBarras")]
        public async Task<IActionResult> ActualizarCodigoBarras(
            int id,
            ActualizarCodigoBarrasDTO dto)
        {

            var producto =
                await _context.Productos
                .FirstOrDefaultAsync(
                    x => x.IdProducto == id && x.Activo);


            if (producto == null)
            {
                return NotFound(
                    "Producto no encontrado o está inactivo.");
            }


            // Validar unicidad del código de barras
            if (!string.IsNullOrWhiteSpace(dto.CodigoBarras))
            {
                bool yaExiste =
                    await _context.Productos
                    .AnyAsync(x =>
                        x.CodigoBarras == dto.CodigoBarras &&
                        x.IdProducto   != id);


                if (yaExiste)
                {
                    return BadRequest(
                        "Ya existe otro producto con ese" +
                        " código de barras.");
                }
            }


            producto.CodigoBarras = dto.CodigoBarras;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje       =
                    "Código de barras actualizado correctamente.",
                idProducto    = producto.IdProducto,
                nombre        = producto.Nombre,
                codigoBarras  = producto.CodigoBarras
            });
        }


        /// <summary>
        /// Desactiva un producto.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DesactivarProducto(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(x => x.IdProducto == id);

            if (producto == null)
            {
                return NotFound("Producto no encontrado.");
            }

            if (!producto.Activo)
            {
                return BadRequest("El producto ya está desactivado.");
            }

            producto.Activo = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Producto desactivado correctamente."
            });
        }


        /// <summary>
        /// US-16: Alerta de stock mínimo.
        /// Devuelve los productos activos cuyo stock actual
        /// sea menor o igual a su stock mínimo establecido.
        /// </summary>
        [HttpGet("Alertas")]
        public async Task<IActionResult> GetAlertasStockMinimo()
        {
            var productosEnAlerta =
                await _context.Productos
                .Where(p =>
                    p.Activo &&
                    p.StockActual <= p.StockMinimo)
                .Select(p => new
                {
                    p.IdProducto,
                    p.Nombre,
                    p.Categoria,
                    p.CodigoBarras,
                    p.StockActual,
                    p.StockMinimo,
                    p.PrecioCompra,
                    p.PrecioVenta,

                    Estado =
                        p.StockActual == 0
                            ? "Agotado"
                            : "Stock Bajo"
                })
                .OrderBy(p => p.StockActual)
                .ToListAsync();


            if (!productosEnAlerta.Any())
            {
                return Ok(new
                {
                    mensaje =
                        "Todos los productos tienen stock suficiente.",
                    totalEnAlerta = 0,
                    productos = productosEnAlerta
                });
            }


            return Ok(new
            {
                mensaje =
                    $"{productosEnAlerta.Count} producto(s) requieren reabastecimiento.",
                totalEnAlerta = productosEnAlerta.Count,
                productos = productosEnAlerta
            });
        }
    }
}