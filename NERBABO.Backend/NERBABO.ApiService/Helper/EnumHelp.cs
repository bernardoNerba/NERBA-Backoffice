using System.ComponentModel;
using System.Reflection;

namespace NERBABO.ApiService.Helper;

/// <summary>
/// Provides helper methods for working with enum values and their <see cref="DescriptionAttribute"/>.
/// </summary>
[Obsolete("Use Humanizer instead.")]
public class EnumHelp
{
    /// <summary>
    /// Gets the description of an enum value from the <see cref="DescriptionAttribute"/>.
    /// If no description is found, returns the enum's name as a fallback.
    /// </summary>
    /// <param name="value">The enum value to get the description for.</param>
    /// <returns>The description string, or the enum name if no description attribute is present.</returns>
    public static string GetDescription(Enum value)
    {
        FieldInfo? fi = value.GetType().GetField(value.ToString());

        if (fi != null && Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
        {
            return descriptionAttribute.Description;
        }

        return value.ToString(); // fallback
    }

    /// <summary>
    /// Gets the enum value of type <typeparamref name="T"/> that matches a given description.
    /// Checks both the <see cref="DescriptionAttribute"/> and the enum name.
    /// </summary>
    /// <typeparam name="T">The enum type to search.</typeparam>
    /// <param name="description">The description string to match.</param>
    /// <returns>The enum value that matches the description or name.</returns>
    /// <exception cref="ArgumentException">Thrown when no matching enum value is found.</exception>
    public static T? GetValueFromDescription<T>(string description) where T : Enum
    {
        var type = typeof(T);
        foreach (var field in type.GetFields())
        {
            if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == description)
                    return (T?)field.GetValue(null);
            }
            else if (field.Name == description)
            {
                return (T?)field.GetValue(null);
            }
        }

        throw new ArgumentException($"Not found: {description}", nameof(description));
    }
}