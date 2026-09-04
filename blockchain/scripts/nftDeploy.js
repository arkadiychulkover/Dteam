import { network } from "hardhat";

async function main() {
    const { ethers } = await network.create();

    const DNFT = await ethers.getContractFactory("DNFT");
    const nft = await DNFT.deploy();
    await nft.waitForDeployment();

    console.log("DNFT deployed to:", await nft.getAddress());
}

main().catch((error) => {
    console.error(error);
    process.exitCode = 1;
});