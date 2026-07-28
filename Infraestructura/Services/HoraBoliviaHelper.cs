namespace LICORERIA.Infraestructura.Services
{
    /// <summary>
    /// El servidor donde corre esta API guarda la hora del sistema en UTC,
    /// pero el negocio opera en horario de Bolivia (UTC-4, sin horario de
    /// verano). Usar HoraBoliviaHelper.Ahora() en vez de DateTime.Now en
    /// cualquier lugar donde se registre fecha/hora de un evento del
    /// negocio (ventas, compras, movimientos de inventario, auditoría).
    /// </summary>
    public static class HoraBoliviaHelper
    {
        // Bolivia no tiene horario de verano, así que el desfase con UTC
        // es siempre -4. Se usa un offset fijo (en vez de
        // TimeZoneInfo.FindSystemTimeZoneById) para no depender de que el
        // servidor tenga instalada la base de datos de zonas horarias
        // "America/La_Paz" (falla en algunos Windows sin ICU configurado).
        private static readonly TimeSpan OffsetBolivia = TimeSpan.FromHours(-4);

        public static DateTime Ahora()
        {
            return DateTime.UtcNow + OffsetBolivia;
        }
    }
}
