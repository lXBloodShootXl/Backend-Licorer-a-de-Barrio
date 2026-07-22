using LICORERIA.Core.DTOs;
using LICORERIA.Core.Models;
namespace LICORERIA.Core.Mapedores
{
    public static class PersonaMapeador
    {
        public static PersonaDTO toPersonaDTO(this Persona persona)
        {
            return new PersonaDTO()
            {
                ci = persona.ci,
                nombre = persona.nombre,
                apellido_p = persona.apellido_p,
                apellido_m = persona.apellido_m,
                sexo = persona.sexo,
                fecha_nacimiento = persona.fecha_nacimiento,
                //Solo desarrollo
                hashhuella = persona.hashhuella
            };
        }
    }
}
