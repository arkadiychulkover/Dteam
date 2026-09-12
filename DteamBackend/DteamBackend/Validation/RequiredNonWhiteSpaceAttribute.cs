using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Validation
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RequiredNonWhiteSpaceAttribute : ValidationAttribute
    {
        public RequiredNonWhiteSpaceAttribute()
        {
        }

        public RequiredNonWhiteSpaceAttribute(string errorMessage) : base(errorMessage)
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                var msg = ErrorMessage ?? $"{validationContext.DisplayName} обов'язкове поле.";
                var memberNames = validationContext.MemberName != null ? new[] { validationContext.MemberName } : null;
                return new ValidationResult(msg, memberNames);
            }

            return ValidationResult.Success;
        }
    }
}
