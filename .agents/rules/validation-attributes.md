---
trigger: always_on
---

# Rule: Declarative Input Validation via DataAnnotations & Validation Attributes

## 1. Область применения
- Правило применяется ко всем входящим DTO, параметрам запросов, контроллерам API и сервисам бэкенда (`DteamBackend`).
- Охватывает проверку формата данных, длины строк, обязательности полей, наличия символов/цифр/букв, диапазонов чисел, отсутствия пробелов, совпадения паролей и кросс-полевой валидации моделей.

## 2. Строгий запрет на императивную валидацию формата входных данных
- **ЗАПРЕЩАЕТСЯ** писать ручные проверки формата, длины, символов и обязательности полей внутри методов контроллеров или сервисов:
  - ❌ `if (newPass.Length < 7) return (false, "...")`
  - ❌ `if (!newPass.Any(char.IsLetter)) return (false, "...")`
  - ❌ `if (!newPass.Any(char.IsDigit)) return (false, "...")`
  - ❌ `if (newPass.Contains(' ')) return (false, "...")`
  - ❌ `if (string.IsNullOrWhiteSpace(dto.Content)) return BadRequest(...)`
  - ❌ `if (dto.Amount <= 0) return BadRequest(...)`
- Вся валидация входных данных обязана объявляться **декларативно** непосредственно в DTO с помощью атрибутов валидации (`System.ComponentModel.DataAnnotations` или кастомных `ValidationAttribute`).

## 3. Четкое разделение ответственности (Separation of Concerns)
1. **DTO и Validation Attributes**:
   - Отвечают за синтаксическую корректность, наличие значений, минимальную/максимальную длину, регулярные выражения, диапазоны чисел, сложность паролей, отсутствие недопустимых пробелов.
   - Выполняются автоматически пайплайном ASP.NET Core (`[ApiController]`) до входа в тело метода контроллера.
2. **IValidatableObject на DTO**:
   - Применяется для кросс-полевой валидации входного DTO (например, если поле `B` обязательно только при условии, что флаг `A == true`).
3. **Контроллеры (Controllers)**:
   - Отвечают за маршрутизацию, получение контекста пользователя (`ClaimsPrincipal`), вызов сервисов и возврат HTTP-ответов (`Ok`, `NotFound`, `BadRequest` по бизнес-ошибкам).
4. **Сервисы (Services)**:
   - Отвечают **исключительно за бизнес-логику**:
     - Проверка существования сущности в БД (`FirstOrDefaultAsync`).
     - Проверка уникальности в БД (`AnyAsync(u => u.Email == ...)`).
     - Сверка хэшей паролей с БД через криптографический сервис (`VerifyPasswordHash`).
     - Проверка прав доступа и владения ресурсом.
     - Сохранение изменений в БД и отправка событий/уведомлений.

## 4. Стандартные и кастомные атрибуты проекта

### Стандартные атрибуты (`System.ComponentModel.DataAnnotations`):
- `[Required(ErrorMessage = "...")]` — обязательность присутствия.
- `[MinLength(n, ErrorMessage = "...")]` / `[MaxLength(n, ErrorMessage = "...")]` — длина коллекций и строк.
- `[StringLength(max, MinimumLength = min, ErrorMessage = "...")]` — диапазон длины.
- `[Range(min, max, ErrorMessage = "...")]` — числовой диапазон.
- `[EmailAddress(ErrorMessage = "...")]` — формат email.
- `[Compare(nameof(OtherProperty), ErrorMessage = "...")]` — совпадение полей (например, подтверждение пароля).
- `[RegularExpression(pattern, ErrorMessage = "...")]` — формат по регулярному выражению.

### Кастомные атрибуты (`DteamBackend.Validation`):
Если требуется сложная составная валидация или специфические сообщения об ошибках, создается/используется кастомный атрибут в `DteamBackend.Validation`:
1. `[StrongPassword(MinLength = 7, RequireLetter = true, RequireDigit = true, DisallowSpaces = true)]`:
   - Проверяет минимальную длину пароля.
   - Требует наличие хотя бы одной буквы (`char.IsLetter`).
   - Требует наличие хотя бы одной цифры (`char.IsDigit`).
   - Запрещает любые пробельные символы (`char.IsWhiteSpace`).
   - Возвращает точные сообщения на украинском языке, синхронизированные с клиентским UI чек-листом.
2. `[RequiredNonWhiteSpace(ErrorMessage = "...")]`:
   - Гарантирует, что строка не является `null`, пустой или состоящей только из пробелов.

---

### Эталонные примеры

#### 1. DTO со сложной валидацией пароля:
```csharp
using System.ComponentModel.DataAnnotations;
using DteamBackend.Validation;

namespace DteamBackend.Models.DTO
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Поточний пароль обов'язковий")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новий пароль обов'язковий")]
        [StrongPassword(MinLength = 7)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердження паролю обов'язкове")]
        [Compare(nameof(NewPassword), ErrorMessage = "Паролі не співпадають")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
```

#### 2. Сервис без ручных проверок формата (только бизнес-логика):
```csharp
public async Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null)
    {
        return (false, "Користувача не знайдено.");
    }

    // 1. Сверка текущего пароля с хэшем из базы (бизнес-логика)
    if (!_passwordHasher.VerifyPasswordHash(dto.OldPassword, user.PasswordHash, user.PasswordSalt))
    {
        return (false, "Невірний поточний пароль.");
    }

    // 2. Бизнес-правило: новый пароль не должен быть равен старому
    var newPass = dto.NewPassword.Trim();
    if (_passwordHasher.VerifyPasswordHash(newPass, user.PasswordHash, user.PasswordSalt))
    {
        return (false, "Новий пароль не повинен співпадати з поточним паролем.");
    }

    // 3. Хэширование и сохранение
    _passwordHasher.CreatePasswordHash(newPass, out string newHash, out string newSalt);
    user.PasswordHash = newHash;
    user.PasswordSalt = newSalt;
    user.UpdatedAt = DateTime.UtcNow;

    await _jwtTokenService.RevokeUserTokensAsync(userId);
    await _db.SaveChangesAsync();

    return (true, null);
}
```

#### 3. Условная кросс-полевая валидация через `IValidatableObject`:
```csharp
public class CreateGameDto : IValidatableObject
{
    public bool IsPublished { get; set; } = true;

    [MaxLength(500)]
    public string? ServerArchivePath { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsPublished && string.IsNullOrWhiteSpace(ServerArchivePath))
        {
            yield return new ValidationResult(
                "Для публікації гри в каталозі необхідно завантажити файл білду гри (.zip). Без файлу білду проект можна зберегти лише як чернетку.",
                new[] { nameof(ServerArchivePath) });
        }
    }
}
```
