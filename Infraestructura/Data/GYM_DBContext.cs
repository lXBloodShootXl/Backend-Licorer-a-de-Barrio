using Microsoft.EntityFrameworkCore;
using LICORERIA.Core.Models;

namespace LICORERIA.Infraestructura.Data
{
    public class LICORERIA_DBContext : DbContext
    {
        public LICORERIA_DBContext(DbContextOptions<LICORERIA_DBContext> options) : base(options) { }

        public DbSet<Persona> Personas { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Recorre todas las entidades y propiedades DateTime
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    // Si la propiedad es DateTime o DateTime?
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("date"); // Se guarda como "date" en PostgreSQL
                    }
                }
            }
            //modelBuilder.Entity<Persona>().HasIndex(p => p.hashhuella).IsUnique();
        }
    }
}
