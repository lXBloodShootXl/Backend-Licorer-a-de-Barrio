using LICORERIA.Core.Models;
using LICORERIA.Infraestructura.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace LICORERIA.Presentacion.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StartUsuarioController : ControllerBase
    {
        private readonly LICORERIA_DBContext _context;


        public StartUsuarioController(
            LICORERIA_DBContext context)
        {
            _context = context;
        }

        private async Task<Usuario?> GetUsuario(int id)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);
        }



        [HttpPost("StarterUser")]
        public async Task<IActionResult> PostUsuario(
            Usuario usuario)
        {
            bool existe = await _context.Usuarios.AnyAsync();
            if (existe == true)
                return BadRequest("Ya existe un usuario.");
            if (
                string.IsNullOrWhiteSpace(usuario.Nombre) ||
                string.IsNullOrWhiteSpace(usuario.UsuarioLogin) ||
                string.IsNullOrWhiteSpace(usuario.Password)
                )
            {
                return BadRequest(
                    "Faltan campos obligatorios.");
            }


            usuario.Password =
                BCrypt.Net.BCrypt.HashPassword(
                    usuario.Password);


            usuario.UltimoAcceso =
                DateTime.Now;


            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                usuario.IdUsuario,
                usuario.Nombre,
                usuario.UsuarioLogin
            });
        }
    }
}