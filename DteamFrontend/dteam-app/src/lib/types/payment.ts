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
  txhHash: string;
  amount: number;
  userId?: string;
  createdAt: string;
}
