using System.Globalization;

namespace LICORERIA.Core.Mapedores
{
    public static class DateOnlyMapeador
    {
        private static readonly string[] Formatos =
        {
            "MM/dd/yyyy", // USA
            "dd/MM/yyyy", // LATAM/EU
            "yyyy-MM-dd",
            "MM-dd-yyyy",
            "dd-MM-yyyy",
            "yyyy/MM/dd"
        };

        public static DateOnly? toDateOnly(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            bool ok = DateOnly.TryParseExact(
                value.Trim(),
                Formatos,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly fecha
            );

            return ok ? fecha : null;
        }
    }
}