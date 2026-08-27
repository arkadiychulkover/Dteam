import { api } from './api';
import type { 
  DepositAddressResponse, 
  VerifyTransactionRequest, 
  PaymentVerificationResult, 
  TransactionHistoryItem 
} from '../types/payment';

export const paymentService = {
  async getDepositAddress(): Promise<string> {
    try {
      const res = await api.get<DepositAddressResponse>('/payment/deposit-address');
      return res.address;
    } catch (e) {
      console.warn('[paymentService] Failed to fetch deposit address, using fallback:', e);
      return 'UQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJKZ';
    }
  },

  async verifyTransaction(txhHash: string, amount: number): Promise<PaymentVerificationResult> {
    const payload: VerifyTransactionRequest = {
      txhHash: txhHash.trim(),
      amount: Number(amount)
    };
    return await api.post<PaymentVerificationResult>('/payment/verify', payload);
  },

  async getTransactionHistory(): Promise<TransactionHistoryItem[]> {
    return await api.get<TransactionHistoryItem[]>('/payment/history');
  }
};
