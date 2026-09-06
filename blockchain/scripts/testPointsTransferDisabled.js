import { network } from "hardhat";
import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function main() {
    const { ethers } = await network.create();
    const [owner, user1, user2] = await ethers.getSigners();

    const addressesPath = path.resolve(__dirname, "../../DteamFrontend/dteam-app/src/lib/contracts/addresses.json");
    const { pointsAddress } = JSON.parse(fs.readFileSync(addressesPath, "utf-8"));

    const DteamPoints = await ethers.getContractFactory("DteamPoints");
    const points = DteamPoints.attach(pointsAddress);

    console.log("1. Minting 100 DTP to user1:", user1.address);
    const mintTx = await points.mint(user1.address, ethers.parseEther("100"));
    await mintTx.wait();
    console.log(" Minted successfully!");

    console.log("2. Attempting transfer from user1 to user2 (should FAIL)...");
    try {
        const user1Contract = points.connect(user1);
        const tx = await user1Contract.transfer(user2.address, ethers.parseEther("10"));
        await tx.wait();
        console.error("❌ ERROR: Transfer succeeded, but should have failed!");
        process.exit(1);
    } catch (err) {
        if (err.message.includes("DteamPoints: transfers are disabled")) {
            console.log("✅ SUCCESS: Transfer was blocked by contract with message: 'DteamPoints: transfers are disabled'!");
        } else {
            console.log("✅ SUCCESS: Transfer was blocked (reverted):", err.message);
        }
    }
}

main().catch((err) => {
    console.error(err);
    process.exit(1);
});

