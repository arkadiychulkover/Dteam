import { BrowserProvider, Contract, FetchRequest, formatEther, JsonRpcProvider } from 'ethers';
import { DTEAM_POINTS_ABI } from '../contracts/DteamPointsAbi';

export const DTEAM_POINTS_CONTRACT_ADDRESS = '0x5FbDB2315678afecb367f032d93F642f64180aa3';
export const HARDHAT_RPC_URL =
  import.meta.env.VITE_HARDHAT_RPC_URL ||
  'https://goldmine-unloved-capsule.ngrok-free.dev';

export { DTEAM_POINTS_ABI };

/** Create a JsonRpcProvider that bypasses ngrok's browser-warning interstitial */
function createHardhatProvider(): JsonRpcProvider {
  const fetchReq = new FetchRequest(HARDHAT_RPC_URL);
  fetchReq.setHeader('ngrok-skip-browser-warning', 'true');
  return new JsonRpcProvider(fetchReq);
}

export async function getBalanceDirectFromBlockchain(walletAddress: string): Promise<number> {
  if (!walletAddress) return 0;

  if (typeof window !== 'undefined' && (window as any).ethereum) {
    try {
      const browserProvider = new BrowserProvider((window as any).ethereum);
      const contract = new Contract(DTEAM_POINTS_CONTRACT_ADDRESS, DTEAM_POINTS_ABI as any, browserProvider);
      const balanceWei = await contract.balanceOf(walletAddress);
      return Number(formatEther(balanceWei));
    } catch (err) {
      console.warn('[Blockchain] MetaMask Provider call failed (possibly network mismatch), falling back to Hardhat RPC:', err);
    }
  }

  const rpcProvider = createHardhatProvider();
  const contract = new Contract(DTEAM_POINTS_CONTRACT_ADDRESS, DTEAM_POINTS_ABI as any, rpcProvider);
  const balanceWei = await contract.balanceOf(walletAddress);
  return Number(formatEther(balanceWei));
}

