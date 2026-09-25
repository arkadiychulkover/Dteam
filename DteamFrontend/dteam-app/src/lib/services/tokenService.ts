import { api } from './api';

export interface VerifyWalletResponse {
  isMatch: boolean;
  registeredAddress?: string | null;
  providedAddress: string;
  message: string;
}

export interface BalanceResponse {
  address: string;
  balance: number;
}

export const tokenService = {
  async verifyWallet(walletAddress: string, userId?: string): Promise<VerifyWalletResponse> {
    return api.post<VerifyWalletResponse>('/token/verify-wallet', { walletAddress, userId });
  },

  async getBalance(address: string): Promise<number> {
    const res = await api.get<BalanceResponse>(`/token/balance/${address}`);
    return res.balance;
  }
};
