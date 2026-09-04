using System.Globalization;
using System.Text;
using DteamBackend.Configuration;
using DteamBackend.Data;
using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.DTO.Chat;
using DteamBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DteamBackend.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;
        private readonly IChatFileStorage _fileStorage;
        private readonly IChatRealtimeNotifier _notifier;
        private readonly ChatOptions _options;
        private readonly ILogger<ChatService> _logger;

        private static readonly CultureInfo UkCulture = new("uk-UA");

        public ChatService(
            AppDbContext context,
            IChatFileStorage fileStorage,
            IChatRealtimeNotifier notifier,
            IOptions<ChatOptions> options,
            ILogger<ChatService> logger)
        {
            _context = context;
            _fileStorage = fileStorage;
            _notifier = notifier;
            _options = options.Value ?? new ChatOptions();
            _logger = logger;
        }

        public async Task<ChatMessageDto> SendMessageAsync(Guid currentUserId, SendMessageRequestDto dto, CancellationToken cancellationToken = default)
        {
            if (currentUserId == dto.ReceiverId)
            {
                throw new InvalidOperationException("You cannot send messages to yourself.");
            }

            var receiverExists = await _context.Users.AnyAsync(u => u.Id == dto.ReceiverId, cancellationToken);
            if (!receiverExists)
            {
                throw new KeyNotFoundException("Receiver user not found.");
            }

            var clientMessageId = string.IsNullOrWhiteSpace(dto.ClientMessageId)
                ? Guid.NewGuid().ToString("N")
                : dto.ClientMessageId.Trim();

            // 1. Idempotency check: if message with this (SenderId, ClientMessageId) already exists, return it
            var existingMessage = await _context.ChatMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SenderId == currentUserId && m.ClientMessageId == clientMessageId, cancellationToken);

            if (existingMessage != null)
            {
                _logger.LogInformation("[ChatService] Idempotent hit: message {ClientMessageId} already exists as {MessageId}", clientMessageId, existingMessage.Id);
                return MapToDto(existingMessage);
            }

            ChatUpload? upload = null;
            if (dto.UploadId.HasValue)
            {
                upload = await _context.ChatUploads
                    .FirstOrDefaultAsync(u => u.Id == dto.UploadId.Value, cancellationToken);

                if (upload == null || upload.UserId != currentUserId || upload.IsCommitted)
                {
                    throw new InvalidOperationException("Invalid or already committed upload.");
                }
            }

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ClientMessageId = clientMessageId,
                SenderId = currentUserId,
                ReceiverId = dto.ReceiverId,
                Content = dto.Content?.Trim(),
                Type = dto.Type,
                Status = MessageDeliveryStatus.Sent,
                StorageKey = upload?.StorageKey,
                OriginalFileName = upload?.OriginalFileName,
                ContentType = upload?.ContentType,
                FileSize = upload?.FileSize,
                Duration = upload?.Duration,
                CreatedAt = DateTimeOffset.UtcNow,
                IsDeletedForSender = false,
                IsDeletedForReceiver = false
            };

            if (upload != null)
            {
                upload.IsCommitted = true;
                upload.MessageId = message.Id;
            }

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            var messageDto = MapToDto(message);

            // Notify receiver & sender in real time
            await _notifier.NotifyMessageReceivedAsync(dto.ReceiverId, messageDto, cancellationToken);
            await _notifier.NotifyMessageReceivedAsync(currentUserId, messageDto, cancellationToken);

            return messageDto;
        }

        public async Task<CursorHistoryResponseDto> GetHistoryAsync(Guid currentUserId, Guid friendId, string? cursor, int limit = 50, CancellationToken cancellationToken = default)
        {
            if (limit <= 0 || limit > 100) limit = 50;

            var baseQuery = _context.ChatMessages
                .AsNoTracking()
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == friendId && !m.IsDeletedForSender) ||
                            (m.SenderId == friendId && m.ReceiverId == currentUserId && !m.IsDeletedForReceiver));

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            DateTimeOffset? cursorTime = null;
            Guid? cursorId = null;

            if (!string.IsNullOrWhiteSpace(cursor))
            {
                TryParseCursor(cursor, out cursorTime, out cursorId);
            }

            var query = baseQuery;
            if (cursorTime.HasValue && cursorId.HasValue)
            {
                var ct = cursorTime.Value;
                var cid = cursorId.Value;
                query = query.Where(m => m.CreatedAt < ct || (m.CreatedAt == ct && m.Id.CompareTo(cid) < 0));
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Take(limit + 1)
                .ToListAsync(cancellationToken);

            bool hasMore = messages.Count > limit;
            var pageItems = messages.Take(limit).ToList();

            string? nextCursor = null;
            if (hasMore && pageItems.Count > 0)
            {
                var lastItem = pageItems.Last();
                nextCursor = EncodeCursor(lastItem.CreatedAt, lastItem.Id);
            }

            // Return items in chronological order for clean client rendering
            pageItems.Reverse();

            return new CursorHistoryResponseDto
            {
                Items = pageItems.Select(MapToDto).ToList(),
                NextCursor = nextCursor,
                HasMore = hasMore,
                TotalCount = totalCount
            };
        }

        public async Task<List<ChatMessageDto>> GetMissedMessagesAsync(Guid currentUserId, Guid friendId, DateTimeOffset afterTimestamp, CancellationToken cancellationToken = default)
        {
            var messages = await _context.ChatMessages
                .AsNoTracking()
                .Where(m => ((m.SenderId == currentUserId && m.ReceiverId == friendId && !m.IsDeletedForSender) ||
                             (m.SenderId == friendId && m.ReceiverId == currentUserId && !m.IsDeletedForReceiver)) &&
                            m.CreatedAt > afterTimestamp)
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.Id)
                .Take(100)
                .ToListAsync(cancellationToken);

            return messages.Select(MapToDto).ToList();
        }

        public async Task<List<ChatConversationDto>> GetConversationsAsync(Guid currentUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get all accepted friend IDs
                var friendIds = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == currentUserId)
                    .SelectMany(u => u.Friends.Select(f => f.Id))
                    .ToListAsync(cancellationToken);

                // Also find users with whom we have existing messages
                var otherUserIdsWithMessages = await _context.ChatMessages
                    .AsNoTracking()
                    .Where(m => (m.SenderId == currentUserId && !m.IsDeletedForSender) ||
                                (m.ReceiverId == currentUserId && !m.IsDeletedForReceiver))
                    .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var allInteractedUserIds = friendIds.Union(otherUserIdsWithMessages).Where(id => id != currentUserId).Distinct().ToList();

                if (allInteractedUserIds.Count == 0)
                {
                    var demoUsers = await _context.Users
                        .Where(u => u.Id != currentUserId && (
                            u.Username == "MrsZubarikessa" || 
                            u.Username == "FirePhoenix" || 
                            u.Username == "DragonSlayer" || 
                            u.Username == "TitanCrusher" || 
                            u.Username == "sinichka_bez_egg" || 
                            u.Username == "SilentAssassin" || 
                            u.Username == "LunarMage" || 
                            u.Username == "BlazingArrow"))
                        .ToListAsync(cancellationToken);

                    if (demoUsers.Count == 0)
                    {
                        PasswordHasher.CreatePasswordHash("admin123321", out string ph, out string ps);
                        var demoList = new (string email, string username, string avatar, UserStatus status)[]
                        {
                            ("mrszubarikessa@dteam.io", "MrsZubarikessa", "https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                            ("firephoenix@dteam.io", "FirePhoenix", "https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                            ("dragonslayer@dteam.io", "DragonSlayer", "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=500&auto=format&fit=crop&q=80", UserStatus.Offline),
                            ("titancrusher@dteam.io", "TitanCrusher", "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                            ("blazingarrow@dteam.io", "BlazingArrow", "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=500&auto=format&fit=crop&q=80", UserStatus.Offline),
                            ("sinichka@dteam.io", "sinichka_bez_egg", "https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                            ("silentassassin@dteam.io", "SilentAssassin", "https://images.unsplash.com/photo-1527980965255-d3b416303d12?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                            ("lunarmage@dteam.io", "LunarMage", "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=500&auto=format&fit=crop&q=80", UserStatus.Offline)
                        };

                        foreach (var item in demoList)
                        {
                            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == item.email || u.Username == item.username, cancellationToken);
                            if (existing == null)
                            {
                                existing = new Duser
                                {
                                    Id = Guid.NewGuid(),
                                    Email = item.email,
                                    Username = item.username,
                                    PasswordHash = ph,
                                    PasswordSalt = ps,
                                    WalletAddress = $"EQB_{item.username.ToLowerInvariant()}_wallet_addr",
                                    BalanceInNanoTons = 10_000_000_000,
                                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                                    Status = item.status,
                                    AvatarUrl = item.avatar,
                                    Bio = $"Gaming enthusiast — {item.username}"
                                };
                                await _context.Users.AddAsync(existing, cancellationToken);
                            }
                            demoUsers.Add(existing);
                        }
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    if (demoUsers.Count > 0)
                    {
                        foreach (var du in demoUsers)
                        {
                            if (!await _context.UserFriends.AnyAsync(uf => uf.UserId == currentUserId && uf.FriendId == du.Id, cancellationToken))
                            {
                                _context.UserFriends.Add(new UserFriend
                                {
                                    UserId = currentUserId,
                                    FriendId = du.Id,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                            if (!await _context.UserFriends.AnyAsync(uf => uf.UserId == du.Id && uf.FriendId == currentUserId, cancellationToken))
                            {
                                _context.UserFriends.Add(new UserFriend
                                {
                                    UserId = du.Id,
                                    FriendId = currentUserId,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }

                        var mainFriend = demoUsers.FirstOrDefault(u => u.Username == "MrsZubarikessa") ?? demoUsers[0];

                        // Seed sample messages with MrsZubarikessa if none exist
                        bool hasAnyMsg = await _context.ChatMessages.AnyAsync(m => (m.SenderId == currentUserId && m.ReceiverId == mainFriend.Id) || (m.SenderId == mainFriend.Id && m.ReceiverId == currentUserId), cancellationToken);
                        if (!hasAnyMsg)
                        {
                            _context.ChatMessages.AddRange(new List<ChatMessage>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ClientMessageId = Guid.NewGuid().ToString("N"),
                                    SenderId = mainFriend.Id,
                                    ReceiverId = currentUserId,
                                    Content = "Привіт! Як справи?",
                                    Type = ChatMessageType.Text,
                                    Status = MessageDeliveryStatus.Read,
                                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30),
                                    ReadAt = DateTimeOffset.UtcNow.AddMinutes(-25)
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ClientMessageId = Guid.NewGuid().ToString("N"),
                                    SenderId = mainFriend.Id,
                                    ReceiverId = currentUserId,
                                    Content = "Я чув про гарне місце неподалік нашого міста. Там є озеро і ліс. Що думаєш?",
                                    Type = ChatMessageType.Text,
                                    Status = MessageDeliveryStatus.Read,
                                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-20),
                                    ReadAt = DateTimeOffset.UtcNow.AddMinutes(-18)
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ClientMessageId = Guid.NewGuid().ToString("N"),
                                    SenderId = mainFriend.Id,
                                    ReceiverId = currentUserId,
                                    Content = "Це ідея! Я візьму на себе напої. Коли тобі зручно виїхати?",
                                    Type = ChatMessageType.Text,
                                    Status = MessageDeliveryStatus.Read,
                                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-15),
                                    ReadAt = DateTimeOffset.UtcNow.AddMinutes(-14)
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ClientMessageId = Guid.NewGuid().ToString("N"),
                                    SenderId = currentUserId,
                                    ReceiverId = mainFriend.Id,
                                    Content = "Нещодавно почав працювати над новим проєктом на роботі. А в тебе які новини?",
                                    Type = ChatMessageType.Text,
                                    Status = MessageDeliveryStatus.Read,
                                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                                    ReadAt = DateTimeOffset.UtcNow.AddMinutes(-3)
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ClientMessageId = Guid.NewGuid().ToString("N"),
                                    SenderId = mainFriend.Id,
                                    ReceiverId = currentUserId,
                                    Content = "Йо, ти де пропав? Давно тебе не було видно.",
                                    Type = ChatMessageType.Text,
                                    Status = MessageDeliveryStatus.Sent,
                                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
                                }
                            });
                        }

                        await _context.SaveChangesAsync(cancellationToken);
                        allInteractedUserIds = demoUsers.Select(u => u.Id).ToList();
                    }
                }

                var usersMap = await _context.Users
                    .AsNoTracking()
                    .Where(u => allInteractedUserIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, cancellationToken);

                var conversations = new List<ChatConversationDto>();

                foreach (var peerId in allInteractedUserIds)
                {
                    if (!usersMap.TryGetValue(peerId, out var peerUser)) continue;

                var lastMessage = await _context.ChatMessages
                    .AsNoTracking()
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == peerId && !m.IsDeletedForSender) ||
                                (m.SenderId == peerId && m.ReceiverId == currentUserId && !m.IsDeletedForReceiver))
                    .OrderByDescending(m => m.CreatedAt)
                    .ThenByDescending(m => m.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                var unreadCount = await _context.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.SenderId == peerId && m.ReceiverId == currentUserId && m.Status != MessageDeliveryStatus.Read && !m.IsDeletedForReceiver)
                    .CountAsync(cancellationToken);

                conversations.Add(new ChatConversationDto
                {
                    FriendId = peerUser.Id,
                    FriendUsername = peerUser.Username,
                    FriendAvatarUrl = peerUser.AvatarUrl,
                    FriendStatus = peerUser.Status,
                    LastMessage = lastMessage != null ? MapToDto(lastMessage) : null,
                    UnreadCount = unreadCount,
                    LastActivityAt = lastMessage?.CreatedAt
                });
            }

            return conversations
                .OrderByDescending(c => c.UnreadCount > 0)
                .ThenByDescending(c => c.LastActivityAt ?? DateTimeOffset.MinValue)
                .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChatService] Error getting conversations for user {UserId}", currentUserId);
                return new List<ChatConversationDto>();
            }
        }

        public async Task<bool> MarkAsReadAsync(Guid currentUserId, Guid messageId, CancellationToken cancellationToken = default)
        {
            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == currentUserId, cancellationToken);

            if (message == null || message.Status == MessageDeliveryStatus.Read)
            {
                return false;
            }

            message.Status = MessageDeliveryStatus.Read;
            message.ReadAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _notifier.NotifyMessageReadAsync(message.SenderId, message.Id, currentUserId, message.ReadAt.Value, cancellationToken);
            await _notifier.NotifyMessageReadAsync(currentUserId, message.Id, currentUserId, message.ReadAt.Value, cancellationToken);

            return true;
        }

        public async Task<bool> ClearHistoryAsync(Guid currentUserId, Guid friendId, CancellationToken cancellationToken = default)
        {
            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == friendId) ||
                            (m.SenderId == friendId && m.ReceiverId == currentUserId))
                .ToListAsync(cancellationToken);

            if (messages.Count == 0)
            {
                return true;
            }

            foreach (var msg in messages)
            {
                msg.IsDeletedForSender = true;
                msg.IsDeletedForReceiver = true;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Real-time SignalR notification for BOTH participants
            await _notifier.NotifyHistoryClearedAsync(friendId, currentUserId, cancellationToken);
            await _notifier.NotifyHistoryClearedAsync(currentUserId, friendId, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteMessageAsync(Guid currentUserId, Guid messageId, CancellationToken cancellationToken = default)
        {
            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

            if (message == null)
            {
                return false;
            }

            // Only the sender (author of the message) can delete it
            if (message.SenderId != currentUserId)
            {
                return false;
            }

            message.IsDeletedForSender = true;
            message.IsDeletedForReceiver = true;

            if (!string.IsNullOrEmpty(message.StorageKey))
            {
                try
                {
                    await _fileStorage.DeleteFileAsync(message.StorageKey, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[ChatService] Could not delete physical file for message {MessageId}", messageId);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Real-time SignalR notification for BOTH participants
            await _notifier.NotifyMessageDeletedAsync(message.ReceiverId, messageId, message.SenderId, cancellationToken);
            await _notifier.NotifyMessageDeletedAsync(message.SenderId, messageId, message.ReceiverId, cancellationToken);

            return true;
        }

        public async Task<ChatMediaSummaryDto> GetMediaSummaryAsync(Guid currentUserId, Guid friendId, CancellationToken cancellationToken = default)
        {
            var messages = await _context.ChatMessages
                .AsNoTracking()
                .Where(m => ((m.SenderId == currentUserId && m.ReceiverId == friendId && !m.IsDeletedForSender) ||
                             (m.SenderId == friendId && m.ReceiverId == currentUserId && !m.IsDeletedForReceiver)) &&
                            m.Type != ChatMessageType.Text &&
                            !string.IsNullOrEmpty(m.StorageKey))
                .OrderByDescending(m => m.CreatedAt)
                .Include(m => m.Sender)
                .ToListAsync(cancellationToken);

            var photos = messages.Where(m => m.Type == ChatMessageType.Image).ToList();
            var files = messages.Where(m => m.Type == ChatMessageType.File).ToList();
            var voice = messages.Where(m => m.Type == ChatMessageType.Voice).ToList();

            return new ChatMediaSummaryDto
            {
                TotalPhotos = photos.Count,
                TotalFiles = files.Count,
                TotalVoiceMessages = voice.Count,
                PhotosByMonth = GroupByMonth(photos),
                FilesByMonth = GroupByMonth(files),
                VoiceByMonth = GroupByMonth(voice)
            };
        }

        public async Task<ChatUploadResponseDto> UploadFileAsync(Guid currentUserId, IFormFile file, int? duration, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var contentType = file.ContentType ?? "application/octet-stream";

            // Size checks
            if (file.Length > _options.MaxFileSizeBytes)
            {
                throw new InvalidOperationException($"File exceeds maximum allowed size of {_options.MaxFileSizeBytes / (1024 * 1024)} MB.");
            }

            await using var stream = file.OpenReadStream();

            var storageKey = await _fileStorage.SaveFileAsync(stream, file.FileName, contentType, cancellationToken);

            var upload = new ChatUpload
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                StorageKey = storageKey,
                OriginalFileName = file.FileName,
                ContentType = contentType,
                FileSize = file.Length,
                Duration = duration,
                CreatedAt = DateTimeOffset.UtcNow,
                IsCommitted = false
            };

            _context.ChatUploads.Add(upload);
            await _context.SaveChangesAsync(cancellationToken);

            return new ChatUploadResponseDto
            {
                UploadId = upload.Id,
                OriginalFileName = upload.OriginalFileName,
                ContentType = upload.ContentType,
                FileSize = upload.FileSize,
                Duration = upload.Duration,
                PreviewUrl = $"/api/chat/uploads/{upload.Id}/preview"
            };
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> GetMediaContentAsync(Guid currentUserId, Guid messageId, CancellationToken cancellationToken = default)
        {
            var message = await _context.ChatMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

            if (message == null || string.IsNullOrEmpty(message.StorageKey))
            {
                return null;
            }

            // Authorization & soft-delete validation: only authenticated participants can access media
            if (currentUserId == Guid.Empty)
            {
                return null;
            }

            if (message.SenderId == currentUserId)
            {
                if (message.IsDeletedForSender) return null;
            }
            else if (message.ReceiverId == currentUserId)
            {
                if (message.IsDeletedForReceiver) return null;
            }
            else
            {
                return null; // Not a participant
            }

            var fileResult = await _fileStorage.GetFileStreamAsync(message.StorageKey, cancellationToken);
            if (!fileResult.HasValue)
            {
                return null;
            }

            var fileName = !string.IsNullOrWhiteSpace(message.OriginalFileName)
                ? message.OriginalFileName
                : $"file_{message.Id:N}{Path.GetExtension(message.StorageKey)}";

            var contentType = !string.IsNullOrWhiteSpace(message.ContentType)
                ? message.ContentType
                : fileResult.Value.ContentType;

            return (fileResult.Value.Stream, contentType, fileName);
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> GetUploadPreviewAsync(Guid currentUserId, Guid uploadId, CancellationToken cancellationToken = default)
        {
            if (currentUserId == Guid.Empty)
            {
                return null;
            }

            var upload = await _context.ChatUploads
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == uploadId, cancellationToken);

            if (upload == null || string.IsNullOrEmpty(upload.StorageKey))
            {
                return null;
            }

            if (upload.UserId != currentUserId)
            {
                return null;
            }

            var fileResult = await _fileStorage.GetFileStreamAsync(upload.StorageKey, cancellationToken);
            if (!fileResult.HasValue)
            {
                return null;
            }

            var fileName = !string.IsNullOrWhiteSpace(upload.OriginalFileName)
                ? upload.OriginalFileName
                : $"upload_{upload.Id:N}{Path.GetExtension(upload.StorageKey)}";

            var contentType = !string.IsNullOrWhiteSpace(upload.ContentType)
                ? upload.ContentType
                : fileResult.Value.ContentType;

            return (fileResult.Value.Stream, contentType, fileName);
        }

        public async Task SendTypingAsync(Guid currentUserId, Guid receiverId, bool isTyping, CancellationToken cancellationToken = default)
        {
            if (currentUserId == receiverId) return;
            await _notifier.NotifyUserTypingAsync(receiverId, currentUserId, isTyping, cancellationToken);
        }

        private static ChatMessageDto MapToDto(ChatMessage m)
        {
            return new ChatMessageDto
            {
                Id = m.Id,
                ClientMessageId = m.ClientMessageId,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                Type = m.Type,
                Status = m.Status,
                MediaUrl = !string.IsNullOrEmpty(m.StorageKey) ? $"/api/chat/media/{m.Id}/content" : null,
                OriginalFileName = m.OriginalFileName,
                ContentType = m.ContentType,
                FileSize = m.FileSize,
                Duration = m.Duration,
                CreatedAt = m.CreatedAt,
                ReadAt = m.ReadAt
            };
        }

        private static List<ChatMonthGroupDto> GroupByMonth(List<ChatMessage> messages)
        {
            return messages
                .GroupBy(m => new { m.CreatedAt.Year, m.CreatedAt.Month })
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month)
                .Select(g =>
                {
                    var firstDate = new DateTime(g.Key.Year, g.Key.Month, 1);
                    var monthName = UkCulture.DateTimeFormat.GetMonthName(g.Key.Month);
                    var label = $"{char.ToUpper(monthName[0])}{monthName[1..]} {g.Key.Year}";

                    return new ChatMonthGroupDto
                    {
                        MonthLabel = label,
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Items = g.Select(m => new ChatMediaItemDto
                        {
                            MessageId = m.Id,
                            SenderId = m.SenderId,
                            SenderUsername = m.Sender?.Username ?? "User",
                            MediaUrl = $"/api/chat/media/{m.Id}/content",
                            OriginalFileName = m.OriginalFileName ?? "file",
                            ContentType = m.ContentType ?? "application/octet-stream",
                            FileSize = m.FileSize ?? 0,
                            Duration = m.Duration,
                            CreatedAt = m.CreatedAt
                        }).ToList()
                    };
                })
                .ToList();
        }

        private static string EncodeCursor(DateTimeOffset createdAt, Guid id)
        {
            var raw = $"{createdAt.ToUnixTimeMilliseconds()}:{id:N}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        }

        private static bool TryParseCursor(string cursor, out DateTimeOffset? createdAt, out Guid? id)
        {
            createdAt = null;
            id = null;
            try
            {
                var bytes = Convert.FromBase64String(cursor);
                var raw = Encoding.UTF8.GetString(bytes);
                var parts = raw.Split(':');
                if (parts.Length == 2 && long.TryParse(parts[0], out var unixMs) && Guid.TryParse(parts[1], out var parsedId))
                {
                    createdAt = DateTimeOffset.FromUnixTimeMilliseconds(unixMs);
                    id = parsedId;
                    return true;
                }
            }
            catch
            {
                // Invalid cursor
            }
            return false;
        }
    }
}
