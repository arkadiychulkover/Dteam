using System.Security.Cryptography;
using DteamBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Services
{
    public class TonService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TonService> _logger;
        private readonly string _apiKey;
        private readonly string _address;

        public TonService(
            IDbContextFactory<AppDbContext> contextFactory, 
            IHttpClientFactory httpFactory, 
            IConfiguration configuration, 
            ILogger<TonService> logger)
        {
            _contextFactory = contextFactory;
            _httpFactory = httpFactory;
            _configuration = configuration;
            _logger = logger;

            _apiKey = _configuration["Ton:ApiKey"] ?? string.Empty;
            _address = _configuration["Ton:Address"] ?? string.Empty;
        }

        private static string NormalizeAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return string.Empty;

            address = address.Trim();

            if (address.Contains(':'))
            {
                var parts = address.Split(':');
                if (parts.Length == 2 && parts[1].Length == 64)
                {
                    return parts[1].ToLowerInvariant();
                }
            }

            try
            {
                var bytes = Convert.FromBase64String(address.Replace('-', '+').Replace('_', '/'));
                if (bytes.Length == 36)
                {
                    var hash = bytes.Skip(2).Take(32).ToArray();
                    return Convert.ToHexString(hash).ToLowerInvariant();
                }
            }
            catch
            {
            }

            return address.ToLowerInvariant();
        }

        public async Task<bool> CheckTranzaction(string txhHash, decimal amount)
        {
            _logger.LogInformation($"[TonService] Starting transaction check. Input hash: {txhHash}, Expected amount: {amount}");

            try
            {
                if (string.IsNullOrWhiteSpace(txhHash))
                {
                    _logger.LogWarning("[TonService] Transaction hash is null or empty.");
                    return false;
                }

                string expectedHashHex;
                if (txhHash.Length == 64 && txhHash.All(c => "0123456789abcdefABCDEF".Contains(c)))
                {
                    expectedHashHex = txhHash.ToLower();
                    _logger.LogInformation($"[TonService] Input identified as pure Hex TX ID: {expectedHashHex}");
                }
                else
                {
                    var bocBytes = Convert.FromBase64String(txhHash.Replace('-', '+').Replace('_', '/'));
                    expectedHashHex = Convert.ToHexString(SHA256.Create().ComputeHash(bocBytes)).ToLower();
                    _logger.LogInformation($"[TonService] Input identified as BOC. Computed Hex TX ID: {expectedHashHex}");
                }

                var expectedAddressNorm = NormalizeAddress(_address);

                using var client = _httpFactory.CreateClient();
                if (!string.IsNullOrWhiteSpace(_apiKey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                }
                client.DefaultRequestHeaders.Add("User-Agent", "DteamWeb3Platform/1.0");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                _logger.LogInformation($"[TonService] Sending request to TonAPI for address: {_address}");
                var response = await client.GetAsync($"https://tonapi.io/v2/blockchain/accounts/{_address}/transactions?limit=20");

                _logger.LogInformation($"[TonService] TonAPI responded with status code: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"[TonService] TonAPI request failed. Status: {response.StatusCode}");
                    return false;
                }

                var rawBody = await response.Content.ReadAsStringAsync();
                var content = System.Text.Json.JsonDocument.Parse(rawBody);

                if (!content.RootElement.TryGetProperty("transactions", out var transactions) ||
                    transactions.ValueKind != System.Text.Json.JsonValueKind.Array)
                {
                    _logger.LogWarning("[TonService] Failed to parse 'transactions' array from TonAPI response.");
                    return false;
                }

                var txCount = transactions.GetArrayLength();
                _logger.LogInformation($"[TonService] Received {txCount} transactions from TonAPI.");

                long tenMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 600;
                using var checkContext = await _contextFactory.CreateDbContextAsync();

                foreach (var tx in transactions.EnumerateArray())
                {
                    var utime = tx.GetProperty("utime").GetInt64();
                    var txHash = tx.GetProperty("hash").GetString()?.ToLower();
                    var inMsgHash = tx.TryGetProperty("in_msg", out var inMsgTemp) && inMsgTemp.TryGetProperty("hash", out var imh) ? imh.GetString()?.ToLower() : null;

                    _logger.LogInformation($"[TonService] Checking transaction {txHash} (InMsg: {inMsgHash}) at time {utime} (tenMinutesAgo limit: {tenMinutesAgo})");

                    if (utime < tenMinutesAgo)
                    {
                        _logger.LogInformation($"[TonService] Transaction {txHash} is older than 10 minutes. Skipping.");
                        continue;
                    }

                    if (txHash != expectedHashHex && inMsgHash != expectedHashHex)
                    {
                        _logger.LogInformation($"[TonService] Neither Transaction {txHash} nor InMsg {inMsgHash} matches expected {expectedHashHex}. Skipping.");
                        continue;
                    }

                    if (!tx.TryGetProperty("in_msg", out var msg) || msg.GetProperty("msg_type").GetString() != "int_msg")
                    {
                        _logger.LogInformation($"[TonService] Transaction {txHash} has no in_msg or is not int_msg. Skipping.");
                        continue;
                    }

                    string? destination = msg.TryGetProperty("destination", out var dst) && dst.TryGetProperty("address", out var addr)
                        ? addr.GetString() : null;

                    decimal txAmount = msg.TryGetProperty("value", out var val) ? val.GetInt64() / 1_000_000_000m : 0;

                    var destinationNorm = NormalizeAddress(destination);

                    _logger.LogInformation($"[TonService] Match found for hash {txHash}. Dest: {destination} (norm: {destinationNorm}), Amount: {txAmount}, Expected dest norm: {expectedAddressNorm}");

                    if (destinationNorm == expectedAddressNorm && txAmount >= amount)
                    {
                        if (await checkContext.Tranxactions.AnyAsync(t => t.TxhHash == txhHash || t.TxhHash == txHash || t.TxhHash == expectedHashHex))
                        {
                            _logger.LogWarning($"[TonService] Transaction {txHash} already exists in DB! Duplicate payment attempt.");
                            return false;
                        }

                        _logger.LogInformation($"[TonService] Transaction {txHash} verified successfully!");
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[TonService] Transaction {txHash} amount ({txAmount}) or destination ({destinationNorm}) did not meet requirements (Expected Amount: {amount}, Expected Dest: {expectedAddressNorm}).");
                    }
                }

                _logger.LogInformation($"[TonService] Finished processing all {txCount} transactions in recent history. No matching valid transaction found.");

                _logger.LogInformation($"[TonService] Fallback: Querying TonAPI directly for transaction {expectedHashHex}");
                var txResponse = await client.GetAsync($"https://tonapi.io/v2/blockchain/transactions/{expectedHashHex}");
                if (txResponse.IsSuccessStatusCode)
                {
                    var txRawBody = await txResponse.Content.ReadAsStringAsync();
                    var txDoc = System.Text.Json.JsonDocument.Parse(txRawBody);
                    var singleTx = txDoc.RootElement;

                    var utime = singleTx.GetProperty("utime").GetInt64();
                    if (utime >= tenMinutesAgo)
                    {
                        if (singleTx.TryGetProperty("out_msgs", out var outMsgs) && outMsgs.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var msg in outMsgs.EnumerateArray())
                            {
                                if (msg.TryGetProperty("msg_type", out var msgType) && msgType.GetString() == "int_msg")
                                {
                                    string? destination = msg.TryGetProperty("destination", out var dst) && dst.TryGetProperty("address", out var addr) ? addr.GetString() : null;
                                    decimal txAmount = msg.TryGetProperty("value", out var val) ? val.GetInt64() / 1_000_000_000m : 0;

                                    var destinationNorm = NormalizeAddress(destination);

                                    _logger.LogInformation($"[TonService] Fallback out_msg found. Dest: {destination} (norm: {destinationNorm}), Amount: {txAmount} (Expected Dest norm: {expectedAddressNorm}, Expected Amount: {amount})");

                                    if (destinationNorm == expectedAddressNorm && txAmount >= amount)
                                    {
                                        if (await checkContext.Tranxactions.AnyAsync(t => t.TxhHash == txhHash || t.TxhHash == expectedHashHex))
                                        {
                                            _logger.LogWarning($"[TonService] Fallback: Transaction {expectedHashHex} already exists in DB!");
                                            return false;
                                        }
                                        _logger.LogInformation($"[TonService] Fallback: Transaction {expectedHashHex} verified successfully via out_msgs!");
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"[TonService] Fallback: Transaction {expectedHashHex} is older than 10 minutes.");
                    }
                }
                else
                {
                    _logger.LogWarning($"[TonService] Fallback query failed with status: {txResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TonService] Exception occurred while checking transaction {txhHash}.");
                return false;
            }

            return false;
        }
    }
}
