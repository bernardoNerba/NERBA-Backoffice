using System.ComponentModel.DataAnnotations;

namespace NerbaApp.Api.Validators
{
    /// <summary>
    /// Validation attribute that applies string length constraints
    /// only if the input value is not null or empty.
    /// </summary>
    public class ValidateLengthIfNotEmptyAttribute : StringLengthAttribute
    {
        /// <summary>
        /// Initializes the attribute with a maximum allowed length.
        /// </summary>
        /// <param name="maximumLength">The maximum length allowed for the string.</param>
        public ValidateLengthIfNotEmptyAttribute(int maximumLength)
            : base(maximumLength)
        {
        }

        /// <summary>
        /// Initializes the attribute with minimum and maximum allowed lengths.
        /// </summary>
        /// <param name="maximumLength">The maximum length allowed for the string.</param>
        /// <param name="minimumLength">The minimum length allowed for the string.</param>
        public ValidateLengthIfNotEmptyAttribute(int maximumLength, int minimumLength)
            : base(maximumLength)
        {
            MinimumLength = minimumLength;
        }

        /// <summary>
        /// Determines whether the specified value is valid.
        /// Skips validation if the input is null or empty.
        /// </summary>
        /// <param name="value">The value of the field being validated.</param>
        /// <returns>
        /// <c>true</c> if the value is null, empty, or satisfies the length constraints;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid(object? value)
        {
            // Skip length validation for null or empty strings
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return true;
            }

            // Apply the base StringLength validation
            return base.IsValid(value);
        }
    }
}
