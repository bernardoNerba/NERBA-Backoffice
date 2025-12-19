using System.Globalization;

namespace NERBABO.ApiService.Helper;

public class StringDateOnlyConverter
{
    private static readonly CultureInfo PortugueseCulture = new("pt-PT");
    private static readonly string[] DateFormats = { "dd/MM/yyyy", "dd-MM-yyyy" };

    public static DateOnly? ConvertToDateOnly(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return null;
        }

        // Try parsing with Portuguese culture and specific formats
        if (DateOnly.TryParseExact(dateString, DateFormats, PortugueseCulture, DateTimeStyles.None, out DateOnly date))
        {
            return date;
        }

        // Fallback to standard parsing with Portuguese culture
        if (DateOnly.TryParse(dateString, PortugueseCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        return null;
    }

    public static string? ConvertToString(DateOnly? date)
    {
        if (date.HasValue)
        {
            return date.Value.ToString("dd/MM/yyyy", PortugueseCulture);
        }
        return null;
    }
}
