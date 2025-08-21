using System;

namespace NERBABO.ApiService.Helper;

public static class UrlHelper
{
    public static string GetBaseUrl()
    {
        HttpContextAccessor httpContextAccessor = new();
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null) return "";

        return $"{request.Scheme}://{request.Host}";
    }
}
