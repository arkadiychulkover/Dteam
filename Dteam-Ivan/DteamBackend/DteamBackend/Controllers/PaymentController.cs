using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TonService _tonService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            AppDbContext context,
            TonService tonService,
            IConfiguration configuration,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _tonService = tonService;
            _configuration = configuration;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [AllowAnonymous]
        [HttpGet("deposit-address")]
        [HttpGet("wallet-address")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> GetDepositAddress()
        {
            var address = _configuration["Ton:Address"] ?? string.Empty;
            return Ok(new { address = address });
        }

        [HttpPost("verify")]
        [HttpPost("validate")]
        [HttpPost("deposit")]
        [ProducesResponseType(typeof(PaymentVerificationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaymentVerificationResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentVerificationResultDto>> VerifyTransaction([FromBody] VerifyTransactionDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            if (user.IsBanned)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Ваш акаунт заблоковано." });
            }

            var cleanHash = dto.TxhHash.Trim();

            var alreadyProcessed = await _context.Tranxactions
                .AnyAsync(t => t.TxhHash.ToLower() == cleanHash.ToLower());

            if (alreadyProcessed)
            {
                _logger.LogWarning($"[PaymentController] Transaction {cleanHash} already exists in DB for User {userId}");
                return BadRequest(new PaymentVerificationResultDto
                {
                    Success = false,
                    Message = "Ця транзакція вже була використана і зарахована раніше.",
                    NewBalanceInNanoTons = user.BalanceInNanoTons,
                    Amount = dto.Amount,
                    TxhHash = cleanHash
                });
            }

            var isValid = await _tonService.CheckTranzaction(cleanHash, dto.Amount);
            if (!isValid)
            {
                _logger.LogWarning($"[PaymentController] Transaction {cleanHash} validation failed via TonService");
                return BadRequest(new PaymentVerificationResultDto
                {
                    Success = false,
                    Message = "Транзакцію не знайдено в мережі TON, сума не відповідає вимогам або термін дії транзакції вичерпано (понад 10 хв).",
                    NewBalanceInNanoTons = user.BalanceInNanoTons,
                    Amount = dto.Amount,
                    TxhHash = cleanHash
                });
            }

            var raceCheck = await _context.Tranxactions
                .AnyAsync(t => t.TxhHash.ToLower() == cleanHash.ToLower());

            if (raceCheck)
            {
                return BadRequest(new PaymentVerificationResultDto
                {
                    Success = false,
                    Message = "Транзакція вже була оброблена паралельним запитом.",
                    NewBalanceInNanoTons = user.BalanceInNanoTons,
                    Amount = dto.Amount,
                    TxhHash = cleanHash
                });
            }

            var transactionRecord = new Tranxaction
            {
                Id = Guid.NewGuid(),
                TxhHash = cleanHash,
                Amount = dto.Amount,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Tranxactions.AddAsync(transactionRecord);

            long nanoTonsToAdd = (long)Math.Round(dto.Amount * 1_000_000_000m);
            user.BalanceInNanoTons += nanoTonsToAdd;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"[PaymentController] User {user.Id} ({user.Username}) deposited {dto.Amount} TON ({nanoTonsToAdd} nanoTONs). New balance: {user.BalanceInNanoTons} nanoTONs.");

            return Ok(new PaymentVerificationResultDto
            {
                Success = true,
                Message = $"Транзакцію успішно підтверджено! На ваш баланс зараховано {dto.Amount} TON.",
                NewBalanceInNanoTons = user.BalanceInNanoTons,
                Amount = dto.Amount,
                TxhHash = cleanHash
            });
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(List<TranxactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<TranxactionDto>>> GetUserTransactions()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var transactions = await _context.Tranxactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TranxactionDto
                {
                    Id = t.Id,
                    TxhHash = t.TxhHash,
                    Amount = t.Amount,
                    UserId = t.UserId,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(transactions);
        }
    }
}

