export const NANOTON_FACTOR = 1_000_000_000;

export function nanoTonToTon(nanoTon: number | string | bigint | undefined | null): number {
  if (nanoTon === undefined || nanoTon === null) return 0;
  try {
    const num = typeof nanoTon === 'number' ? nanoTon : Number(nanoTon);
    if (isNaN(num)) return 0;
    return num / NANOTON_FACTOR;
  } catch {
    return 0;
  }
}

export function formatTon(tons: number): string {
  return `${tons.toLocaleString('uk-UA', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} TON`;
}

export function getEffectivePrice(
  nanoTon: number | string | bigint | undefined | null,
  discountPercentage: number = 0
): number {
  const baseTons = nanoTonToTon(nanoTon);
  if (baseTons === 0 || discountPercentage >= 100) return 0;
  if (discountPercentage <= 0) return baseTons;
  return baseTons * (1 - discountPercentage / 100);
}

export function formatPrice(
  nanoTon: number | string | bigint | undefined | null,
  discountPercentage: number = 0
): string {
  const finalTons = getEffectivePrice(nanoTon, discountPercentage);
  if (finalTons === 0) return 'Безкоштовно';
  return formatTon(finalTons);
}

export function formatBasePrice(nanoTon: number | string | bigint | undefined | null): string {
  const baseTons = nanoTonToTon(nanoTon);
  if (baseTons === 0) return 'Безкоштовно';
  return formatTon(baseTons);
}

export function formatBytes(bytes: number): string {
  if (!bytes || bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i] || 'B'}`;
}

export function formatDate(isoDate?: string | null): string {
  if (!isoDate) return '';
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

export function formatPlayTime(minutes?: number | null): string {
  if (!minutes || minutes <= 0) return 'Ще не грали';
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  if (hours === 0) return `${mins} хв`;
  if (mins === 0) return `${hours} год`;
  return `${hours} год ${mins} хв`;
}

