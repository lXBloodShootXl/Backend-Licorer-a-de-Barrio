using Microsoft.AspNetCore.Mvc;

namespace LICORERIA.Presentacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonasController : ControllerBase
    {
        private static readonly List<Persona> Personas = new();

        /// <summary>
        /// Obtiene la lista de Personas activos.
        /// </summary>
        [HttpGet]
        public IActionResult GetPersonas()
        {
            var personas = Personas.Where(p => !p.Borrado).ToList();
            return Ok(personas);
        }

        /// <summary>
        /// Obtiene una Persona por su CI.
        /// </summary>
        [HttpGet("{ci}")]
        public IActionResult GetPersona(string ci)
        {
            var persona = Personas.FirstOrDefault(p => p.Ci == ci && !p.Borrado);

            if (persona is null)
                return NotFound($"No se encontró una Persona con CI {ci}.");

            return Ok(persona);
        }

        /// <summary>
        /// Obtiene Personas borradas.
        /// </summary>
        [HttpGet("Borrados")]
        public IActionResult GetPersonasBorrados()
        {
            var personas = Personas.Where(p => p.Borrado).ToList();
            return Ok(personas);
        }

        /// <summary>
        /// Crea una nueva Persona.
        /// </summary>
        [HttpPost]
        public IActionResult PostPersona(Persona persona)
        {
            if (string.IsNullOrWhiteSpace(persona.Ci) ||
                string.IsNullOrWhiteSpace(persona.Nombre))
            {
                return BadRequest("Faltan campos.");
            }

            if (Personas.Any(p => p.Ci == persona.Ci))
                return BadRequest("Ya existe una Persona con ese CI.");

            Personas.Add(persona);

            return CreatedAtAction(nameof(GetPersona),
                new { ci = persona.Ci },
                persona);
        }

        /// <summary>
        /// Actualiza una Persona.
        /// </summary>
        [HttpPatch("{ci}")]
        public IActionResult PutPersona(string ci, Persona datos)
        {
            var persona = Personas.FirstOrDefault(p => p.Ci == ci);

            if (persona is null)
                return NotFound($"No se encontró una Persona con CI {ci}.");

            if (!string.IsNullOrWhiteSpace(datos.Nombre))
                persona.Nombre = datos.Nombre;

            if (!string.IsNullOrWhiteSpace(datos.ApellidoP))
                persona.ApellidoP = datos.ApellidoP;

            if (!string.IsNullOrWhiteSpace(datos.ApellidoM))
                persona.ApellidoM = datos.ApellidoM;

            persona.Sexo = datos.Sexo ?? persona.Sexo;

            if (!string.IsNullOrWhiteSpace(datos.FechaNacimiento))
                persona.FechaNacimiento = datos.FechaNacimiento;

            if (!string.IsNullOrWhiteSpace(datos.HashHuella))
                persona.HashHuella = datos.HashHuella;

            return Ok(persona);
        }

        /// <summary>
        /// Eliminación lógica.
        /// </summary>
        [HttpDelete("{ci}")]
        public IActionResult DeletePersona(string ci)
        {
            var persona = Personas.FirstOrDefault(p => p.Ci == ci);

            if (persona is null)
                return NotFound($"No se encontró una Persona con CI {ci}.");

            persona.Borrado = true;

            return Ok(persona);
        }

        /// <summary>
        /// Reactiva una Persona eliminada.
        /// </summary>
        [HttpPatch("Habilitar/{ci}")]
        public IActionResult HabilitarPersona(string ci)
        {
            var persona = Personas.FirstOrDefault(p => p.Ci == ci);

            if (persona is null)
                return NotFound($"No se encontró una Persona con CI {ci}.");

            persona.Borrado = false;

            return Ok(persona);
        }
    }

    public class Persona
    {
        public string Ci { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? ApellidoP { get; set; }
        public string? ApellidoM { get; set; }
        public bool? Sexo { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? HashHuella { get; set; }
        public bool Borrado { get; set; } = false;
    }
}