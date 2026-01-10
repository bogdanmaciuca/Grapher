using System;
using System.ComponentModel.DataAnnotations;

namespace Grapher.Validation
{
    public class SensibleDateAttribute : ValidationAttribute
    {
        public int MinYear { get; set; } = 1900;
        public int MaxYear { get; set; } = 2200;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // Null is valid (use [Required] to disallow null)
                return ValidationResult.Success;
            }

            if (value is DateTime dt)
            {
                if (dt.Year < MinYear || dt.Year > MaxYear)
                {
                    return new ValidationResult($"The field {validationContext.DisplayName} must be between year {MinYear} and {MaxYear}.");
                }
                return ValidationResult.Success;
            }

            return new ValidationResult($"The field {validationContext.DisplayName} must be a valid date.");
        }
    }
}
