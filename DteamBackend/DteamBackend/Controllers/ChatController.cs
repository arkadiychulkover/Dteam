using System.Security.Claims;
using DteamBackend.Interfaces;
using DteamBackend.Models.DTO.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations(CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("[ChatController] GetConversations called. currentUserId: {UserId}", currentUserId);
            if (currentUserId == Guid.Empty) return Unauthorized();

            var conversations = await _chatService.GetConversationsAsync(currentUserId, cancellationToken);
            _logger.LogInformation("[ChatController] Found {Count} conversations for user {UserId}", conversations.Count, currentUserId);
            return Ok(conversations);
        }

        [HttpGet("history/{friendId:guid}")]
        public async Task<IActionResult> GetHistory(Guid friendId, [FromQuery] string? before, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            var history = await _chatService.GetHistoryAsync(currentUserId, friendId, before, limit, cancellationToken);
            return Ok(history);
        }

        [HttpGet("sync/{friendId:guid}")]
        public async Task<IActionResult> GetMissedMessages(Guid friendId, [FromQuery] DateTimeOffset after, CancellationToken cancellationToken = default)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            var missed = await _chatService.GetMissedMessagesAsync(currentUserId, friendId, after, cancellationToken);
            return Ok(missed);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto dto, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            try
            {
                var message = await _chatService.SendMessageAsync(currentUserId, dto, cancellationToken);
                return Ok(message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("messages/{messageId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid messageId, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            var success = await _chatService.MarkAsReadAsync(currentUserId, messageId, cancellationToken);
            return Ok(new { success });
        }

        [HttpDelete("history/{friendId:guid}")]
        public async Task<IActionResult> ClearHistory(Guid friendId, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            var success = await _chatService.ClearHistoryAsync(currentUserId, friendId, cancellationToken);
            return Ok(new { success });
        }

        [HttpGet("media/{friendId:guid}")]
        public async Task<IActionResult> GetMediaSummary(Guid friendId, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            var summary = await _chatService.GetMediaSummaryAsync(currentUserId, friendId, cancellationToken);
            return Ok(summary);
        }

        [HttpPost("upload")]
        [RequestSizeLimit(200 * 1024 * 1024)] // 200MB max
        public async Task<IActionResult> UploadAttachment([FromForm] IFormFile file, [FromForm] int? duration, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            try
            {
                var result = await _chatService.UploadFileAsync(currentUserId, file, duration, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("media/{messageId:guid}/content")]
        public async Task<IActionResult> GetMediaContent(Guid messageId, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            var contentResult = await _chatService.GetMediaContentAsync(currentUserId, messageId, cancellationToken);
            if (!contentResult.HasValue)
            {
                return NotFound(new { message = "Attachment not found or access denied." });
            }

            var (stream, contentType, fileName) = contentResult.Value;

            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");
            return File(stream, contentType);
        }

        [HttpPost("typing")]
        public async Task<IActionResult> Typing([FromBody] TypingRequestDto dto, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            await _chatService.SendTypingAsync(currentUserId, dto.ReceiverId, dto.IsTyping, cancellationToken);
            return Ok(new { success = true });
        }

        [HttpPost("seed-demo")]
        public async Task<IActionResult> SeedDemo(CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return Unauthorized();

            var convs = await _chatService.GetConversationsAsync(currentUserId, cancellationToken);
            return Ok(convs);
        }
    }

    public class TypingRequestDto
    {
        public Guid ReceiverId { get; set; }
        public bool IsTyping { get; set; }
    }
}
