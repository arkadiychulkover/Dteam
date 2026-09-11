using System.Diagnostics;
using System.Net.Sockets;

namespace DteamBackend.BackgroundServices
{
    public class HardhatNodeManagerService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HardhatNodeManagerService> _logger;
        private Process? _hardhatProcess;
        private Process? _ngrokProcess;
        private readonly CancellationTokenSource _cts = new();

        public HardhatNodeManagerService(IConfiguration configuration, ILogger<HardhatNodeManagerService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Execute in separate background task to not delay ASP.NET web app startup
            _ = Task.Run(() => StartServicesAsync(_cts.Token), cancellationToken);
            return Task.CompletedTask;
        }

        private async Task StartServicesAsync(CancellationToken ct)
        {
            try
            {
                await StartHardhatNodeAsync(ct);
                await StartNgrokTunnelAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HardhatNodeManager] Error initializing Hardhat / Ngrok services.");
            }
        }

        private async Task StartHardhatNodeAsync(CancellationToken ct)
        {
            if (IsPortInUse(8545))
            {
                _logger.LogInformation("[HardhatNodeManager] Port 8545 is already in use. Assuming Hardhat node is running.");
                return;
            }

            string? blockchainDir = FindBlockchainDir();
            if (blockchainDir == null)
            {
                _logger.LogWarning("[HardhatNodeManager] Could not find 'blockchain' directory with package.json. Skipping local node launch.");
                return;
            }

            var startNodeScript = Path.Combine(blockchainDir, "scripts", "startNodeAndDeploy.js");
            string command;
            string args;

            if (File.Exists(startNodeScript))
            {
                command = "node";
                args = "scripts/startNodeAndDeploy.js";
            }
            else
            {
                command = OperatingSystem.IsWindows() ? "npx.cmd" : "npx";
                args = "hardhat node";
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    WorkingDirectory = blockchainDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _hardhatProcess = new Process { StartInfo = startInfo };
                _hardhatProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        _logger.LogInformation("[HardhatNode] {Msg}", e.Data);
                };
                _hardhatProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        _logger.LogWarning("[HardhatNode Error] {Msg}", e.Data);
                };

                _hardhatProcess.Start();
                _hardhatProcess.BeginOutputReadLine();
                _hardhatProcess.BeginErrorReadLine();
                _logger.LogInformation("[HardhatNodeManager] Started Hardhat node process (PID: {Pid}) in {Dir}", _hardhatProcess.Id, blockchainDir);

                // Wait up to 30 seconds for port 8545 to open
                for (int i = 0; i < 60 && !ct.IsCancellationRequested; i++)
                {
                    if (IsPortInUse(8545))
                    {
                        _logger.LogInformation("[HardhatNodeManager] Hardhat node RPC is now responding on http://127.0.0.1:8545!");
                        break;
                    }
                    await Task.Delay(500, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HardhatNodeManager] Failed to launch Hardhat node process.");
            }
        }

        private async Task StartNgrokTunnelAsync(CancellationToken ct)
        {
            var authtoken = _configuration["Ngrok:AuthToken"]
                ?? Environment.GetEnvironmentVariable("NGROK_AUTHTOKEN")
                ?? "3GUkEq28LEcrwxwbHNioWfgWNHO_5nPV4pw1ho9vYLS1QeQNx";

            var domain = _configuration["Ngrok:Domain"]
                ?? Environment.GetEnvironmentVariable("NGROK_DOMAIN")
                ?? "goldmine-unloved-capsule.ngrok-free.dev";

            if (string.IsNullOrWhiteSpace(authtoken))
            {
                _logger.LogWarning("[Ngrok] No authtoken configured. Skipping ngrok tunnel.");
                return;
            }

            if (!CanFindExecutable("ngrok"))
            {
                _logger.LogWarning("[Ngrok] 'ngrok' binary not found in PATH or environment. Skipping ngrok tunnel.");
                return;
            }

            try
            {
                // Run config add-authtoken
                var configPsi = new ProcessStartInfo
                {
                    FileName = "ngrok",
                    Arguments = $"config add-authtoken {authtoken}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var configProcess = Process.Start(configPsi);
                if (configProcess != null)
                {
                    await configProcess.WaitForExitAsync(ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Ngrok] Could not execute ngrok config command. Will rely on NGROK_AUTHTOKEN env var.");
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ngrok",
                    Arguments = $"http --url={domain} 8545",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                startInfo.EnvironmentVariables["NGROK_AUTHTOKEN"] = authtoken;

                _ngrokProcess = new Process { StartInfo = startInfo };
                _ngrokProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        _logger.LogInformation("[Ngrok] {Msg}", e.Data);
                };
                _ngrokProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        _logger.LogWarning("[Ngrok Error] {Msg}", e.Data);
                };

                _ngrokProcess.Start();
                _ngrokProcess.BeginOutputReadLine();
                _ngrokProcess.BeginErrorReadLine();
                _logger.LogInformation("[Ngrok] Started ngrok tunnel to port 8545 with domain {Domain} (PID: {Pid})", domain, _ngrokProcess.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Ngrok] Failed to start ngrok tunnel process.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            try
            {
                if (_ngrokProcess != null && !_ngrokProcess.HasExited)
                {
                    _ngrokProcess.Kill(true);
                    _logger.LogInformation("[HardhatNodeManager] Stopped ngrok tunnel process.");
                }
            }
            catch { }

            try
            {
                if (_hardhatProcess != null && !_hardhatProcess.HasExited)
                {
                    _hardhatProcess.Kill(true);
                    _logger.LogInformation("[HardhatNodeManager] Stopped Hardhat node process.");
                }
            }
            catch { }

            return Task.CompletedTask;
        }

        private static string? FindBlockchainDir()
        {
            var candidates = new[]
            {
                "/app/blockchain",
                Path.Combine(AppContext.BaseDirectory, "blockchain"),
                Path.Combine(Directory.GetCurrentDirectory(), "blockchain"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "blockchain"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "blockchain"),
                "d:\\Dteam\\blockchain"
            };

            foreach (var dir in candidates)
            {
                if (Directory.Exists(dir) && File.Exists(Path.Combine(dir, "package.json")))
                {
                    return Path.GetFullPath(dir);
                }
            }
            return null;
        }

        private static bool IsPortInUse(int port)
        {
            try
            {
                using var client = new TcpClient();
                var result = client.BeginConnect("127.0.0.1", port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                if (success)
                {
                    client.EndConnect(result);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool CanFindExecutable(string name)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "where" : "which",
                    Arguments = name,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p?.WaitForExit();
                return p?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
