
namespace NERBABO.ApiService.Helper;

public class StringDateOnlyConverter
{
    public static DateOnly? ConvertToDateOnly(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return null;
        }
        if (DateOnly.TryParse(dateString, out DateOnly date))
        {
            return date;
        }
        return null;
    }
    public static string? ConvertToString(DateOnly? date)
    {
        if (date.HasValue)
        {
            return date.Value.ToString("dd-MM-yyyy");
        }
        return null;
    }
}
