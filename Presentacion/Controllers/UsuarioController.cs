using BCrypt.Net;
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
    public class UsuarioController : ControllerBase
    {
        private readonly LICORERIA_DBContext _context;


        public UsuarioController(
            LICORERIA_DBContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .ToListAsync();

            return Ok(usuarios);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == id);


            if (usuario == null)
                return NotFound(
                    "Usuario no encontrado.");


            return Ok(usuario);
        }


        [HttpPost]
        public async Task<IActionResult> PostUsuario(
            Usuario usuario)
        {
            if (
                string.IsNullOrWhiteSpace(usuario.Nombre) ||
                string.IsNullOrWhiteSpace(usuario.UsuarioLogin) ||
                string.IsNullOrWhiteSpace(usuario.Password)
               )
            {
                return BadRequest(
                    "Faltan campos obligatorios.");
            }


            var existe = await _context.Usuarios
                .AnyAsync(u =>
                    u.UsuarioLogin == usuario.UsuarioLogin);


            if (existe)
            {
                return BadRequest(
                    "Ya existe ese usuario.");
            }


            usuario.Password =
                BCrypt.Net.BCrypt.HashPassword(
                    usuario.Password);


            usuario.UltimoAcceso =
                DateTime.Now;


            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();


            return CreatedAtAction(
                nameof(GetUsuario),
                new
                {
                    id = usuario.IdUsuario
                },
                usuario);
        }


        [HttpPatch("{id}/CambiarPassword")]
        public async Task<IActionResult> CambiarPassword(
            int id,
            CambiarPasswordDTO datos)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == id);


            if (usuario == null)
                return NotFound(
                    "Usuario no encontrado.");


            if (!BCrypt.Net.BCrypt.Verify(
                datos.PasswordActual,
                usuario.Password))
            {
                return BadRequest(
                    "Contraseña actual incorrecta.");
            }


            usuario.Password =
                BCrypt.Net.BCrypt.HashPassword(
                    datos.PasswordNueva);


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                "Contraseña actualizada correctamente."
            });
        }
    }
}