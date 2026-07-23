using LICORERIA.Infraestructura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LICORERIA.Presentacion.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReporteController : ControllerBase
    {
        private readonly LICORERIA_DBContext _context;

        public ReporteController(
            LICORERIA_DBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Genera el reporte del inventario.
        /// Muestra productos disponibles, agotados y con stock bajo.
        /// </summary>
        [HttpGet("Inventario")]
        public async Task<IActionResult> ReporteInventario()
        {
            var reporte = await _context.Productos
                .Where(p => p.Activo)
                .Select(p => new
                {
                    p.IdProducto,
                    p.Nombre,
                    p.Categoria,
                    p.CodigoBarras,
                    p.PrecioCompra,
                    p.PrecioVenta,
                    p.StockActual,
                    p.StockMinimo,

                    Estado =
                        p.StockActual == 0
                            ? "Agotado"
                            : p.StockActual <= p.StockMinimo
                                ? "Stock Bajo"
                                : "Disponible"
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return Ok(reporte);
        }


        /// <summary>
        /// US-21: Reporte de ventas diario.
        /// Si no se envía fecha, usa el día de hoy.
        /// </summary>
        [HttpGet("Ventas/Diario")]
        public async Task<IActionResult> ReporteVentasDiario(
            DateTime? fecha)
        {
            var dia = fecha.HasValue
                ? fecha.Value.Date
                : DateTime.Today;


            var ventas =
                await _context.Ventas
                .Where(v => v.Fecha == dia)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();


            return Ok(new
            {
                periodo       = "Diario",
                fecha         = dia.ToString("yyyy-MM-dd"),
                totalVentas   = ventas.Count,
                totalIngresos = ventas.Sum(v => v.Total),
                totalGanancia = ventas.Sum(v => v.Ganancia),

                ventas = ventas.Select(v => new
                {
                    v.IdVenta,
                    v.Hora,
                    v.Total,
                    v.Ganancia,
                    productos = v.Detalles.Select(d => new
                    {
                        d.Producto.Nombre,
                        d.Cantidad,
                        d.PrecioUnitario,
                        d.Subtotal
                    })
                })
            });
        }


        /// <summary>
        /// US-21: Reporte de ventas semanal.
        /// Si no se envía fecha, usa la semana actual (lunes a domingo).
        /// </summary>
        [HttpGet("Ventas/Semanal")]
        public async Task<IActionResult> ReporteVentasSemanal(
            DateTime? fecha)
        {
            var referencia = fecha.HasValue
                ? fecha.Value.Date
                : DateTime.Today;


            // Calcular lunes y domingo de la semana de referencia
            int diasDesdeElLunes =
                ((int)referencia.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

            var inicioSemana = referencia.AddDays(-diasDesdeElLunes);
            var finSemana    = inicioSemana.AddDays(6);


            var ventas =
                await _context.Ventas
                .Where(v =>
                    v.Fecha >= inicioSemana &&
                    v.Fecha <= finSemana)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();


            return Ok(new
            {
                periodo       = "Semanal",
                desde         = inicioSemana.ToString("yyyy-MM-dd"),
                hasta         = finSemana.ToString("yyyy-MM-dd"),
                totalVentas   = ventas.Count,
                totalIngresos = ventas.Sum(v => v.Total),
                totalGanancia = ventas.Sum(v => v.Ganancia),

                ventas = ventas.Select(v => new
                {
                    v.IdVenta,
                    v.Fecha,
                    v.Hora,
                    v.Total,
                    v.Ganancia,
                    productos = v.Detalles.Select(d => new
                    {
                        d.Producto.Nombre,
                        d.Cantidad,
                        d.PrecioUnitario,
                        d.Subtotal
                    })
                })
            });
        }


        /// <summary>
        /// US-21: Reporte de ventas mensual.
        /// Si no se envían año y mes, usa el mes actual.
        /// </summary>
        [HttpGet("Ventas/Mensual")]
        public async Task<IActionResult> ReporteVentasMensual(
            int? anio,
            int? mes)
        {
            int a = anio ?? DateTime.Today.Year;
            int m = mes  ?? DateTime.Today.Month;


            if (m < 1 || m > 12)
            {
                return BadRequest(
                    "El mes debe estar entre 1 y 12.");
            }


            var inicioMes = new DateTime(a, m, 1);
            var finMes    = inicioMes.AddMonths(1).AddDays(-1);


            var ventas =
                await _context.Ventas
                .Where(v =>
                    v.Fecha >= inicioMes &&
                    v.Fecha <= finMes)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();


            return Ok(new
            {
                periodo       = "Mensual",
                anio          = a,
                mes           = m,
                desde         = inicioMes.ToString("yyyy-MM-dd"),
                hasta         = finMes.ToString("yyyy-MM-dd"),
                totalVentas   = ventas.Count,
                totalIngresos = ventas.Sum(v => v.Total),
                totalGanancia = ventas.Sum(v => v.Ganancia),

                ventas = ventas.Select(v => new
                {
                    v.IdVenta,
                    v.Fecha,
                    v.Hora,
                    v.Total,
                    v.Ganancia,
                    productos = v.Detalles.Select(d => new
                    {
                        d.Producto.Nombre,
                        d.Cantidad,
                        d.PrecioUnitario,
                        d.Subtotal
                    })
                })
            });
        }


        /// <summary>
        /// US-22: Reporte de ganancias diario.
        /// Si no se envía fecha, usa el día de hoy.
        /// </summary>
        [HttpGet("Ganancias/Diario")]
        public async Task<IActionResult> ReporteGananciasDiario(
            DateTime? fecha)
        {
            var dia = fecha.HasValue
                ? fecha.Value.Date
                : DateTime.Today;


            var ventas =
                await _context.Ventas
                .Where(v => v.Fecha == dia)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();


            var detalleProductos = ventas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.Producto.Nombre)
                .Select(g => new
                {
                    producto          = g.Key,
                    cantidadVendida   = g.Sum(d => d.Cantidad),
                    ingresos          = g.Sum(d => d.Subtotal),
                    ganancia          =
                        g.Sum(d =>
                            (d.PrecioUnitario -
                             d.Producto.PrecioCompra)
                             * d.Cantidad)
                })
                .OrderByDescending(x => x.ganancia)
                .ToList();


            return Ok(new
            {
                periodo          = "Diario",
                fecha            = dia.ToString("yyyy-MM-dd"),
                totalVentas      = ventas.Count,
                totalIngresos    = ventas.Sum(v => v.Total),
                totalGanancia    = ventas.Sum(v => v.Ganancia),
                rentabilidad     =
                    ventas.Sum(v => v.Total) > 0
                        ? Math.Round(
                            ventas.Sum(v => v.Ganancia) /
                            ventas.Sum(v => v.Total) * 100, 2)
                        : 0,
                gananciaPorProducto = detalleProductos
            });
        }


        /// <summary>
        /// US-22: Reporte de ganancias semanal.
        /// Si no se envía fecha, usa la semana actual (lunes a domingo).
        /// </summary>
        [HttpGet("Ganancias/Semanal")]
        public async Task<IActionResult> ReporteGananciasSemanal(
            DateTime? fecha)
        {
            var referencia = fecha.HasValue
                ? fecha.Value.Date
                : DateTime.Today;


            int diasDesdeElLunes =
                ((int)referencia.DayOfWeek -
                 (int)DayOfWeek.Monday + 7) % 7;

            var inicioSemana = referencia.AddDays(-diasDesdeElLunes);
            var finSemana    = inicioSemana.AddDays(6);


            var ventas =
                await _context.Ventas
                .Where(v =>
                    v.Fecha >= inicioSemana &&
                    v.Fecha <= finSemana)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();


            var detalleProductos = ventas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.Producto.Nombre)
                .Select(g => new
                {
                    producto          = g.Key,
                    cantidadVendida   = g.Sum(d => d.Cantidad),
                    ingresos          = g.Sum(d => d.Subtotal),
                    ganancia          =
                        g.Sum(d =>
                            (d.PrecioUnitario -
                             d.Producto.PrecioCompra)
                             * d.Cantidad)
                })
                .OrderByDescending(x => x.ganancia)
                .ToList();


            return Ok(new
            {
                periodo          = "Semanal",
                desde            = inicioSemana.ToString("yyyy-MM-dd"),
                hasta            = finSemana.ToString("yyyy-MM-dd"),
                totalVentas      = ventas.Count,
                totalIngresos    = ventas.Sum(v => v.Total),
                totalGanancia    = ventas.Sum(v => v.Ganancia),
                rentabilidad     =
                    ventas.Sum(v => v.Total) > 0
                        ? Math.Round(
                            ventas.Sum(v => v.Ganancia) /
                            ventas.Sum(v => v.Total) * 100, 2)
                        : 0,
                gananciaPorProducto = detalleProductos
            });
        }


        /// <summary>
        /// US-22: Reporte de ganancias mensual.
        /// Si no se envían año y mes, usa el mes actual.
        /// </summary>
        [HttpGet("Ganancias/Mensual")]
        public async Task<IActionResult> ReporteGananciasMensual(
            int? anio,
            int? mes)
        {
            int a = anio ?? DateTime.Today.Year;
            int m = mes  ?? DateTime.Today.Month;


            if (m < 1 || m > 12)
            {
                return BadRequest(
                    "El mes debe estar entre 1 y 12.");
            }


            var inicioMes = new DateTime(a, m, 1);
            var finMes    = inicioMes.AddMonths(1).AddDays(-1);


            var ventas =
                await _context.Ventas
                .Where(v =>
                    v.Fecha >= inicioMes &&
                    v.Fecha <= finMes)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();


            var detalleProductos = ventas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.Producto.Nombre)
                .Select(g => new
                {
                    producto          = g.Key,
                    cantidadVendida   = g.Sum(d => d.Cantidad),
                    ingresos          = g.Sum(d => d.Subtotal),
                    ganancia          =
                        g.Sum(d =>
                            (d.PrecioUnitario -
                             d.Producto.PrecioCompra)
                             * d.Cantidad)
                })
                .OrderByDescending(x => x.ganancia)
                .ToList();


            return Ok(new
            {
                periodo          = "Mensual",
                anio             = a,
                mes              = m,
                desde            = inicioMes.ToString("yyyy-MM-dd"),
                hasta            = finMes.ToString("yyyy-MM-dd"),
                totalVentas      = ventas.Count,
                totalIngresos    = ventas.Sum(v => v.Total),
                totalGanancia    = ventas.Sum(v => v.Ganancia),
                rentabilidad     =
                    ventas.Sum(v => v.Total) > 0
                        ? Math.Round(
                            ventas.Sum(v => v.Ganancia) /
                            ventas.Sum(v => v.Total) * 100, 2)
                        : 0,
                gananciaPorProducto = detalleProductos
            });
        }
    }
}