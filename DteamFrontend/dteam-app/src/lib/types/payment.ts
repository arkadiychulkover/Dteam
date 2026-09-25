export interface DepositAddressResponse {
  address: string;
}

export interface VerifyTransactionRequest {
  txhHash: string;
  amount: number;
}

export interface PaymentVerificationResult {
  success: boolean;
  message: string;
  newBalanceInNanoTons: number;
  amount: number;
  txhHash: string;
}

export interface TransactionHistoryItem {
  id: string;
  txhHash?: string | null;
  amount: number;
  userId?: string;
  createdAt: string;
}

export type TransactionType = 'deposit' | 'purchase' | 'reward' | 'payout';

export interface UnifiedTransactionItem {
  id: string;
  type: TransactionType;
  title: string;
  amountTon: number;
  amountNanoTon: number;
  txHash?: string | null;
  status: 'completed' | 'pending' | 'failed';
  createdAt: string;
  gameCoverUrl?: string | null;
  gameId?: string | null;
}
