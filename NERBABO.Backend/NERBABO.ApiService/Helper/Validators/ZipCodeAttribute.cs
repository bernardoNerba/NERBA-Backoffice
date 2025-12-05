using System.ComponentModel.DataAnnotations;
namespace NerbaApp.Api.Validators;

/// <summary>
/// Validation attribute to ensure a postal code matches the format "NNNN-NNN",
/// where N is a digit and a hyphen separates the 4th and 5th characters.
/// Accepts empty strings as valid.
/// </summary>
public class ZipCodeAttribute : ValidationAttribute
{
    /// <summary>
    /// Validates that the input is either empty or matches the format "1234-567".
    /// Ensures all digits are numeric and the hyphen is in the correct position.
    /// </summary>
    /// <param name="value">The value to validate. Expected to be a string.</param>
    /// <param name="validationContext">Context of the validation operation. Not used.</param>
    /// <returns>
    /// <see cref="ValidationResult.Success"/> if the value is empty or a valid Zip code;
    /// otherwise, a <see cref="ValidationResult"/> with an error message.
    /// </returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Accept null values as valid (for optional fields)
        if (value is null)
        {
            return ValidationResult.Success;
        }

        // Ensure the value is a string
        if (value is string postalCode)
        {
            // Accept empty or whitespace string as valid
            if (string.IsNullOrWhiteSpace(postalCode))
            {
                return ValidationResult.Success;
            }

            // Validate exact format: 4 digits, hyphen, 3 digits
            if (postalCode.Length == 8 &&
                postalCode[4] == '-' &&
                postalCode.Take(4).All(char.IsDigit) &&
                postalCode.Skip(5).All(char.IsDigit))
            {
                return ValidationResult.Success;
            }
        }

        // Invalid format or value type
        return new ValidationResult(ErrorMessage
            ?? "Código Postal deve conter exatamente 8 caracteres no formato '1234-567'.");
    }
}
