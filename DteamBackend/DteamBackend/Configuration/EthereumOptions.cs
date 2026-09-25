namespace DteamBackend.Configuration
{
    public class EthereumOptions
    {
        public const string SectionName = "Ethereum";

        public string RpcUrl { get; set; } = "http://127.0.0.1:8545";
        public string PublicKey { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string ContractAddress { get; set; } = string.Empty;
    }
}
