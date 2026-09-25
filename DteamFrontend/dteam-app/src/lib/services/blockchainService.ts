import { BrowserProvider, Contract, formatEther, JsonRpcProvider } from 'ethers';
import { DTEAM_POINTS_ABI } from '../contracts/DteamPointsAbi';

export const DTEAM_POINTS_CONTRACT_ADDRESS = '0x5FbDB2315678afecb367f032d93F642f64180aa3';
export const HARDHAT_RPC_URL =
  import.meta.env.VITE_HARDHAT_RPC_URL ||
  'https://dteam-backend-production.up.railway.app/api/rpc';

export { DTEAM_POINTS_ABI };

import { api } from './api';

export async function getBalanceDirectFromBlockchain(walletAddress: string, useMetaMask: boolean = false): Promise<number> {
  if (!walletAddress) return 0;

  if (useMetaMask && typeof window !== 'undefined' && (window as any).ethereum) {
    try {
      const browserProvider = new BrowserProvider((window as any).ethereum);
      const contract = new Contract(DTEAM_POINTS_CONTRACT_ADDRESS, DTEAM_POINTS_ABI as any, browserProvider);
      const balanceWei = await contract.balanceOf(walletAddress);
      return Number(formatEther(balanceWei));
    } catch (err) {
      console.warn('[Blockchain] MetaMask Provider call failed, falling back to Hardhat RPC:', err);
    }
  }

  try {
    const rpcProvider = new JsonRpcProvider(HARDHAT_RPC_URL);
    const contract = new Contract(DTEAM_POINTS_CONTRACT_ADDRESS, DTEAM_POINTS_ABI as any, rpcProvider);
    const balanceWei = await contract.balanceOf(walletAddress);
    return Number(formatEther(balanceWei));
  } catch (err) {
    console.warn('[Blockchain] Hardhat RPC call failed:', err);
    throw err;
  }
}

export async function getTdpBalanceHybrid(walletAddress?: string | null, isMetaMaskConnected: boolean = false): Promise<number> {
  if (!walletAddress) return 0;

  if (isMetaMaskConnected) {
    try {
      return await getBalanceDirectFromBlockchain(walletAddress, true);
    } catch (err) {
      console.warn('[TDP Balance] On-chain via MetaMask failed, falling back to Backend API:', err);
    }
  }

  try {
    const res = await api.get<{ balance: number }>(`/token/balance/${walletAddress}`);
    if (res && typeof res.balance === 'number') {
      return res.balance;
    }
  } catch (err) {
    console.warn('[TDP Balance] Backend API query failed, trying Hardhat RPC:', err);
  }

  try {
    return await getBalanceDirectFromBlockchain(walletAddress, false);
  } catch {
    return 0;
  }
}
