using System;
using System.Linq;

namespace NERBABO.ApiService.Helper;

/// <summary>
/// Helper class for handling person name operations
/// </summary>
public static class NameHelper
{
    /// <summary>
    /// Splits a full name into first name and last name.
    /// Last word becomes last name, everything else becomes first name.
    /// </summary>
    /// <param name="fullName">The complete name to split</param>
    /// <returns>Tuple with FirstName and LastName</returns>
    /// <exception cref="ArgumentException">Thrown when fullName has less than 2 words</exception>
    /// <example>
    /// ("João Silva") returns ("João", "Silva")
    /// ("João Silva Santos") returns ("João Silva", "Santos")
    /// ("José María García") returns ("José María", "García")
    /// </example>
    public static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Nome completo não pode estar vazio", nameof(fullName));
        }

        var trimmed = fullName.Trim();
        var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            throw new ArgumentException(
                "Nome completo deve conter pelo menos duas palavras (nome próprio e apelido)",
                nameof(fullName));
        }

        var lastName = parts[^1];
        var firstName = string.Join(" ", parts[..^1]);

        return (firstName, lastName);
    }
}
