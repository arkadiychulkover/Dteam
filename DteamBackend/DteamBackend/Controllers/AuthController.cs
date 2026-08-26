using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.DTO.Auth;
using DteamBackend.Models.Enums;
using DteamBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;

        public AuthController(
            AppDbContext db,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IEmailService emailService)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
        }

        [HttpPost("register")]
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

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                User = UserDto.FromEntity(user)
            });
        }

        [HttpPost("login")]
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

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                User = UserDto.FromEntity(user)
            });
        }

        [Authorize]
        [HttpGet("me")]
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
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
            {
                return Ok(new { message = "If the email exists, instructions have been sent" });
            }

            var resetCode = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetCode;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
            await _db.SaveChangesAsync();

            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetCode);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to send email. Check SMTP configuration." });
            }

            return Ok(new
            {
                message = "Код подтверждения отправлен на ваш Email"
            });
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyCodeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null || user.PasswordResetToken != dto.Code.Trim() || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Incorrect code" });
            }

            var secureResetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = secureResetToken; 
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(10);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Code was verified", resetToken = secureResetToken });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => 
                u.PasswordResetToken == dto.Token && 
                u.PasswordResetTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
            {
                return BadRequest(new { message = "Incorrect or time existed page" });
            }

            _passwordHasher.CreatePasswordHash(dto.NewPassword, out string hash, out string salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Password successfully changed" });
        }

        [Authorize]
        [HttpPost("logout")]
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
                    await _db.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Success logout" });
        }
    }
}
