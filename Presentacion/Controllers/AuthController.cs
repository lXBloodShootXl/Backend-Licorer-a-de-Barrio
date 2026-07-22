using BCrypt.Net;
using LICORERIA.Core.DTOs;
using LICORERIA.Infraestructura.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LICORERIA.Presentacion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LICORERIA_DBContext _context;
        private readonly IConfiguration _configuration;


        public AuthController(
            LICORERIA_DBContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        /// <summary>
        /// Inicia sesión del propietario.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x =>
                    x.UsuarioLogin == login.Usuario);


            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Credenciales incorrectas."
                });
            }


            if (!BCrypt.Net.BCrypt.Verify(
                login.Password,
                usuario.Password))
            {
                return Unauthorized(new
                {
                    mensaje = "Credenciales incorrectas."
                });
            }


            usuario.UltimoAcceso = DateTime.Now;

            await _context.SaveChangesAsync();


            var claims = new[]
            {
                new Claim(
                    ClaimTypes.Name,
                    usuario.Nombre),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString())
            };


            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));


            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials);


            var tokenString =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);


            return Ok(new
            {
                mensaje = "Bienvenido.",
                nombre = usuario.Nombre,
                token = tokenString
            });
        }
    }
}