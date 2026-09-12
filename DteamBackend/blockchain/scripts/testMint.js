import { network } from "hardhat";

async function main() {
    const { ethers } = await network.create();
    const contractAddress = "0xDc64a140Aa3E981100a9becA4E685f962f0cF6C9";
    const recipient = "0xf39fd6e51aad88f6f4ce6ab8827279cfffb92266";

    const [signer] = await ethers.getSigners();
    console.log("Signer:", signer.address);

    const DNFT = await ethers.getContractFactory("DNFT");
    const nft = DNFT.attach(contractAddress);

    console.log("Minting DNFT to:", recipient);
    const tx = await nft.safeMint(recipient, "http://localhost:5117/nft/1_1_1.png");
    const receipt = await tx.wait();
    console.log("Mint successful! Tx:", receipt.hash);
}

main().catch((err) => {
    console.error(err);
    process.exit(1);
});
