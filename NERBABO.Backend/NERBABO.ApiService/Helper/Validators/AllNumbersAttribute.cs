using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Helper.Validators
{
    /// <summary>
    /// Validation attribute to ensure a string contains only numeric characters (0–9).
    /// </summary>
    public class AllNumbersAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates that the input value is a non-null string consisting only of numeric digits.
        /// </summary>
        /// <param name="value">The value to validate. Expected to be a string.</param>
        /// <param name="validationContext">Context for the validation operation. Not used in this implementation.</param>
        /// <returns>
        /// <see cref="ValidationResult.Success"/> if the value is a string and contains only digits;
        /// otherwise, a <see cref="ValidationResult"/> with an error message.
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Check if the value is a string
            if (value is string str)
            {
                // Accept empty/null strings as valid (for optional fields)
                if (string.IsNullOrEmpty(str))
                {
                    return ValidationResult.Success;
                }

                // Validate that all characters are digits
                if (str.All(char.IsDigit))
                {
                    return ValidationResult.Success;
                }
            }

            // Accept null values as valid (for optional fields)
            if (value is null)
            {
                return ValidationResult.Success;
            }

            // Return error if validation fails, using custom error message if set
            return new ValidationResult(ErrorMessage ?? "Todos os caractéres devem ser numeros.");
        }
    }
}
