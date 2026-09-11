using System.Text;
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
                    message = "Dteam JSON-RPC 2.0 proxy endpoint is active.",
                    chainId = 31337
                });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                var localRpcUrl = _configuration["Ethereum:LocalRpcUrl"] ?? "http://127.0.0.1:8545";

                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var requestBody = await reader.ReadToEndAsync();

                using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(localRpcUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return Content(responseContent, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[RpcController] Error proxying JSON-RPC request to local Hardhat node.");
                return StatusCode(502, new
                {
                    jsonrpc = "2.0",
                    id = (object?)null,
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
