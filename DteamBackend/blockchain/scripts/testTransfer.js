import { network } from "hardhat";

async function main() {
    const { ethers } = await network.create();
    const contractAddress = "0xDc64a140Aa3E981100a9becA4E685f962f0cF6C9";
    const [admin, user2] = await ethers.getSigners();

    console.log(`Transferring token 1 from ${admin.address} to ${user2.address}...`);

    const DNFT = await ethers.getContractFactory("DNFT");
    const nft = DNFT.attach(contractAddress);

    const tx = await nft["safeTransferFrom(address,address,uint256)"](admin.address, user2.address, 1);
    const receipt = await tx.wait();
    console.log("Transfer confirmed! Tx:", receipt.hash);
}

main().catch((err) => {
    console.error(err);
    process.exit(1);
});
