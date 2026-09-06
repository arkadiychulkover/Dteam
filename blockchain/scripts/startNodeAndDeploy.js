import { spawn } from "child_process";
import http from "http";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const blockchainRoot = path.resolve(__dirname, "..");

console.log("===================================================");
console.log("  Hardhat Node with Auto-Deploy & Auto-Sync");
console.log("===================================================");

function checkRpcReady() {
    return new Promise((resolve) => {
        const req = http.request(
            {
                hostname: "127.0.0.1",
                port: 8545,
                path: "/",
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                timeout: 1000,
            },
            (res) => {
                if (res.statusCode === 200) {
                    resolve(true);
                } else {
                    resolve(false);
                }
            }
        );
        req.on("error", () => resolve(false));
        req.on("timeout", () => {
            req.destroy();
            resolve(false);
        });
        req.write(JSON.stringify({ jsonrpc: "2.0", method: "net_version", params: [], id: 1 }));
        req.end();
    });
}

async function waitForRpc(maxRetries = 60, delayMs = 500) {
    for (let i = 0; i < maxRetries; i++) {
        const ready = await checkRpcReady();
        if (ready) return true;
        await new Promise((r) => setTimeout(r, delayMs));
    }
    return false;
}

async function main() {
    const isAlreadyRunning = await checkRpcReady();
    let nodeProcess = null;

    if (isAlreadyRunning) {
        console.log("ℹ️  Hardhat Node is already running on http://127.0.0.1:8545");
    } else {
        console.log("🚀 Starting `npx hardhat node` in background...");
        const isWindows = process.platform === "win32";
        const cmd = isWindows ? "npx.cmd" : "npx";
        nodeProcess = spawn(cmd, ["hardhat", "node"], {
            cwd: blockchainRoot,
            stdio: "inherit",
            shell: true,
        });

        nodeProcess.on("error", (err) => {
            console.error("❌ Failed to start hardhat node:", err);
            process.exit(1);
        });

        console.log("⏳ Waiting for RPC to become ready on http://127.0.0.1:8545...");
        const ready = await waitForRpc();
        if (!ready) {
            console.error("❌ Timed out waiting for Hardhat RPC to start.");
            if (nodeProcess) nodeProcess.kill();
            process.exit(1);
        }
    }

    console.log("\n===================================================");
    console.log("  RPC is ready! Deploying contracts & syncing...");
    console.log("===================================================");

    const isWindows = process.platform === "win32";
    const cmd = isWindows ? "npx.cmd" : "npx";
    const deployProcess = spawn(
        cmd,
        ["hardhat", "run", "scripts/deployAll.js", "--network", "localhost"],
        {
            cwd: blockchainRoot,
            stdio: "inherit",
            shell: true,
        }
    );

    deployProcess.on("close", (code) => {
        if (code === 0) {
            console.log("\n===================================================");
            console.log("🎉 All contracts deployed and synced successfully!");
            console.log("   Node is running. Press Ctrl+C in this terminal to stop.");
            console.log("===================================================\n");
        } else {
            console.error(`\n⚠️ Deploy script finished with exit code ${code}`);
        }
    });

    if (nodeProcess) {
        const cleanup = () => {
            try {
                nodeProcess.kill();
            } catch (e) {}
            process.exit();
        };
        process.on("SIGINT", cleanup);
        process.on("SIGTERM", cleanup);
    }
}

main().catch(console.error);

