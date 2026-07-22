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
    }
}