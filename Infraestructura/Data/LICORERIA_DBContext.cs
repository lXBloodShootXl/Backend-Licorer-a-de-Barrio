using Microsoft.EntityFrameworkCore;
using LICORERIA.Core.Models;

namespace LICORERIA.Infraestructura.Data
{
    public class LICORERIA_DBContext : DbContext
    {
        public LICORERIA_DBContext(DbContextOptions<LICORERIA_DBContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Producto> Productos { get; set; }

        public DbSet<Venta> Ventas { get; set; }

        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<DetalleVenta>()
                .HasOne(x => x.Venta)
                .WithMany(x => x.Detalles)
                .HasForeignKey(x => x.IdVenta);



            modelBuilder.Entity<DetalleVenta>()
                .HasOne(x => x.Producto)
                .WithMany(x => x.DetallesVenta)
                .HasForeignKey(x => x.IdProducto);



            modelBuilder.Entity<Venta>()
                .HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.IdUsuario);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.IdProducto);


            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.IdUsuario);
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
