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

        public static List<string> RecentLogs { get; } = new();
        public static string? LastError { get; set; }
        public static string? DetectedBlockchainDir { get; set; }
        public static string? NodeVersion { get; set; }
        public static Process? ActiveHardhatProcess { get; private set; }
        public static Process? ActiveNgrokProcess { get; private set; }

        public HardhatNodeManagerService(IConfiguration configuration, ILogger<HardhatNodeManagerService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public static void LogDiagnostic(string message)
        {
            lock (RecentLogs)
            {
                RecentLogs.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
                if (RecentLogs.Count > 100) RecentLogs.RemoveAt(0);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() => StartServicesAsync(_cts.Token), cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StartServicesAsync(CancellationToken ct)
        {
            try
            {
                CheckNodeVersion();
                await StartHardhatNodeAsync(ct);
                await StartNgrokTunnelAsync(ct);
                _ = Task.Run(() => NgrokWatchdogLoopAsync(ct), ct);
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
                LogDiagnostic($"Error initializing services: {ex.Message}");
                _logger.LogError(ex, "[HardhatNodeManager] Error initializing Hardhat / Ngrok services.");
            }
        }

        private async Task NgrokWatchdogLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(15000, ct);
                    if (_ngrokProcess == null || _ngrokProcess.HasExited)
                    {
                        LogDiagnostic("Ngrok watchdog: tunnel is offline or exited. Retrying connection...");
                        await StartNgrokTunnelAsync(ct);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    LogDiagnostic($"Ngrok watchdog error: {ex.Message}");
                }
            }
        }

        private static void CheckNodeVersion()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "node",
                    Arguments = "-v",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                if (p != null)
                {
                    NodeVersion = p.StandardOutput.ReadToEnd().Trim();
                    LogDiagnostic($"Node.js version: {NodeVersion}");
                }
            }
            catch (Exception ex)
            {
                NodeVersion = "Not found: " + ex.Message;
                LogDiagnostic($"Node.js check failed: {ex.Message}");
            }
        }

        public async Task StartHardhatNodeAsync(CancellationToken ct)
        {
            if (IsPortInUse(8545))
            {
                LogDiagnostic("Port 8545 is already in use. Hardhat node is running.");
                _logger.LogInformation("[HardhatNodeManager] Port 8545 is already in use. Assuming Hardhat node is running.");
                return;
            }

            string? blockchainDir = FindBlockchainDir();
            DetectedBlockchainDir = blockchainDir;
            if (blockchainDir == null)
            {
                LogDiagnostic("Could not find 'blockchain' directory with package.json.");
                _logger.LogWarning("[HardhatNodeManager] Could not find 'blockchain' directory with package.json. Skipping local node launch.");
                return;
            }

            LogDiagnostic($"Found blockchain directory at: {blockchainDir}");

            var nodeModulesPath = Path.Combine(blockchainDir, "node_modules");
            if (!Directory.Exists(nodeModulesPath))
            {
                LogDiagnostic("node_modules not found in blockchain dir, running npm install...");
                try
                {
                    var npmPsi = new ProcessStartInfo
                    {
                        FileName = OperatingSystem.IsWindows() ? "npm.cmd" : "npm",
                        Arguments = "install",
                        WorkingDirectory = blockchainDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    using var npmProc = Process.Start(npmPsi);
                    if (npmProc != null)
                    {
                        npmProc.OutputDataReceived += (s, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) LogDiagnostic($"[npm] {e.Data}"); };
                        npmProc.ErrorDataReceived += (s, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) LogDiagnostic($"[npm err] {e.Data}"); };
                        npmProc.BeginOutputReadLine();
                        npmProc.BeginErrorReadLine();
                        await npmProc.WaitForExitAsync(ct);
                        LogDiagnostic($"npm install finished with exit code {npmProc.ExitCode}");
                    }
                }
                catch (Exception npmEx)
                {
                    LogDiagnostic($"npm install exception: {npmEx.Message}");
                }
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
                ActiveHardhatProcess = _hardhatProcess;

                _hardhatProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogDiagnostic($"[Hardhat] {e.Data}");
                        _logger.LogInformation("[HardhatNode] {Msg}", e.Data);
                    }
                };
                _hardhatProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogDiagnostic($"[Hardhat ERR] {e.Data}");
                        _logger.LogWarning("[HardhatNode Error] {Msg}", e.Data);
                    }
                };

                _hardhatProcess.Start();
                _hardhatProcess.BeginOutputReadLine();
                _hardhatProcess.BeginErrorReadLine();
                LogDiagnostic($"Started Hardhat node process (PID: {_hardhatProcess.Id}) command: {command} {args}");

                for (int i = 0; i < 60 && !ct.IsCancellationRequested; i++)
                {
                    if (IsPortInUse(8545))
                    {
                        LogDiagnostic("Hardhat node RPC is now responding on http://127.0.0.1:8545!");
                        _logger.LogInformation("[HardhatNodeManager] Hardhat node RPC is now responding on http://127.0.0.1:8545!");
                        break;
                    }
                    if (_hardhatProcess.HasExited)
                    {
                        LogDiagnostic($"Hardhat process exited unexpectedly with code {_hardhatProcess.ExitCode}");
                        break;
                    }
                    await Task.Delay(500, ct);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
                LogDiagnostic($"Failed to launch Hardhat node: {ex.Message}");
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
                LogDiagnostic("No ngrok authtoken configured.");
                return;
            }

            if (!CanFindExecutable("ngrok"))
            {
                LogDiagnostic("'ngrok' binary not found in PATH.");
                return;
            }

            try
            {
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
                    LogDiagnostic("Configured ngrok authtoken.");
                }
            }
            catch (Exception ex)
            {
                LogDiagnostic($"Could not execute ngrok config: {ex.Message}");
            }

            try
            {
                if (_ngrokProcess != null && !_ngrokProcess.HasExited)
                {
                    try { _ngrokProcess.Kill(true); } catch { }
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "ngrok",
                    Arguments = $"http --url={domain} --pooling-enabled 8545",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                startInfo.EnvironmentVariables["NGROK_AUTHTOKEN"] = authtoken;

                _ngrokProcess = new Process { StartInfo = startInfo };
                ActiveNgrokProcess = _ngrokProcess;
                _ngrokProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogDiagnostic($"[Ngrok] {e.Data}");
                        _logger.LogInformation("[Ngrok] {Msg}", e.Data);
                    }
                };
                _ngrokProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogDiagnostic($"[Ngrok ERR] {e.Data}");
                        _logger.LogWarning("[Ngrok Error] {Msg}", e.Data);
                    }
                };

                _ngrokProcess.Start();
                _ngrokProcess.BeginOutputReadLine();
                _ngrokProcess.BeginErrorReadLine();
                LogDiagnostic($"Started ngrok tunnel (PID: {_ngrokProcess.Id}) domain {domain}");
            }
            catch (Exception ex)
            {
                LogDiagnostic($"Failed to start ngrok tunnel: {ex.Message}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            try
            {
                if (_ngrokProcess != null && !_ngrokProcess.HasExited)
                    _ngrokProcess.Kill(true);
            }
            catch { }

            try
            {
                if (_hardhatProcess != null && !_hardhatProcess.HasExited)
                    _hardhatProcess.Kill(true);
            }
            catch { }

            return Task.CompletedTask;
        }

        public static string? FindBlockchainDir()
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

        public static bool IsPortInUse(int port)
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

        public static bool CanFindExecutable(string name)
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
