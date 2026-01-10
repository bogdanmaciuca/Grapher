using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Grapher.Validation
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // Null is valid (unless [Required] is used)
                return ValidationResult.Success;
            }

            var currentValue = (DateTime)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
            {
                return new ValidationResult($"Unknown property: {_comparisonProperty}");
            }

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);

            // If the start date is null, we can't compare, so we assume valid
            if (comparisonValue == null)
            {
                 return ValidationResult.Success;
            }
            
            if (comparisonValue is DateTime startDate)
            {
                if (currentValue < startDate)
                {
                    return new ValidationResult($"The field {validationContext.DisplayName} must be later than {_comparisonProperty}.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
