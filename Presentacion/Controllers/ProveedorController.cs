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
    public class ProveedorController : ControllerBase
    {
        private readonly LICORERIA_DBContext _context;

        public ProveedorController(LICORERIA_DBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los proveedores activos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProveedores()
        {
            var proveedores = await _context.Proveedores
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return Ok(proveedores);
        }

        /// <summary>
        /// Obtiene un proveedor por su ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.IdProveedor == id && p.Activo);

            if (proveedor == null)
            {
                return NotFound("Proveedor no encontrado o inactivo.");
            }

            return Ok(proveedor);
        }

        /// <summary>
        /// Crea un nuevo proveedor.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearProveedor([FromBody] ProveedorDTO request)
        {
            var existe = await _context.Proveedores
                .AnyAsync(p => p.Nombre.ToLower() == request.Nombre.ToLower());

            if (existe)
            {
                return BadRequest("Ya existe un proveedor con este nombre.");
            }

            var proveedor = new Proveedor
            {
                Nombre = request.Nombre,
                Telefono = request.Telefono,
                Direccion = request.Direccion,
                Observaciones = request.Observaciones,
                Activo = true
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.IdProveedor }, proveedor);
        }

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProveedor(int id, [FromBody] ProveedorDTO request)
        {
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.IdProveedor == id && p.Activo);

            if (proveedor == null)
            {
                return NotFound("Proveedor no encontrado o inactivo.");
            }

            // Validar que el nuevo nombre no pertenezca a otro proveedor
            var existeNombre = await _context.Proveedores
                .AnyAsync(p => p.IdProveedor != id && p.Nombre.ToLower() == request.Nombre.ToLower());

            if (existeNombre)
            {
                return BadRequest("Ya existe otro proveedor con este nombre.");
            }

            proveedor.Nombre = request.Nombre;
            proveedor.Telefono = request.Telefono;
            proveedor.Direccion = request.Direccion;
            proveedor.Observaciones = request.Observaciones;

            await _context.SaveChangesAsync();

            return Ok(proveedor);
        }

        /// <summary>
        /// Elimina lógicamente un proveedor.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.IdProveedor == id && p.Activo);

            if (proveedor == null)
            {
                return NotFound("Proveedor no encontrado o ya está inactivo.");
            }

            proveedor.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Proveedor eliminado exitosamente." });
        }
    }
}
