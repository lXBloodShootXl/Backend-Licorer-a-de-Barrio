using System.ComponentModel.DataAnnotations;
using LICORERIA.Infraestructura.Services;

namespace LICORERIA.Core.Models
{
    public class Auditoria
    {
        [Key]
        public int IdAuditoria { get; set; }

        public string Tabla { get; set; }

        public string Registro { get; set; }

        public string Accion { get; set; }

        public DateTime Fecha { get; set; }

        // Relación con Usuario (* a 1)
        public int IdUsuario { get; set; }

        public Usuario Usuario { get; set; }

        // Método solicitado en el diagrama
        public void RegistrarAccion()
        {
            Fecha = HoraBoliviaHelper.Ahora();
        }
    }
}