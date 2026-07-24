namespace LICORERIA.Core.DTOs
{
    public class PersonaDTO
    {
        public string ci { get; set; } = null!;
        public string nombre { get; set; } = null!;
        public string? apellido_p { get; set; } = null;
        public string? apellido_m { get; set; } = null;
        public bool sexo { get; set; } = true;
        public DateOnly? fecha_nacimiento { get; set; } = null;
        //Solo desarrollo
        public string hashhuella { get; set; } = null!;
    }
}
