using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using DteamBackend.Configuration;
using DteamBackend.Data;
using DteamBackend.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace DteamBackend.Services
{
    public class HardhatTokenService : IHardhatTokenService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HardhatTokenService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly EthereumOptions _options;
        private string? _contractAbi;

        public HardhatTokenService(
            AppDbContext context,
            IConfiguration configuration,
            IOptions<EthereumOptions> options,
            ILogger<HardhatTokenService> logger,
            IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _options = options.Value;
            _logger = logger;
            _env = env;
        }

        private string GetContractAbi()
        {
            if (!string.IsNullOrEmpty(_contractAbi))
                return _contractAbi;

            var path = Path.Combine(_env.ContentRootPath, "Contracts", "DteamPoints.json");
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

        private Web3 GetWeb3()
        {
            var rpcUrl = string.IsNullOrWhiteSpace(_options.RpcUrl)
                ? _configuration["Ethereum:RpcUrl"] ?? "http://127.0.0.1:8545"
                : _options.RpcUrl;

            var privateKey = string.IsNullOrWhiteSpace(_options.PrivateKey)
                ? _configuration["Ethereum:PrivateKey"]
                : _options.PrivateKey;

            if (string.IsNullOrWhiteSpace(privateKey))
            {
                throw new InvalidOperationException("Ethereum:PrivateKey is not configured in settings.");
            }

            var account = new Account(privateKey);
            return new Web3(account, rpcUrl);
        }

        private string GetContractAddress()
        {
            var address = string.IsNullOrWhiteSpace(_options.ContractAddress)
                ? _configuration["Ethereum:ContractAddress"]
                : _options.ContractAddress;

            if (string.IsNullOrWhiteSpace(address))
            {
                throw new InvalidOperationException("Ethereum:ContractAddress is not configured in settings.");
            }

            return address;
        }

        public async Task<string> AwardTokensByAddressAsync(string recipientAddress, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(recipientAddress))
                throw new ArgumentException("Recipient address is required", nameof(recipientAddress));

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            var web3 = GetWeb3();
            var contractAddress = GetContractAddress();
            var abi = GetContractAbi();
            var contract = web3.Eth.GetContract(abi, contractAddress);
            var mintFunction = contract.GetFunction("mint");

            var amountWei = Web3.Convert.ToWei(amount);

            var gas = await mintFunction.EstimateGasAsync(recipientAddress, amountWei);
            var txHash = await mintFunction.SendTransactionAsync(
                web3.TransactionManager.Account.Address,
                gas,
                new HexBigInteger(0),
                recipientAddress,
                amountWei
            );

            _logger.LogInformation("[HardhatTokenService] Awarded {Amount} DTP to {Recipient}. TxHash: {TxHash}", amount, recipientAddress, txHash);
            return txHash;
        }

        public async Task<string> AwardTokensAsync(Guid userId, decimal amount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (string.IsNullOrWhiteSpace(user.HardhatAddress))
                throw new InvalidOperationException($"User '{user.Username}' does not have a linked Hardhat/MetaMask address.");

            return await AwardTokensByAddressAsync(user.HardhatAddress, amount);
        }

        public async Task<string> DebitTokensByAddressAsync(string fromAddress, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(fromAddress))
                throw new ArgumentException("From address is required", nameof(fromAddress));

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            var web3 = GetWeb3();
            var contractAddress = GetContractAddress();
            var abi = GetContractAbi();
            var contract = web3.Eth.GetContract(abi, contractAddress);
            var burnFunction = contract.GetFunction("burn");

            var amountWei = Web3.Convert.ToWei(amount);

            var gas = await burnFunction.EstimateGasAsync(fromAddress, amountWei);
            var txHash = await burnFunction.SendTransactionAsync(
                web3.TransactionManager.Account.Address,
                gas,
                new HexBigInteger(0),
                fromAddress,
                amountWei
            );

            _logger.LogInformation("[HardhatTokenService] Debited/burned {Amount} DTP from {FromAddress}. TxHash: {TxHash}", amount, fromAddress, txHash);
            return txHash;
        }

        public async Task<string> DebitTokensAsync(Guid userId, decimal amount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (string.IsNullOrWhiteSpace(user.HardhatAddress))
                throw new InvalidOperationException($"User '{user.Username}' does not have a linked Hardhat/MetaMask address.");

            return await DebitTokensByAddressAsync(user.HardhatAddress, amount);
        }

        public async Task<decimal> GetBalanceAsync(string walletAddress)
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
                throw new ArgumentException("Wallet address is required", nameof(walletAddress));

            var web3 = GetWeb3();
            var contractAddress = GetContractAddress();
            var abi = GetContractAbi();
            var contract = web3.Eth.GetContract(abi, contractAddress);
            var balanceOfFunction = contract.GetFunction("balanceOf");

            var balanceWei = await balanceOfFunction.CallAsync<BigInteger>(walletAddress);
            return Web3.Convert.FromWei(balanceWei);
        }

        public async Task<string> UpdateAdminAddressFromSettingsAsync()
        {
            var configuredAdminAddress = _configuration["Ethereum:PublicKey"]?.Trim();
            if (string.IsNullOrWhiteSpace(configuredAdminAddress))
            {
                throw new InvalidOperationException("Ethereum:PublicKey is not configured in appsettings.json.");
            }

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.IsAdmin || u.Username == "adim");
            if (adminUser == null)
            {
                throw new KeyNotFoundException("Admin user not found in database.");
            }

            if (!string.Equals(adminUser.HardhatAddress, configuredAdminAddress, StringComparison.OrdinalIgnoreCase))
            {
                adminUser.HardhatAddress = configuredAdminAddress;
                adminUser.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("[HardhatTokenService] Updated admin user '{Username}' HardhatAddress to '{Address}' from appsettings.json", adminUser.Username, configuredAdminAddress);
            }

            return configuredAdminAddress;
        }

        public async Task<(bool isMatch, string? registeredAddress, string providedAddress)> VerifyWalletMatchAsync(Guid userId, string providedAddress)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            var registeredAddress = user.HardhatAddress?.Trim();
            var cleanProvidedAddress = providedAddress?.Trim() ?? string.Empty;

            bool isMatch = !string.IsNullOrEmpty(registeredAddress) &&
                           string.Equals(registeredAddress, cleanProvidedAddress, StringComparison.OrdinalIgnoreCase);

            return (isMatch, registeredAddress, cleanProvidedAddress);
        }
    }
}
