using System;
using System.ComponentModel.DataAnnotations;

namespace NerbaApp.Api.Validators
{
    /// <summary>
    /// Validation attribute to ensure a date value represents a past date.
    /// Accepts empty input as valid.
    /// </summary>
    public class PastDateAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates that the provided value is a valid <see cref="DateOnly"/> string
        /// and represents a date that is earlier than today.
        /// </summary>
        /// <param name="value">The value to validate. Expected to be a string or convertible to string.</param>
        /// <param name="validationContext">Context for the validation operation (not used).</param>
        /// <returns>
        /// <see cref="ValidationResult.Success"/> if the value is null, empty, or a valid past date;
        /// otherwise, a <see cref="ValidationResult"/> with an error message.
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Convert the input to a string or use empty string if null
            string stringValue = value?.ToString() ?? string.Empty;

            // Allow empty input (optional field)
            if (string.IsNullOrEmpty(stringValue))
            {
                return ValidationResult.Success;
            }
            else
            {
                // Try to parse the string as a DateOnly object
                if (DateOnly.TryParse(stringValue, out DateOnly date))
                {
                    // Check if the date is in the past
                    if (date < DateOnly.FromDateTime(DateTime.Today))
                    {
                        return ValidationResult.Success;
                    }
                }
            }

            // Validation failed: not a past date or invalid format
            return new ValidationResult(ErrorMessage
                ?? "A data deve ser anterior à atual.");
        }
    }
}
