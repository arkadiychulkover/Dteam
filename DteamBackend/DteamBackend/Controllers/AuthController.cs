using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.DTO.Auth;
using DteamBackend.Models.Enums;
using DteamBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [ApiController]
    [EnableRateLimiting("AuthLimiter")]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly IAccountService _accountService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext db,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            IAccountService accountService,
            ILogger<AuthController> logger)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _accountService = accountService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var normalizedUsername = dto.Username.Trim();

            var existingUserByEmail = await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (existingUserByEmail != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            var existingUserByUsername = await _db.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername.ToLower());

            if (existingUserByUsername != null)
            {
                return BadRequest(new { message = "User with this username already exists" });
            }

            _passwordHasher.CreatePasswordHash(dto.Password, out string hash, out string salt);

            var user = new Duser
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                Username = normalizedUsername,
                PasswordHash = hash,
                PasswordSalt = salt,
                WalletAddress = string.IsNullOrWhiteSpace(dto.WalletAddress) ? null : dto.WalletAddress.Trim(),
                HardhatAddress = string.IsNullOrWhiteSpace(dto.HardhatAddress) ? null : dto.HardhatAddress.Trim(),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                Status = UserStatus.Online,
                BalanceInNanoTons = 0,
                TotalEarningsInNanoTons = 0,
                IsInFamily = false,
                IsAdmin = false,
                IsBanned = false
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                User = UserDto.FromEntity(user)
            });
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identifier = dto.EmailOrUsername.Trim().ToLowerInvariant();

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == identifier || u.Username.ToLower() == identifier);

            if (user == null)
            {
                return BadRequest(new { message = "Incorrect username/email or password" });
            }

            if (user.IsBanned)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are blocked" });
            }

            var isValidPassword = _passwordHasher.VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt);
            if (!isValidPassword)
            {
                return BadRequest(new { message = "Incorrect username/email or password" });
            }

            user.LastLoginAt = DateTime.UtcNow;
            user.Status = UserStatus.Online;
            await _db.SaveChangesAsync();

            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                User = UserDto.FromEntity(user)
            });
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _jwtTokenService.RefreshTokensAsync(dto.RefreshToken);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "You are unauthorized" });
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(UserDto.FromEntity(user));
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rawIdentifier = (!string.IsNullOrWhiteSpace(dto.EmailOrUsername) ? dto.EmailOrUsername : dto.Email).Trim();
            var normalized = rawIdentifier.ToLowerInvariant();

            _logger.LogInformation("[Auth] Отримано запит на скидання пароля для: {Identifier}", rawIdentifier);

            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Email.ToLower() == normalized ||
                u.Username.ToLower() == normalized);

            if (user == null)
            {
                _logger.LogWarning("[Auth] Користувача не знайдено за ідентифікатором: {Identifier}", rawIdentifier);
                return Ok(new
                {
                    message = "Якщо обліковий запис існує, інструкції надіслано на вашу пошту.",
                    debugCode = (string?)null,
                    userEmail = (string?)null
                });
            }

            var resetCode = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetCode;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
            await _db.SaveChangesAsync();

            bool emailSent = false;
            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetCode);
                emailSent = true;
                _logger.LogInformation("[Auth] Лист скидання пароля успішно надіслано на {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth] Не вдалося надіслати лист на {Email}. Використовується тестовий код відновлення.", user.Email);
            }

            return Ok(new
            {
                message = emailSent
                    ? "Код підтвердження надіслано на вашу електронну адресу"
                    : "Код сформовано. Використайте тестовий код підтвердження.",
                debugCode = resetCode,
                userEmail = user.Email
            });
        }

        [HttpPost("verify-reset-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyCodeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identifier = dto.Email.Trim().ToLowerInvariant();
            var trimmedCode = dto.Code.Trim();

            _logger.LogInformation("[Auth] Перевірка коду відновлення для: {Identifier}", identifier);

            var user = await _db.Users.FirstOrDefaultAsync(u =>
                (u.Email.ToLower() == identifier || u.Username.ToLower() == identifier) &&
                u.PasswordResetToken == trimmedCode &&
                u.PasswordResetTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
            {
                _logger.LogWarning("[Auth] Невірний або прострочений код відновлення для: {Identifier}", identifier);
                return BadRequest(new { message = "Невірний або прострочений код підтвердження." });
            }

            var secureResetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = secureResetToken;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
            await _db.SaveChangesAsync();

            _logger.LogInformation("[Auth] Код підтверджено для користувача: {Username}", user.Username);

            return Ok(new
            {
                message = "Код успішно підтверджено",
                resetToken = secureResetToken,
                userEmail = user.Email
            });
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trimmedToken = dto.Token.Trim();
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == trimmedToken &&
                u.PasswordResetTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
            {
                _logger.LogWarning("[Auth] Спроба оновлення пароля за недійсним або простроченим токеном.");
                return BadRequest(new { message = "Недійсний або прострочений токен відновлення. Надішліть новий запит." });
            }

            _passwordHasher.CreatePasswordHash(dto.NewPassword, out string hash, out string salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _jwtTokenService.RevokeUserTokensAsync(user.Id);
            await _db.SaveChangesAsync();

            _logger.LogInformation("[Auth] Пароль успішно змінено для користувача: {Username}", user.Username);

            return Ok(new { message = "Пароль успішно змінено. Тепер ви можете увійти з новим паролем." });
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await _db.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Status = UserStatus.Offline;
                }
                await _jwtTokenService.RevokeUserTokensAsync(userId);
            }

            return Ok(new { message = "Success logout" });
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var (success, error) = await _accountService.ChangePasswordAsync(userId, dto);
            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return Ok(new { message = "Пароль успішно змінено. Усі активні сесії оновлено." });
        }
    }
}
