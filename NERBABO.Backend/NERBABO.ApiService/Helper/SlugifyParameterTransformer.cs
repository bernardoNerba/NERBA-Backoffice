namespace NERBABO.ApiService.Helper;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;

        // Convert to lowercase and return
        return value.ToString()?.ToLowerInvariant();
    }
}