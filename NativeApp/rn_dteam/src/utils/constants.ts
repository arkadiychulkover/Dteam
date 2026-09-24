/**
 * Dteam Mobile App - Backend and API Constants
 */

export const BACKEND_URL = 'https://dteam-backend-production.up.railway.app';
export const API_BASE_URL = `${BACKEND_URL}/api`;

export const GENRES = [
  'Всі ігри',
  'RPG',
  'Action',
  'Strategy',
  'Cyberpunk',
  'Adventure',
  'Fantasy',
  'Indie',
  'Multiplayer',
  'Sci-Fi',
] as const;

/**
 * Resolves static media URLs from backend or external CDNs safely.
 * Complies with Dteam static media handling rules.
 */
export function resolveMediaUrl(url?: string | null): string | null {
  if (!url) return null;
  const trimmed = url.trim();
  if (!trimmed) return null;

  if (
    trimmed.startsWith('http://') ||
    trimmed.startsWith('https://') ||
    trimmed.startsWith('blob:') ||
    trimmed.startsWith('data:')
  ) {
    return trimmed;
  }

  const cleanPath = trimmed.startsWith('/') ? trimmed : `/${trimmed}`;
  return `${BACKEND_URL.replace(/\/+$/, '')}${cleanPath}`;
}
