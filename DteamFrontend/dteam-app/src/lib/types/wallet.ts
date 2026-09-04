export interface WalletState {
  isConnected: boolean;
  address: string | null;
  balanceNanoTon: bigint;
  formattedBalance: string;
}

export interface TonTransaction {
  id: string;
  txHash: string;
  amountNanoTon: number | string;
  type: 'deposit' | 'withdrawal' | 'game_purchase' | 'game_payout';
  status: 'pending' | 'completed' | 'failed';
  timestamp: string;
  gameTitle?: string;
}

