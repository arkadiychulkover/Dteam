using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using DteamBackend.Data;
using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace DteamBackend.Services
{
    public class NftService : INftService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<NftService> _logger;
        private readonly IConfiguration _configuration;
        private string? _contractAbi;

        public const string DefaultContractAddress = "0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0";

        public NftService(
            AppDbContext context,
            IWebHostEnvironment environment,
            ILogger<NftService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
        }

        private string GetContractAbi()
        {
            if (!string.IsNullOrEmpty(_contractAbi))
                return _contractAbi;

            var path = Path.Combine(_environment.ContentRootPath, "Contracts", "DteamNFT.json");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"ABI file not found at {path}");
            }

            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("abi", out var abiElement))
            {
                _contractAbi = abiElement.GetRawText();
            }
            else
            {
                _contractAbi = json;
            }

            return _contractAbi;
        }

        public async Task EnsureNftCollectionInitializedAsync()
        {
            try
            {
                // 1. Создаем таблицу NftItems в SQLite, если её ещё нет
                await _context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""NftItems"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_NftItems"" PRIMARY KEY,
                        ""TokenId"" INTEGER NULL,
                        ""Name"" TEXT NOT NULL,
                        ""Description"" TEXT NOT NULL DEFAULT '',
                        ""Rarity"" INTEGER NOT NULL DEFAULT 0,
                        ""ImageUrl"" TEXT NOT NULL,
                        ""BackgroundIndex"" INTEGER NOT NULL DEFAULT 0,
                        ""PatternIndex"" INTEGER NOT NULL DEFAULT 0,
                        ""ModelIndex"" INTEGER NOT NULL DEFAULT 0,
                        ""ContractAddress"" TEXT NOT NULL DEFAULT '',
                        ""OwnerAddress"" TEXT NULL,
                        ""FromAddress"" TEXT NULL,
                        ""UserId"" TEXT NULL,
                        ""IsMinted"" INTEGER NOT NULL DEFAULT 0,
                        ""PriceInPoints"" TEXT NOT NULL DEFAULT '100',
                        ""LastTransferredAt"" TEXT NULL,
                        ""CreatedAt"" TEXT NOT NULL
                    );
                ");

                // 2. Безопасное добавление новых колонок для существующей таблицы
                try { await _context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""NftItems"" ADD COLUMN ""UserId"" TEXT NULL;"); } catch { }
                try { await _context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""NftItems"" ADD COLUMN ""FromAddress"" TEXT NULL;"); } catch { }
                try { await _context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""NftItems"" ADD COLUMN ""LastTransferredAt"" TEXT NULL;"); } catch { }
                try { await _context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""NftItems"" ADD COLUMN ""GiftMessage"" TEXT NULL;"); } catch { }

                // 3. Индексы для таблицы NftItems
                try { await _context.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_NftItems_TokenId"" ON ""NftItems"" (""TokenId"");"); } catch { }
                try { await _context.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_NftItems_Rarity"" ON ""NftItems"" (""Rarity"");"); } catch { }
                try { await _context.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_NftItems_IsMinted"" ON ""NftItems"" (""IsMinted"");"); } catch { }
                try { await _context.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_NftItems_UserId"" ON ""NftItems"" (""UserId"");"); } catch { }
                try { await _context.Database.ExecuteSqlRawAsync(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_NftItems_Combo"" ON ""NftItems"" (""BackgroundIndex"", ""PatternIndex"", ""ModelIndex"");"); } catch { }

                // 4. Таблица NftTransfers и её индексы
                await _context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""NftTransfers"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_NftTransfers"" PRIMARY KEY,
                        ""NftItemId"" TEXT NOT NULL,
                        ""TokenId"" INTEGER NOT NULL,
                        ""FromAddress"" TEXT NOT NULL,
                        ""ToAddress"" TEXT NOT NULL,
                        ""FromUserId"" TEXT NULL,
                        ""ToUserId"" TEXT NULL,
                        ""TransactionHash"" TEXT NULL,
                        ""BlockNumber"" INTEGER NOT NULL DEFAULT 0,
                        ""TransferredAt"" TEXT NOT NULL
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_NftTransfers_NftItemId"" ON ""NftTransfers"" (""NftItemId"");
                    CREATE INDEX IF NOT EXISTS ""IX_NftTransfers_TokenId"" ON ""NftTransfers"" (""TokenId"");
                    CREATE INDEX IF NOT EXISTS ""IX_NftTransfers_FromAddress"" ON ""NftTransfers"" (""FromAddress"");
                    CREATE INDEX IF NOT EXISTS ""IX_NftTransfers_ToAddress"" ON ""NftTransfers"" (""ToAddress"");
                    CREATE INDEX IF NOT EXISTS ""IX_NftTransfers_ToUserId"" ON ""NftTransfers"" (""ToUserId"");
                ");

                // 5. Синхронизация TokenId для уже сминченных NFT с реальным блокчейн-токеном из NftTransfers
                try
                {
                    var targetContract = _configuration["Ethereum:NftContractAddress"] ?? DefaultContractAddress;
                    var oldContractItems = await _context.NftItems.AnyAsync(n => n.ContractAddress != targetContract);
                    if (oldContractItems)
                    {
                        _logger.LogInformation("[NftService] Contract address changed to {NewContract}. Resetting collection for new contract...", targetContract);
                        await _context.Database.ExecuteSqlRawAsync(@"
                            DELETE FROM ""NftTransfers"";
                            UPDATE ""NftItems"" 
                            SET ""IsMinted"" = 0, ""OwnerAddress"" = NULL, ""FromAddress"" = NULL, ""UserId"" = NULL, ""ContractAddress"" = {0};
                            UPDATE ""Users"" SET ""TimeRewardNftsMintedCount"" = 0;
                        ", targetContract);
                    }
                    else
                    {
                        await _context.Database.ExecuteSqlRawAsync(@"
                            UPDATE ""NftItems""
                            SET ""TokenId"" = (
                                SELECT t.""TokenId""
                                FROM ""NftTransfers"" t
                                WHERE t.""NftItemId"" = ""NftItems"".""Id""
                                ORDER BY t.""TransferredAt"" DESC
                                LIMIT 1
                            )
                            WHERE ""IsMinted"" = 1 AND EXISTS (
                                SELECT 1 FROM ""NftTransfers"" t WHERE t.""NftItemId"" = ""NftItems"".""Id""
                            );
                        ");
                    }
                }
                catch (Exception syncEx)
                {
                    _logger.LogWarning(syncEx, "[NftService] Could not sync NftItems TokenId from NftTransfers");
                }

                var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var nftOutputDir = Path.Combine(webRoot, "nft");

                if (!Directory.Exists(nftOutputDir))
                {
                    Directory.CreateDirectory(nftOutputDir);
                }

                var existingCount = await _context.NftItems.CountAsync();
                var pngFiles = Directory.GetFiles(nftOutputDir, "*.png");

                // Если папка пуста или в БД нет записей, запускаем генерацию и сидирование
                if (pngFiles.Length == 0)
                {
                    _logger.LogInformation("[NftService] NFT output directory is empty. Running image combination via nftCombiner.js...");

                    var layersBaseDir = Path.Combine(_environment.ContentRootPath, "Resources", "NftLayers");
                    var backgroundDir = Path.Combine(layersBaseDir, "backgrounds");
                    var patternDir = Path.Combine(layersBaseDir, "patterns");
                    var modelDir = Path.Combine(layersBaseDir, "models");
                    var scriptPath = Path.Combine(_environment.ContentRootPath, "Services", "jsServices", "nftCombiner.js");

                    if (!File.Exists(scriptPath))
                    {
                        _logger.LogError($"[NftService] Combiner script not found at {scriptPath}");
                        return;
                    }

                    if (!Directory.Exists(backgroundDir) || !Directory.Exists(patternDir) || !Directory.Exists(modelDir))
                    {
                        _logger.LogError("[NftService] Layers directories not found.");
                        return;
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = "node",
                        Arguments = $"\"{scriptPath}\" \"{backgroundDir}\" \"{patternDir}\" \"{modelDir}\" \"{nftOutputDir}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(scriptPath)
                    };

                    using var process = Process.Start(psi);
                    if (process != null)
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        var error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            _logger.LogError($"[NftService] nftCombiner.js failed with code {process.ExitCode}: {error}");
                            return;
                        }

                        _logger.LogInformation($"[NftService] nftCombiner.js output: {output}");
                    }

                    pngFiles = Directory.GetFiles(nftOutputDir, "*.png");
                }

                // Сидируем записи в базу данных, если их там ещё нет
                if (existingCount == 0 && pngFiles.Length > 0)
                {
                    _logger.LogInformation($"[NftService] Seeding {pngFiles.Length} NFT items to database...");

                    var contractAddr = _configuration["Ethereum:NftContractAddress"] ?? DefaultContractAddress;
                    var nftItems = new List<NftItem>();
                    int counter = 0;

                    foreach (var file in pngFiles.OrderBy(f => f))
                    {
                        var fileName = Path.GetFileName(file);
                        var nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                        var parts = nameWithoutExt.Split('_');

                        int bgIdx = 1, patIdx = 1, modIdx = 1;
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[0], out bgIdx);
                            int.TryParse(parts[1], out patIdx);
                            int.TryParse(parts[2], out modIdx);
                        }

                        var rarity = CalculateRarity(bgIdx, patIdx, modIdx);
                        var (name, desc) = GenerateMetadata(counter, bgIdx, patIdx, modIdx, rarity);
                        var price = GetBasePrice(rarity);

                        nftItems.Add(new NftItem
                        {
                            Id = Guid.NewGuid(),
                            TokenId = counter,
                            Name = name,
                            Description = desc,
                            Rarity = rarity,
                            ImageUrl = $"/nft/{fileName}",
                            BackgroundIndex = bgIdx,
                            PatternIndex = patIdx,
                            ModelIndex = modIdx,
                            ContractAddress = contractAddr,
                            IsMinted = false,
                            PriceInPoints = price,
                            CreatedAt = DateTime.UtcNow
                        });

                        counter++;
                    }

                    await _context.NftItems.AddRangeAsync(nftItems);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"[NftService] Successfully seeded {nftItems.Count} NFT items!");
                }
                else
                {
                    _logger.LogInformation($"[NftService] NFT collection is already initialized with {existingCount} items.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NftService] Error ensuring NFT collection initialized.");
            }
        }

        public async Task<List<NftItem>> GetAllNftsAsync(NftRarity? rarity = null, bool? isMinted = null)
        {
            var query = _context.NftItems.AsNoTracking().AsQueryable();

            if (rarity.HasValue)
            {
                query = query.Where(n => n.Rarity == rarity.Value);
            }

            if (isMinted.HasValue)
            {
                query = query.Where(n => n.IsMinted == isMinted.Value);
            }

            return await query.OrderBy(n => n.TokenId).ToListAsync();
        }

        public async Task<NftItem?> GetNftByIdAsync(Guid id)
        {
            var item = await _context.NftItems.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id);
            if (item != null && item.IsMinted)
            {
                var transfer = await _context.NftTransfers
                    .AsNoTracking()
                    .Where(t => t.NftItemId == id)
                    .OrderByDescending(t => t.TransferredAt)
                    .FirstOrDefaultAsync();

                if (transfer != null)
                {
                    item.TokenId = transfer.TokenId;
                }
            }
            return item;
        }

        public async Task<NftItem?> GetNftByTokenIdAsync(int tokenId)
        {
            var transfer = await _context.NftTransfers
                .AsNoTracking()
                .Where(t => t.TokenId == tokenId)
                .OrderByDescending(t => t.TransferredAt)
                .FirstOrDefaultAsync();

            if (transfer != null)
            {
                var item = await _context.NftItems.AsNoTracking().FirstOrDefaultAsync(n => n.Id == transfer.NftItemId);
                if (item != null)
                {
                    item.TokenId = tokenId;
                    return item;
                }
            }

            return await _context.NftItems.AsNoTracking().FirstOrDefaultAsync(n => n.TokenId == tokenId);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.NftItems.CountAsync();
        }

        public async Task<List<NftItem>> GetUserGiftsAsync(Guid userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            var hardhatAddr = user?.HardhatAddress?.ToLowerInvariant();
            var walletAddr = user?.WalletAddress?.ToLowerInvariant();

            var items = await _context.NftItems
                .AsNoTracking()
                .Where(n => n.UserId == userId
                    || (hardhatAddr != null && n.OwnerAddress != null && n.OwnerAddress.ToLower() == hardhatAddr)
                    || (walletAddr != null && n.OwnerAddress != null && n.OwnerAddress.ToLower() == walletAddr))
                .OrderByDescending(n => n.LastTransferredAt ?? n.CreatedAt)
                .ToListAsync();

            var itemIds = items.Select(i => i.Id).ToList();
            if (itemIds.Count > 0)
            {
                var transfers = await _context.NftTransfers
                    .AsNoTracking()
                    .Where(t => itemIds.Contains(t.NftItemId))
                    .OrderByDescending(t => t.TransferredAt)
                    .ToListAsync();

                var transferMap = transfers
                    .GroupBy(t => t.NftItemId)
                    .ToDictionary(g => g.Key, g => g.First().TokenId);

                foreach (var item in items)
                {
                    if (transferMap.TryGetValue(item.Id, out var onChainTokenId))
                    {
                        item.TokenId = onChainTokenId;
                    }
                }
            }

            return items;
        }

        public async Task<List<NftTransfer>> GetTransfersByNftIdAsync(Guid nftId)
        {
            return await _context.NftTransfers
                .AsNoTracking()
                .Where(t => t.NftItemId == nftId)
                .OrderByDescending(t => t.TransferredAt)
                .ToListAsync();
        }

        public async Task<List<NftTransfer>> GetTransfersByTokenIdAsync(int tokenId)
        {
            return await _context.NftTransfers
                .AsNoTracking()
                .Where(t => t.TokenId == tokenId)
                .OrderByDescending(t => t.TransferredAt)
                .ToListAsync();
        }

        public async Task<NftItem> MintNftAsync(string recipientAddress, int? tokenId = null, string? customDescription = null)
        {
            if (string.IsNullOrWhiteSpace(recipientAddress))
                throw new ArgumentException("Recipient address is required", nameof(recipientAddress));

            NftItem? nftItem;
            if (tokenId.HasValue)
            {
                nftItem = await _context.NftItems.FirstOrDefaultAsync(n => n.TokenId == tokenId.Value);
                if (nftItem == null)
                    throw new KeyNotFoundException($"NFT with TokenId {tokenId.Value} not found.");
                if (nftItem.IsMinted)
                    throw new InvalidOperationException($"NFT with TokenId {tokenId.Value} is already minted.");
            }
            else
            {
                // Обираємо випадковий незамінчений NFT зі всієї колекції
                var unmintedIds = await _context.NftItems
                    .Where(n => !n.IsMinted)
                    .Select(n => n.Id)
                    .ToListAsync();

                if (unmintedIds.Count == 0)
                    throw new InvalidOperationException("No unminted NFTs remaining in the collection.");

                var randomId = unmintedIds[Random.Shared.Next(unmintedIds.Count)];
                nftItem = await _context.NftItems.FirstOrDefaultAsync(n => n.Id == randomId);

                if (nftItem == null)
                    throw new InvalidOperationException("Failed to retrieve selected random unminted NFT.");
            }

            if (!string.IsNullOrWhiteSpace(customDescription))
            {
                nftItem.Description = customDescription;
            }

            var rpcUrl = _configuration["Ethereum:RpcUrl"] ?? "http://127.0.0.1:8545";
            var privateKey = _configuration["Ethereum:PrivateKey"];
            var contractAddress = _configuration["Ethereum:NftContractAddress"] ?? DefaultContractAddress;

            if (string.IsNullOrWhiteSpace(privateKey))
                throw new InvalidOperationException("Ethereum:PrivateKey is not configured in settings.");

            var account = new Account(privateKey);
            var web3 = new Web3(account, rpcUrl);
            var abi = GetContractAbi();
            var contract = web3.Eth.GetContract(abi, contractAddress);

            var baseUrl = _configuration["App:BaseUrl"] ?? _configuration["BaseUrl"] ?? "http://localhost:5117";
            var tokenUri = $"{baseUrl.TrimEnd('/')}/api/nft/{nftItem.Id}";

            string txHash;
            if (nftItem.TokenId.HasValue)
            {
                var safeMintWithIdFunc = contract.GetFunction("safeMintWithId");
                var tokenIdBigInt = new BigInteger(nftItem.TokenId.Value);
                var gas = await safeMintWithIdFunc.EstimateGasAsync(recipientAddress, tokenIdBigInt, tokenUri);
                txHash = await safeMintWithIdFunc.SendTransactionAsync(
                    account.Address,
                    gas,
                    new HexBigInteger(0),
                    recipientAddress,
                    tokenIdBigInt,
                    tokenUri
                );
            }
            else
            {
                var safeMintFunction = contract.GetFunction("safeMint");
                var gas = await safeMintFunction.EstimateGasAsync(recipientAddress, tokenUri);
                txHash = await safeMintFunction.SendTransactionAsync(
                    account.Address,
                    gas,
                    new HexBigInteger(0),
                    recipientAddress,
                    tokenUri
                );
            }

            // Считываем receipt для получения настоящего ончейн tokenId
            try
            {
                var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
                if (receipt?.Logs != null)
                {
                    var transferEvent = contract.GetEvent("Transfer");
                    var decoded = transferEvent.DecodeAllEventsForEvent<TransferEventDTO>(receipt.Logs);
                    if (decoded.Count > 0)
                    {
                        var onChainId = (int)decoded[0].Event.TokenId;
                        nftItem.TokenId = onChainId;
                        nftItem.Name = $"Dollar NFT #{onChainId:D3}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[NftService] Could not decode Transfer event from receipt for tx {TxHash}", txHash);
            }

            _logger.LogInformation(
                "[NftService] Minted NFT Token #{TokenId} to {Recipient}. TxHash: {TxHash}",
                nftItem.TokenId, recipientAddress, txHash);

            nftItem.IsMinted = true;
            nftItem.OwnerAddress = recipientAddress;
            nftItem.LastTransferredAt = DateTime.UtcNow;

            var recipientLower = recipientAddress.ToLowerInvariant();
            var recipientUser = await _context.Users.FirstOrDefaultAsync(u =>
                (u.HardhatAddress != null && u.HardhatAddress.ToLower() == recipientLower) ||
                (u.WalletAddress != null && u.WalletAddress.ToLower() == recipientLower));
            if (recipientUser != null)
            {
                nftItem.UserId = recipientUser.Id;
            }

            await _context.SaveChangesAsync();

            return nftItem;
        }

        public async Task<NftItem> MintToUserAsync(Guid userId, int? tokenId = null, string? customDescription = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            var address = user.HardhatAddress ?? user.WalletAddress;
            if (string.IsNullOrWhiteSpace(address))
                throw new InvalidOperationException($"User '{user.Username}' does not have a linked wallet/Hardhat address.");

            var item = await MintNftAsync(address, tokenId, customDescription);
            if (item.UserId != userId)
            {
                item.UserId = userId;
                await _context.SaveChangesAsync();
            }
            return item;
        }

        private static NftRarity CalculateRarity(int bgIdx, int patIdx, int modIdx)
        {
            if (modIdx == 5) return NftRarity.Legendary; // Золотой Король Доллар
            if (modIdx == 4 || patIdx == 2) return NftRarity.Epic; // Джентльмен или Золотой Дождь
            if (modIdx == 3 || patIdx == 4) return NftRarity.Rare; // Киберпанк или Бычий Рынок
            if (modIdx == 2 || patIdx == 1) return NftRarity.Uncommon; // Thug Life или Неон
            return NftRarity.Common; // Классический безумный доллар
        }

        private static decimal GetBasePrice(NftRarity rarity) => rarity switch
        {
            NftRarity.Common => 100m,
            NftRarity.Uncommon => 250m,
            NftRarity.Rare => 500m,
            NftRarity.Epic => 1000m,
            NftRarity.Legendary => 2500m,
            _ => 100m
        };

        private static (string Name, string Description) GenerateMetadata(int id, int bg, int pat, int mod, NftRarity rarity)
        {
            var modelNames = new[] { "", "Crazy Dollar", "Thug Life Dollar", "Cyber Visor Dollar", "Gentleman Tycoon", "Golden King Dollar" };
            var bgNames = new[] { "", "Cyber Vault", "Cosmic Nebula", "Golden Treasury", "Neon City", "Matrix Code" };
            var patNames = new[] { "", "Neon Shower", "Gold Rain", "Laser Grid", "Bullish Chart", "Guilloche Seal" };

            var mName = mod >= 1 && mod <= 5 ? modelNames[mod] : "Dollar NFT";
            var bName = bg >= 1 && bg <= 5 ? bgNames[bg] : "Vault";
            var pName = pat >= 1 && pat <= 5 ? patNames[pat] : "Pattern";

            var fullName = $"{mName} #{id:D3}";
            var desc = $"Exclusive DTEAM Dollar NFT ({rarity}). Features {mName} on {bName} with {pName} overlay.";

            return (fullName, desc);
        }
    }
}
