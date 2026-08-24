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

export function getEffectivePrice(nanoTon: number | string | bigint, discountPercentage: number = 0): number {
  const baseTons = nanoTonToTon(nanoTon);
  if (baseTons === 0 || discountPercentage >= 100) return 0;
  if (discountPercentage <= 0) return baseTons;
  return baseTons * (1 - discountPercentage / 100);
}

export function formatPrice(nanoTon: number | string | bigint, discountPercentage: number = 0): string {
  const finalTons = getEffectivePrice(nanoTon, discountPercentage);
  if (finalTons === 0) return 'Безкоштовно';
  return `${finalTons.toLocaleString('uk-UA', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} TON`;
}

export function formatBasePrice(nanoTon: number | string | bigint): string {
  const baseTons = nanoTonToTon(nanoTon);
  if (baseTons === 0) return 'Безкоштовно';
  return `${baseTons.toLocaleString('uk-UA', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} TON`;
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
  if (minutes < 60) return `${minutes} хв`;
  const hours = (minutes / 60).toFixed(1);
  return `${hours} год`;
}

export function formatDate(isoDate: string): string {
  try {
    return new Date(isoDate).toLocaleDateString('uk-UA', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  } catch {
    return isoDate;
  }
}
