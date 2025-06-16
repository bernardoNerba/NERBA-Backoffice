using NERBABO.ApiService.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace NERBABO.ApiService.Helper.Validators
{
    public class ValidateHoursAttribute : ValidationAttribute
    {
        public double MinHours { get; set; } = 0;
        public double MaxHours { get; set; } = 8760; // Maximum hours in a year (365 * 24)
        public bool AllowZero { get; set; } = true;

        public ValidateHoursAttribute()
        {
            ErrorMessage = "O campo {0} deve ser um valor numérico válido entre {1} e {2} horas.";
        }

        public ValidateHoursAttribute(double minHours, double maxHours, bool allowZero = true)
        {
            MinHours = minHours;
            MaxHours = maxHours;
            AllowZero = allowZero;
            ErrorMessage = "O campo {0} deve ser um valor numérico válido entre {1} e {2} horas.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Let [Required] handle null validation
            }

            // Handle different numeric types that might be passed
            double hours;

            if (value is float floatValue)
            {
                // Check if float is valid (not NaN or Infinity)
                if (float.IsNaN(floatValue) || float.IsInfinity(floatValue))
                {
                    return new ValidationResult(
                        FormatErrorMessage(validationContext.DisplayName),
                        new[] { validationContext.MemberName });
                }
                hours = floatValue;
            }
            else if (value is double doubleValue)
            {
                if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
                {
                    return new ValidationResult(
                        FormatErrorMessage(validationContext.DisplayName),
                        new[] { validationContext.MemberName });
                }
                hours = doubleValue;
            }
            else if (value is decimal decimalValue)
            {
                hours = (double)decimalValue;
            }
            else if (value is int intValue)
            {
                hours = intValue;
            }
            else if (value is string stringValue)
            {
                // Try to parse string as double
                if (!double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out hours))
                {
                    return new ValidationResult(
                        FormatErrorMessage(validationContext.DisplayName),
                        new[] { validationContext.MemberName });
                }
            }
            else
            {
                // Unsupported type
                return new ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName),
                    new[] { validationContext.MemberName });
            }

            // Validate hours range
            if (!AllowZero && hours == 0)
            {
                return new ValidationResult(
                    $"O campo {validationContext.DisplayName} deve ser maior que zero.",
                    new[] { validationContext.MemberName });
            }

            if (hours < MinHours || hours > MaxHours)
            {
                return new ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName),
                    new[] { validationContext.MemberName });
            }

            // Additional validation for reasonable hour values
            if (hours < 0)
            {
                return new ValidationResult(
                    $"O campo {validationContext.DisplayName} não pode ser negativo.",
                    new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, ErrorMessageString, name, MinHours, MaxHours);
        }
    }
}


