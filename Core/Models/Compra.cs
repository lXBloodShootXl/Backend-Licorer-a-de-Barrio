using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LICORERIA.Core.Models
{
    public class Compra
    {
        [Key]
        public int IdCompra { get; set; }
        
        public DateTime Fecha { get; set; }
        
        [Required]
        [MaxLength(150)]
        public string NombreProveedor { get; set; }
        
        public decimal Total { get; set; }

        public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
    }
}
