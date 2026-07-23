using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LICORERIA.Presentacion.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BackupController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// US-35: Genera una copia de seguridad de la base de datos.
        /// </summary>
        [HttpPost]
        public IActionResult CrearBackup()
        {
            try
            {
                var connection =
                    _configuration.GetConnectionString("LICORERIAContext");

                var builder = new Npgsql.NpgsqlConnectionStringBuilder(connection);

                string carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Backups");

                Directory.CreateDirectory(carpeta);

                string archivo =
                    $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";

                string ruta =
                    Path.Combine(carpeta, archivo);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\PostgreSQL\15\bin\pg_dump.exe",
                    Arguments =
                        $"-h {builder.Host} " +
                        $"-p {builder.Port} " +
                        $"-U {builder.Username} " +
                        $"-F p " +
                        $"-f \"{ruta}\" " +
                        $"{builder.Database}",

                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                psi.Environment["PGPASSWORD"] = builder.Password;

                var process = Process.Start(psi);

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    return BadRequest(new
                    {
                        mensaje = process.StandardError.ReadToEnd()
                    });
                }

                return Ok(new
                {
                    mensaje = "Copia de seguridad creada correctamente.",
                    archivo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}