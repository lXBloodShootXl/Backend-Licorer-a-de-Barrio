using LICORERIA.Infraestructura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LICORERIA.Presentacion.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriaController : ControllerBase
    {
        private readonly LICORERIA_DBContext _context;

        public AuditoriaController(
            LICORERIA_DBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Consulta todas las auditorías.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAuditorias()
        {
            var auditorias = await _context.Auditorias
                .Include(x => x.Usuario)
                .OrderByDescending(x => x.Fecha)
                .Select(x => new
                {
                    x.IdAuditoria,
                    x.Tabla,
                    x.Registro,
                    x.Accion,
                    x.Fecha,
                    Usuario = x.Usuario.Nombre
                })
                .ToListAsync();

            return Ok(auditorias);
        }

        /// <summary>
        /// Consulta una auditoría por ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditoria(int id)
        {
            var auditoria = await _context.Auditorias
                .Include(x => x.Usuario)
                .Where(x => x.IdAuditoria == id)
                .Select(x => new
                {
                    x.IdAuditoria,
                    x.Tabla,
                    x.Registro,
                    x.Accion,
                    x.Fecha,
                    Usuario = x.Usuario.Nombre
                })
                .FirstOrDefaultAsync();

            if (auditoria == null)
                return NotFound("Auditoría no encontrada.");

            return Ok(auditoria);
        }
    }
}