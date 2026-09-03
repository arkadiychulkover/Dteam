using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DteamBackend.Interfaces;
using DteamBackend.Models.DTO.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IHardhatTokenService _tokenService;
        private readonly ILogger<TokenController> _logger;

        public TokenController(IHardhatTokenService tokenService, ILogger<TokenController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Проверка соответствия текущего кошелька с фронтенда с тем, что был привязан в БД при регистрации.
        /// Принимает строку публичного ключа (кошелька) и опционально UserId (или берет из JWT-токена).
        /// </summary>
        [HttpPost("verify-wallet")]
        public async Task<ActionResult<VerifyWalletResponseDto>> VerifyWallet([FromBody] VerifyWalletDto dto)
        {
            try
            {
                Guid userId;
                if (dto.UserId.HasValue && dto.UserId.Value != Guid.Empty)
                {
                    userId = dto.UserId.Value;
                }
                else
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
                    {
                        return BadRequest(new { message = "Необходимо указать UserId или передать валидный JWT токен авторизации." });
                    }
                }

                var (isMatch, registeredAddress, providedAddress) = await _tokenService.VerifyWalletMatchAsync(userId, dto.WalletAddress);

                return Ok(new VerifyWalletResponseDto
                {
                    IsMatch = isMatch,
                    RegisteredAddress = registeredAddress,
                    ProvidedAddress = providedAddress,
                    Message = isMatch
                        ? "Кошелек совпадает с зарегистрированным в профиле."
                        : string.IsNullOrEmpty(registeredAddress)
                            ? "У данного пользователя не привязан Hardhat/MetaMask кошелек."
                            : "Кошелек НЕ совпадает с зарегистрированным в профиле!"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying wallet match");
                return StatusCode(500, new { message = "Внутренняя ошибка при проверке кошелька." });
            }
        }

        /// <summary>
        /// Начисление токенов пользователю (по UserId или адресу кошелька)
        /// </summary>
        [HttpPost("award")]
        public async Task<ActionResult<TokenOperationResponseDto>> AwardTokens([FromBody] TokenOperationDto dto)
        {
            try
            {
                string txHash;
                string target;

                if (dto.UserId.HasValue && dto.UserId.Value != Guid.Empty)
                {
                    txHash = await _tokenService.AwardTokensAsync(dto.UserId.Value, dto.Amount);
                    target = $"User ID {dto.UserId.Value}";
                }
                else if (!string.IsNullOrWhiteSpace(dto.WalletAddress))
                {
                    txHash = await _tokenService.AwardTokensByAddressAsync(dto.WalletAddress.Trim(), dto.Amount);
                    target = dto.WalletAddress.Trim();
                }
                else
                {
                    return BadRequest(new { message = "Укажите UserId или WalletAddress для начисления." });
                }

                return Ok(new TokenOperationResponseDto
                {
                    Success = true,
                    TransactionHash = txHash,
                    Amount = dto.Amount,
                    TargetAddress = target,
                    Message = $"Успешно начислено {dto.Amount} DTP."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding tokens");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Снятие / сжигание токенов у пользователя (по UserId или адресу кошелька)
        /// </summary>
        [HttpPost("debit")]
        public async Task<ActionResult<TokenOperationResponseDto>> DebitTokens([FromBody] TokenOperationDto dto)
        {
            try
            {
                string txHash;
                string target;

                if (dto.UserId.HasValue && dto.UserId.Value != Guid.Empty)
                {
                    txHash = await _tokenService.DebitTokensAsync(dto.UserId.Value, dto.Amount);
                    target = $"User ID {dto.UserId.Value}";
                }
                else if (!string.IsNullOrWhiteSpace(dto.WalletAddress))
                {
                    txHash = await _tokenService.DebitTokensByAddressAsync(dto.WalletAddress.Trim(), dto.Amount);
                    target = dto.WalletAddress.Trim();
                }
                else
                {
                    return BadRequest(new { message = "Укажите UserId или WalletAddress для списания." });
                }

                return Ok(new TokenOperationResponseDto
                {
                    Success = true,
                    TransactionHash = txHash,
                    Amount = dto.Amount,
                    TargetAddress = target,
                    Message = $"Успешно списано {dto.Amount} DTP."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debiting tokens");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получение баланса токенов кошелька
        /// </summary>
        [HttpGet("balance/{address}")]
        public async Task<ActionResult> GetBalance(string address)
        {
            try
            {
                var balance = await _tokenService.GetBalanceAsync(address);
                return Ok(new { address = address, balance = balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching balance for {Address}", address);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Обновление адреса администратора в БД из appsettings.json
        /// </summary>
        [HttpPost("admin/sync-address")]
        public async Task<ActionResult> SyncAdminAddress()
        {
            try
            {
                var updatedAddress = await _tokenService.UpdateAdminAddressFromSettingsAsync();
                return Ok(new
                {
                    success = true,
                    adminAddress = updatedAddress,
                    message = "Адрес администратора успешно синхронизирован из appsettings.json."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing admin address from settings");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
