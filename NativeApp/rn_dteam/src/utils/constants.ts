import { tokenStorage } from '../services/tokenStorage';

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

export function resolveMediaUrl(url?: string | null): string | null {
  if (!url) return null;
  const trimmed = url.trim();
  if (!trimmed) return null;

  if (trimmed.startsWith('blob:') || trimmed.startsWith('data:')) {
    return trimmed;
  }

  let fullUrl: string;
  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    fullUrl = trimmed;
  } else {
    const cleanPath = trimmed.startsWith('/') ? trimmed : `/${trimmed}`;
    fullUrl = `${BACKEND_URL.replace(/\/+$/, '')}${cleanPath}`;
  }

  // Attach auth token to protected backend endpoints if not already attached
  if (
    (fullUrl.includes('/api/chat/media/') || fullUrl.includes('/api/chat/uploads/')) &&
    !fullUrl.includes('token=') &&
    !fullUrl.includes('access_token=')
  ) {
    const token = tokenStorage.getSyncToken();
    if (token) {
      const sep = fullUrl.includes('?') ? '&' : '?';
      fullUrl = `${fullUrl}${sep}token=${encodeURIComponent(token)}`;
    }
  }

  return fullUrl;
}
