import { network } from "hardhat";

async function main() {
    const { ethers } = await network.create();

    const DteamPoints = await ethers.getContractFactory("DteamPoints");
    const token = await DteamPoints.deploy();
    await token.waitForDeployment();

    console.log("DteamPoints deployed to:", await token.getAddress());

    const [deployer] = await ethers.getSigners();
    const tx = await token.mint(deployer.address, ethers.parseEther("1000000000"));
    await tx.wait();
    console.log("Minted 1000000000 DteamPoints to:", deployer.address);
}

main().catch((error) => {
    console.error(error);
    process.exitCode = 1;
});