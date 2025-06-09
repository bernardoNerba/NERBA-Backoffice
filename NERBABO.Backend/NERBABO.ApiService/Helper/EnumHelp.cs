using Humanizer;
using System;

namespace NERBABO.ApiService.Helper
{
    public static class EnumHelp
    {
        public static bool IsValidEnum<T>(string value) where T : struct, Enum
        {
            return Enum.GetValues<T>()
                .Select(x => x.Humanize().Transform(To.TitleCase))
                .Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
        }
    }
}
