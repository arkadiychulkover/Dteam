using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Validation
{
    /// <summary>
    /// Declarative password complexity validation attribute.
    /// Replaces imperative checks in services/controllers with standard DataAnnotations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public int MinLength { get; set; } = 7;
        public bool RequireLetter { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool DisallowSpaces { get; set; } = true;

        public bool AllowNullOrEmpty { get; set; } = false;
        public string? EmptyErrorMessage { get; set; }

        public string? MinLengthErrorMessage { get; set; }
        public string? RequireLetterErrorMessage { get; set; }
        public string? RequireDigitErrorMessage { get; set; }
        public string? DisallowSpacesErrorMessage { get; set; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var memberNames = validationContext.MemberName != null
                ? new[] { validationContext.MemberName }
                : null;

            if (value is null)
            {
                if (AllowNullOrEmpty) return ValidationResult.Success;
                var msg = EmptyErrorMessage ?? ErrorMessage ?? "Пароль обов'язковий і не може бути порожнім.";
                return new ValidationResult(msg, memberNames);
            }

            var password = value.ToString();
            if (string.IsNullOrEmpty(password))
            {
                if (AllowNullOrEmpty) return ValidationResult.Success;
                var msg = EmptyErrorMessage ?? ErrorMessage ?? "Пароль обов'язковий і не може бути порожнім.";
                return new ValidationResult(msg, memberNames);
            }

            var trimmed = password.Trim();

            // 1. Length check
            if (trimmed.Length < MinLength)
            {
                var msg = MinLengthErrorMessage ?? ErrorMessage ?? $"Новий пароль має містити щонайменше {MinLength} символів.";
                return new ValidationResult(msg, memberNames);
            }

            // 2. Letter check
            if (RequireLetter && !trimmed.Any(char.IsLetter))
            {
                var msg = RequireLetterErrorMessage ?? ErrorMessage ?? "Пароль має містити щонайменше одну літеру.";
                return new ValidationResult(msg, memberNames);
            }

            // 3. Digit check
            if (RequireDigit && !trimmed.Any(char.IsDigit))
            {
                var msg = RequireDigitErrorMessage ?? ErrorMessage ?? "Пароль має містити щонайменше одну цифру.";
                return new ValidationResult(msg, memberNames);
            }

            // 4. Whitespace check (no spaces allowed)
            if (DisallowSpaces && (password.Contains(' ') || password.Any(char.IsWhiteSpace)))
            {
                var msg = DisallowSpacesErrorMessage ?? ErrorMessage ?? "Пароль не повинен містити пробілів.";
                return new ValidationResult(msg, memberNames);
            }

            return ValidationResult.Success;
        }
    }
}
