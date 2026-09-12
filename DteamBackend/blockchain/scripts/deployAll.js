import { network } from "hardhat";
import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

function syncAddresses(pointsAddress, nftAddress) {
    console.log("\n--- Synchronizing Contract Addresses across Project ---");

    let backendAppSettingsPath = path.resolve(__dirname, "../../DteamBackend/DteamBackend/appsettings.json");
    if (!fs.existsSync(backendAppSettingsPath)) {
        const candidates = [
            path.resolve(__dirname, "../../appsettings.json"),
            path.resolve(__dirname, "../appsettings.json"),
            "/app/appsettings.json"
        ];
        for (const c of candidates) {
            if (fs.existsSync(c)) {
                backendAppSettingsPath = c;
                break;
            }
        }
    }
    if (fs.existsSync(backendAppSettingsPath)) {
        try {
            const content = fs.readFileSync(backendAppSettingsPath, "utf-8");
            const config = JSON.parse(content);
            if (!config.Ethereum) config.Ethereum = {};
            config.Ethereum.ContractAddress = pointsAddress;
            config.Ethereum.NftContractAddress = nftAddress;
            fs.writeFileSync(backendAppSettingsPath, JSON.stringify(config, null, 2) + "\n", "utf-8");
            console.log(" [Backend] Updated appsettings.json with new addresses");
        } catch (err) {
            console.warn("  [Backend] Could not update appsettings.json:", err.message);
        }
    }

    const frontendContractsDir = path.resolve(__dirname, "../../DteamFrontend/dteam-app/src/lib/contracts");
    if (!fs.existsSync(frontendContractsDir)) {
        fs.mkdirSync(frontendContractsDir, { recursive: true });
    }
    const frontendAddressesPath = path.join(frontendContractsDir, "addresses.json");
    try {
        const addressesData = {
            pointsAddress,
            nftAddress,
            updatedAt: new Date().toISOString()
        };
        fs.writeFileSync(frontendAddressesPath, JSON.stringify(addressesData, null, 2) + "\n", "utf-8");
        console.log(" [Frontend] Updated src/lib/contracts/addresses.json");
    } catch (err) {
        console.warn("  [Frontend] Could not write addresses.json:", err.message);
    }

    const nftServicePath = path.resolve(__dirname, "../../DteamFrontend/dteam-app/src/lib/services/nftService.ts");
    if (fs.existsSync(nftServicePath)) {
        try {
            let content = fs.readFileSync(nftServicePath, "utf-8");
            content = content.replace(
                /export const DTEAM_NFT_CONTRACT_ADDRESS = ['"][^'"]+['"];/,
                `export const DTEAM_NFT_CONTRACT_ADDRESS = '${nftAddress}';`
            );
            fs.writeFileSync(nftServicePath, content, "utf-8");
            console.log(" [Frontend] Updated DTEAM_NFT_CONTRACT_ADDRESS in nftService.ts");
        } catch (err) {
            console.warn("  [Frontend] Could not update nftService.ts:", err.message);
        }
    }

    const blockchainServicePath = path.resolve(__dirname, "../../DteamFrontend/dteam-app/src/lib/services/blockchainService.ts");
    if (fs.existsSync(blockchainServicePath)) {
        try {
            let content = fs.readFileSync(blockchainServicePath, "utf-8");
            content = content.replace(
                /export const DTEAM_POINTS_CONTRACT_ADDRESS = ['"][^'"]+['"];/,
                `export const DTEAM_POINTS_CONTRACT_ADDRESS = '${pointsAddress}';`
            );
            fs.writeFileSync(blockchainServicePath, content, "utf-8");
            console.log(" [Frontend] Updated DTEAM_POINTS_CONTRACT_ADDRESS in blockchainService.ts");
        } catch (err) {
            console.warn("  [Frontend] Could not update blockchainService.ts:", err.message);
        }
    }
    console.log("------------------------------------------------------\n");
}

async function main() {
    const { ethers } = await network.create();
    const [deployer] = await ethers.getSigners();
    console.log("Deploying contracts with account:", deployer.address);

    const DteamPoints = await ethers.getContractFactory("DteamPoints");
    const points = await DteamPoints.deploy();
    await points.waitForDeployment();
    const pointsAddress = await points.getAddress();
    console.log("DteamPoints deployed to:", pointsAddress);

    const mintTx = await points.mint(deployer.address, ethers.parseEther("1000000000"));
    await mintTx.wait();
    console.log("Minted 1,000,000,000 DTP to:", deployer.address);

    const DNFT = await ethers.getContractFactory("DNFT");
    const nft = await DNFT.deploy();
    await nft.waitForDeployment();
    const nftAddress = await nft.getAddress();
    console.log("DNFT deployed to:", nftAddress);

    console.log("\n=================================");
    console.log(" Dteam Contracts Deployed Successfully!");
    console.log(" DteamPoints (DTP):", pointsAddress);
    console.log(" DNFT (Badges):    ", nftAddress);
    console.log("=================================");

    syncAddresses(pointsAddress, nftAddress);
}

main().catch((error) => {
    console.error(error);
    process.exitCode = 1;
});
