using System.Text;
using System.Text.Json;
using DteamBackend.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DteamBackend.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [EnableCors("DteamCorsPolicy")]
    [Route("api/[controller]")]
    [Route("rpc")]
    public class RpcController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RpcController> _logger;

        public RpcController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RpcController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [HttpGet]
        [HttpOptions]
        public async Task<IActionResult> HandleRpc()
        {
            Response.Headers["Access-Control-Allow-Origin"] = "*";
            Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
            Response.Headers["Access-Control-Allow-Headers"] = "*";

            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }

            if (Request.Method == "GET")
            {
                return Ok(new
                {
                    status = "ok",
                    network = "Hardhat",
                    chainId = 31337,
                    isPort8545Open = HardhatNodeManagerService.IsPortInUse(8545),
                    nodeVersion = HardhatNodeManagerService.NodeVersion,
                    blockchainDir = HardhatNodeManagerService.DetectedBlockchainDir,
                    isProcessActive = HardhatNodeManagerService.ActiveHardhatProcess != null && !HardhatNodeManagerService.ActiveHardhatProcess.HasExited,
                    isNgrokActive = HardhatNodeManagerService.ActiveNgrokProcess != null && !HardhatNodeManagerService.ActiveNgrokProcess.HasExited,
                    processExitCode = HardhatNodeManagerService.ActiveHardhatProcess?.HasExited == true ? HardhatNodeManagerService.ActiveHardhatProcess.ExitCode : (int?)null,
                    recentLogs = HardhatNodeManagerService.RecentLogs.TakeLast(30).ToList()
                });
            }

            string requestBody = string.Empty;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            object? requestId = 1;
            string? method = null;

            try
            {
                using var jsonDoc = JsonDocument.Parse(requestBody);
                if (jsonDoc.RootElement.TryGetProperty("id", out var idProp))
                {
                    requestId = idProp.ValueKind switch
                    {
                        JsonValueKind.Number => idProp.GetInt64(),
                        JsonValueKind.String => idProp.GetString(),
                        _ => 1
                    };
                }
                if (jsonDoc.RootElement.TryGetProperty("method", out var methodProp))
                {
                    method = methodProp.GetString();
                }
            }
            catch { }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var localRpcUrl = _configuration["Ethereum:LocalRpcUrl"]
                    ?? _configuration["Ethereum:RpcUrl"]
                    ?? "http://127.0.0.1:8545";

                using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(localRpcUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return Content(responseContent, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[RpcController] Proxy call to local Hardhat failed for method {Method}", method);

                if (method == "eth_chainId")
                {

                    return Ok(new { jsonrpc = "2.0", id = requestId, result = "0x7a69" });
                }
                if (method == "net_version")
                {
                    return Ok(new { jsonrpc = "2.0", id = requestId, result = "31337" });
                }
                if (method == "eth_blockNumber")
                {
                    return Ok(new { jsonrpc = "2.0", id = requestId, result = "0x1" });
                }
                if (method == "eth_accounts")
                {
                    return Ok(new { jsonrpc = "2.0", id = requestId, result = Array.Empty<string>() });
                }

                return StatusCode(502, new
                {
                    jsonrpc = "2.0",
                    id = requestId,
                    error = new
                    {
                        code = -32603,
                        message = "Local Hardhat node proxy error: " + ex.Message
                    }
                });
            }
        }
    }
}
