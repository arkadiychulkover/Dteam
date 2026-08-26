export const NANOTON_FACTOR = 1_000_000_000n;

export function nanoTonToTon(nanoTon: number | string | bigint): number {
  try {
    const bigVal = typeof nanoTon === 'bigint' ? nanoTon : BigInt(Math.floor(Number(nanoTon) || 0));
    return Number(bigVal) / 1_000_000_000;
  } catch {
    return 0;
  }
}

export function tonToNanoTon(ton: number): bigint {
  return BigInt(Math.floor(ton * 1_000_000_000));
}

export function formatPrice(nanoTon: number | string | bigint): string {
  const tons = nanoTonToTon(nanoTon);
  if (tons === 0) return 'Free to Play';
  return `${tons.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 4 })} TON`;
}

export function formatAddress(address?: string | null): string {
  if (!address) return '';
  if (address.length <= 10) return address;
  return `${address.slice(0, 4)}...${address.slice(-4)}`;
}

export function formatBytes(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
}

export function formatPlayTime(minutes: number): string {
  if (minutes < 60) return `${minutes} min`;
  const hours = (minutes / 60).toFixed(1);
  return `${hours} hrs`;
}

export function formatDate(isoDate: string): string {
  try {
    return new Date(isoDate).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  } catch {
    return isoDate;
  }
}
