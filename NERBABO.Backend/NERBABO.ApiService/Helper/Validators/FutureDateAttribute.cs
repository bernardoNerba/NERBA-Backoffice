using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Helper.Validators
{
    /// <summary>
    /// Validation attribute to ensure that a date value represents a future date.
    /// Accepts empty input as valid.
    /// </summary>
    public class FutureDateAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates that the provided value is a valid <see cref="DateOnly"/> string
        /// and represents a date that is later than today.
        /// </summary>
        /// <param name="value">The value to validate. Expected to be a string or convertible to string.</param>
        /// <param name="validationContext">Provides context for the validation operation. Not used.</param>
        /// <returns>
        /// <see cref="ValidationResult.Success"/> if the value is null, empty, or a valid future date;
        /// otherwise, a <see cref="ValidationResult"/> with an error message.
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Convert the input to a string, default to empty if null
            string stringValue = value?.ToString() ?? string.Empty;

            // Allow empty input (optional field scenario)
            if (string.IsNullOrEmpty(stringValue))
            {
                return ValidationResult.Success;
            }

            // Try to parse the string into a DateOnly object
            else if (DateOnly.TryParse(stringValue, out DateOnly date))
            {
                // Validate that the date is in the future (after today)
                if (date > DateOnly.FromDateTime(DateTime.Today))
                {
                    return ValidationResult.Success;
                }
            }

            // Return validation error if the date is invalid or not in the future
            return new ValidationResult(ErrorMessage
                ?? "A data deve ser posterior à atual."); // "The date must be in the future."
        }
    }
}
