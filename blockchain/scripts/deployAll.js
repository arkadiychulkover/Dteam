import { network } from "hardhat";

async function main() {
    const { ethers } = await network.create();
    const [deployer] = await ethers.getSigners();
    console.log("Deploying contracts with account:", deployer.address);

    // 1. Deploy DteamPoints (Nonce 0 -> 0x5FbDB2315678afecb367f032d93F642f64180aa3)
    const DteamPoints = await ethers.getContractFactory("DteamPoints");
    const points = await DteamPoints.deploy();
    await points.waitForDeployment();
    const pointsAddress = await points.getAddress();
    console.log("DteamPoints deployed to:", pointsAddress);

    // 2. Mint initial points to deployer (Nonce 1)
    const mintTx = await points.mint(deployer.address, ethers.parseEther("1000000000"));
    await mintTx.wait();
    console.log("Minted 1,000,000,000 DTP to:", deployer.address);

    // 3. Deploy DNFT (Nonce 2 -> 0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0)
    const DNFT = await ethers.getContractFactory("DNFT");
    const nft = await DNFT.deploy();
    await nft.waitForDeployment();
    const nftAddress = await nft.getAddress();
    console.log("DNFT deployed to:", nftAddress);

    console.log("\n=================================");
    console.log(" Dteam Contracts Deployed Successfully!");
    console.log(" DteamPoints (DTP):", pointsAddress);
    console.log(" DNFT (Badges):    ", nftAddress);
    console.log("=================================\n");
}

main().catch((error) => {
    console.error(error);
    process.exitCode = 1;
});
