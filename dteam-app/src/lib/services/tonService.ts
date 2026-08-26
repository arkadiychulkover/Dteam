import { tonToNanoTon } from '../utils/formatters';

export const tonService = {
  async connectWallet(): Promise<string> {
    await new Promise((res) => setTimeout(res, 500));
    return 'EQBvW8Z5huBkMJYdn3PBRnVDLyTO2_OTHTuP4asMb_Fton';
  },

  async mockSendTransaction(amountNanoTon: bigint, recipientAddress: string): Promise<string> {
    await new Promise((res) => setTimeout(res, 800));
    return '0x' + Array.from({ length: 32 }, () => Math.floor(Math.random() * 16).toString(16)).join('');
  },
};
