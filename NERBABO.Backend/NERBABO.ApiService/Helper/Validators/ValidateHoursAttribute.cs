using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace NERBABO.ApiService.Helper.Validators
{
    /// <summary>
    /// Custom validation attribute to check whether a value represents a valid number of hours.
    /// </summary>
    public class ValidateHoursAttribute : ValidationAttribute
    {
        public double MinHours { get; set; } = 0;
        public double MaxHours { get; set; } = 8760; // Max hours in a year (365 * 24)
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

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Let [Required] handle nulls
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (!TryConvertToDouble(value, out var hours))
            {
                return new ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName),
                    new[] { validationContext.MemberName }.Where(m => m is not null)! // safe cast to IEnumerable<string>
                );
            }

            if (!AllowZero && hours == 0)
            {
                return new ValidationResult(
                    $"O campo {validationContext.DisplayName} deve ser maior que zero.",
                    new[] { validationContext.MemberName }.Where(m => m is not null)!
                    );
            }

            if (hours < 0)
            {
                return new ValidationResult(
                    $"O campo {validationContext.DisplayName} não pode ser negativo.",
                    new[] { validationContext.MemberName }.Where(m => m is not null)!
                    );
            }

            if (hours < MinHours || hours > MaxHours)
            {
                return new ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName),
                    new[] { validationContext.MemberName }.Where(m => m is not null)!
                    );
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Converts supported numeric/string values to double safely.
        /// </summary>
        private bool TryConvertToDouble(object value, out double result)
        {
            result = 0;

            switch (value)
            {
                case float f when !float.IsNaN(f) && !float.IsInfinity(f):
                    result = f;
                    return true;

                case double d when !double.IsNaN(d) && !double.IsInfinity(d):
                    result = d;
                    return true;

                case decimal dec:
                    result = (double)dec;
                    return true;

                case int i:
                    result = i;
                    return true;

                case long l:
                    result = l;
                    return true;

                case string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                    result = parsed;
                    return true;

                default:
                    return false;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, ErrorMessageString, name, MinHours, MaxHours);
        }
    }
}
