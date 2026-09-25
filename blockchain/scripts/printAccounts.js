import { ethers } from "ethers";

const mnemonic = "test test test test test test test test test test test junk";

console.log("\n=== HARDHAT DEFAULT TEST ACCOUNTS & PRIVATE KEYS ===");
for (let i = 0; i < 10; i++) {
  const path = `m/44'/60'/0'/0/${i}`;
  const wallet = ethers.HDNodeWallet.fromPhrase(mnemonic, undefined, path);
  console.log(`Account #${i}: ${wallet.address}`);
  console.log(`Private Key: ${wallet.privateKey}\n`);
}
