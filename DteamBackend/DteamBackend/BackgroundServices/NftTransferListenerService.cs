using System.Numerics;
using DteamBackend.Data;
using DteamBackend.Models;
using Microsoft.EntityFrameworkCore;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

using System.Text.Json;

namespace DteamBackend.Services
{
    [Event("Transfer")]
    public class TransferEventDTO : IEventDTO
    {
        [Parameter("address", "from", 1, true)]
        public string From { get; set; } = string.Empty;

        [Parameter("address", "to", 2, true)]
        public string To { get; set; } = string.Empty;

        [Parameter("uint256", "tokenId", 3, true)]
        public BigInteger TokenId { get; set; }
    }

    public class NftTransferListenerService : BackgroundService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NftTransferListenerService> _logger;
        private readonly IWebHostEnvironment _env;
        private BigInteger _lastProcessedBlock = 0;
        private string? _contractAbi;

        public const string DefaultContractAddress = "0xb7278A61aa25c888815aFC32Ad3cC52fF24fE575";
        public const string ZeroAddress = "0x0000000000000000000000000000000000000000";

        public NftTransferListenerService(
            IDbContextFactory<AppDbContext> contextFactory,
            IConfiguration configuration,
            ILogger<NftTransferListenerService> logger,
            IWebHostEnvironment env)
        {
            _contextFactory = contextFactory;
            _configuration = configuration;
            _logger = logger;
            _env = env;
        }

        private string GetContractAbi()
        {
            if (!string.IsNullOrEmpty(_contractAbi))
                return _contractAbi;

            var path = Path.Combine(_env.ContentRootPath, "Contracts", "DteamNFT.json");
            if (!File.Exists(path))
            {
                var altPath = Path.Combine(AppContext.BaseDirectory, "Contracts", "DteamNFT.json");
                if (File.Exists(altPath))
                {
                    path = altPath;
                }
                else
                {
                    throw new FileNotFoundException($"ABI file not found at {path}");
                }
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[NftTransferListenerService] Starting NFT Transfer listener service...");

            var rpcUrl = _configuration["Ethereum:RpcUrl"] ?? "http://127.0.0.1:8545";
            var contractAddress = _configuration["Ethereum:NftContractAddress"] ?? DefaultContractAddress;

            // Wait 5 seconds after startup to ensure DB migrations and initial seeding are complete
            try
            {
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            // Start initial scan from block 0 to ensure any missed historical events are fully captured
            _lastProcessedBlock = 0;
            _logger.LogInformation("[NftTransferListenerService] Starting initial sync scan from block 0");

            var web3 = new Web3(rpcUrl);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var latestBlockHex = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                    var latestBlock = latestBlockHex.Value;

                    if (latestBlock >= _lastProcessedBlock)
                    {
                        var transferEvent = web3.Eth.GetEvent<TransferEventDTO>(contractAddress);
                        var filter = transferEvent.CreateFilterInput(
                            fromBlock: new BlockParameter(new HexBigInteger(_lastProcessedBlock)),
                            toBlock: new BlockParameter(new HexBigInteger(latestBlock))
                        );

                        var logs = await transferEvent.GetAllChangesAsync(filter);

                        if (logs != null && logs.Count > 0)
                        {
                            _logger.LogInformation("[NftTransferListenerService] Found {Count} Transfer event(s) between blocks {From} and {To}",
                                logs.Count, _lastProcessedBlock, latestBlock);

                            await using var db = await _contextFactory.CreateDbContextAsync(stoppingToken);

                            foreach (var log in logs)
                            {
                                await ProcessTransferEventAsync(db, log, stoppingToken);
                            }

                            await db.SaveChangesAsync(stoppingToken);
                        }

                        _lastProcessedBlock = latestBlock + 1;
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("[NftTransferListenerService] Polling check: {Message}. Retrying...", ex.Message);
                }

                try
                {
                    await Task.Delay(3000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("[NftTransferListenerService] NFT Transfer listener service stopped.");
        }

        private async Task ProcessTransferEventAsync(
            AppDbContext db,
            EventLog<TransferEventDTO> log,
            CancellationToken ct)
        {
            var from = log.Event.From?.Trim() ?? string.Empty;
            var to = log.Event.To?.Trim() ?? string.Empty;
            var tokenId = (int)log.Event.TokenId;
            var txHash = log.Log.TransactionHash ?? string.Empty;
            var blockNumber = (long)(log.Log.BlockNumber?.Value ?? 0);

            _logger.LogInformation("[NftTransferListenerService] Processing Transfer: From={From}, To={To}, TokenId={TokenId}, Tx={TxHash}",
                from, to, tokenId, txHash);

            // Prevent duplicate records for the same transaction & token
            var existing = await db.NftTransfers.FirstOrDefaultAsync(
                t => t.TransactionHash == txHash && t.TokenId == tokenId, ct);
            if (existing != null)
            {
                return;
            }

            // Try matching NftItem by tokenURI from contract (since random items are bound via /api/nft/{guid})
            NftItem? nftItem = null;
            try
            {
                var rpcUrl = _configuration["Ethereum:RpcUrl"] ?? "http://127.0.0.1:8545";
                var contractAddress = _configuration["Ethereum:NftContractAddress"] ?? DefaultContractAddress;
                var web3 = new Web3(rpcUrl);
                var contract = web3.Eth.GetContract(GetContractAbi(), contractAddress);
                var tokenUri = await contract.GetFunction("tokenURI").CallAsync<string>(tokenId);
                if (!string.IsNullOrEmpty(tokenUri))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(tokenUri, @"/api/nft/([a-f0-9\-]{36})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success && Guid.TryParse(match.Groups[1].Value, out var guid))
                    {
                        nftItem = await db.NftItems.FirstOrDefaultAsync(n => n.Id == guid, ct);
                    }
                }
            }
            catch (Exception uriEx)
            {
                _logger.LogDebug("[NftTransferListenerService] Could not resolve tokenURI for #{TokenId}: {Msg}", tokenId, uriEx.Message);
            }

            // Fallback: Find matching NftItem directly by TokenId
            if (nftItem == null)
            {
                nftItem = await db.NftItems.FirstOrDefaultAsync(n => n.TokenId == tokenId, ct);
            }

            if (nftItem == null)
            {
                _logger.LogWarning("[NftTransferListenerService] NftItem with TokenId={TokenId} not found in DB. Creating placeholder record.", tokenId);
                nftItem = new NftItem
                {
                    Id = Guid.NewGuid(),
                    TokenId = tokenId,
                    Name = $"Dollar NFT #{tokenId:D3}",
                    Description = $"Minted on-chain DNFT Token #{tokenId}",
                    ImageUrl = "/nft/1_1_1.png",
                    ContractAddress = log.Log.Address ?? DefaultContractAddress,
                    IsMinted = true,
                    CreatedAt = DateTime.UtcNow
                };
                await db.NftItems.AddAsync(nftItem, ct);
            }

            // Find recipient user with Gifts (by HardhatAddress or WalletAddress, case-insensitive)
            Duser? recipientUser = null;
            if (!string.IsNullOrWhiteSpace(to) && !to.Equals(ZeroAddress, StringComparison.OrdinalIgnoreCase))
            {
                var lowerTo = to.ToLowerInvariant();
                recipientUser = await db.Users
                    .Include(u => u.Gifts)
                    .FirstOrDefaultAsync(u =>
                        (u.HardhatAddress != null && u.HardhatAddress.ToLower() == lowerTo) ||
                        (u.WalletAddress != null && u.WalletAddress.ToLower() == lowerTo), ct);
            }

            // Find sender user with Gifts (if transferred from another user rather than minted from 0x0)
            Duser? senderUser = null;
            if (!string.IsNullOrWhiteSpace(from) && !from.Equals(ZeroAddress, StringComparison.OrdinalIgnoreCase))
            {
                var lowerFrom = from.ToLowerInvariant();
                senderUser = await db.Users
                    .Include(u => u.Gifts)
                    .FirstOrDefaultAsync(u =>
                        (u.HardhatAddress != null && u.HardhatAddress.ToLower() == lowerFrom) ||
                        (u.WalletAddress != null && u.WalletAddress.ToLower() == lowerFrom), ct);
            }

            // Remove NFT from previous owner's Gifts collection
            if (senderUser != null)
            {
                var giftToRemove = senderUser.Gifts.FirstOrDefault(g => g.Id == nftItem.Id || (g.TokenId.HasValue && g.TokenId == tokenId));
                if (giftToRemove != null)
                {
                    senderUser.Gifts.Remove(giftToRemove);
                }
            }

            // Update NftItem state and owner
            nftItem.TokenId = tokenId;
            nftItem.Name = $"Dollar NFT #{tokenId:D3}";
            nftItem.OwnerAddress = to;
            nftItem.FromAddress = from;
            nftItem.UserId = recipientUser?.Id;
            nftItem.User = recipientUser;
            nftItem.IsMinted = true;
            nftItem.LastTransferredAt = DateTime.UtcNow;

            // Add NFT to new recipient's Gifts collection
            if (recipientUser != null)
            {
                if (!recipientUser.Gifts.Any(g => g.Id == nftItem.Id || (g.TokenId.HasValue && g.TokenId == tokenId)))
                {
                    recipientUser.Gifts.Add(nftItem);
                }
            }

            // Record transfer record in DB
            var transferRecord = new NftTransfer
            {
                Id = Guid.NewGuid(),
                NftItemId = nftItem.Id,
                TokenId = tokenId,
                FromAddress = from,
                ToAddress = to,
                FromUserId = senderUser?.Id,
                ToUserId = recipientUser?.Id,
                TransactionHash = txHash,
                BlockNumber = blockNumber,
                TransferredAt = DateTime.UtcNow
            };

            await db.NftTransfers.AddAsync(transferRecord, ct);

            _logger.LogInformation(
                "[NftTransferListenerService] Transfer saved to DB: Token #{TokenId} -> Recipient User: '{Username}' (Id: {UserId}), Owner: {OwnerAddress}",
                tokenId, recipientUser?.Username ?? "Unknown/External", recipientUser?.Id, to);
        }
    }
}
