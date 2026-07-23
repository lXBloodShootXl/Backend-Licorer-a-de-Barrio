using LICORERIA.Core.Models;
using LICORERIA.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace LICORERIA.Infraestructura.Data
{
    public class LICORERIA_DBContext : DbContext
    {
        private readonly UsuarioActualService _usuarioActual = null!;
        public LICORERIA_DBContext(
    DbContextOptions<LICORERIA_DBContext> options,
    UsuarioActualService usuarioActual)
    : base(options)
        {
            _usuarioActual = usuarioActual;
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Producto> Productos { get; set; }

        public DbSet<Venta> Ventas { get; set; }

        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }

        public DbSet<Auditoria> Auditorias { get; set; }
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(x => x.Compra)
                .WithMany(x => x.Detalles)
                .HasForeignKey(x => x.IdCompra);

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.IdProducto);


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

            modelBuilder.Entity<Auditoria>()
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
        public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
        {
            var auditorias = new List<Auditoria>();
            var idUsuario = _usuarioActual.ObtenerIdUsuario();

            if (idUsuario == null)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            var entradas = ChangeTracker.Entries()
                .Where(e =>
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted);

            foreach (var entry in entradas)
            {
                if (entry.Entity is Auditoria)
                    continue;

                string accion = entry.State switch
                {
                    EntityState.Added => "INSERT",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => ""
                };

                auditorias.Add(new Auditoria
                {
                    Tabla = entry.Entity.GetType().Name,
                    Registro = System.Text.Json.JsonSerializer.Serialize(entry.Entity),
                    Accion = accion,
                    Fecha = DateTime.Now,
                    IdUsuario = idUsuario.Value
                });
            }

            var resultado = await base.SaveChangesAsync(cancellationToken);

            if (auditorias.Any())
            {
                Auditorias.AddRange(auditorias);
                await base.SaveChangesAsync(cancellationToken);
            }

            return resultado;
        }
    }
}
