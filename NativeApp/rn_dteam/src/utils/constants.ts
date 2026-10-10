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

  if (trimmed.startsWith('blob:') || trimmed.startsWith('data:')) { return trimmed; }

  let fullUrl: string;
  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    if (trimmed.startsWith(BACKEND_URL)) {
      const path = trimmed.slice(BACKEND_URL.length);
      if (
        path.startsWith('/icons/') ||
        path.startsWith('/community/') ||
        path.startsWith('/comunity/') ||
        path.startsWith('/uploads/') ||
        path.startsWith('/storage/') ||
        path.startsWith('/game_images/') ||
        path.startsWith('/nft/')
      ) {
        fullUrl = `${BACKEND_URL.replace(/\/+$/, '')}/api/media${path}`;
      } else if (path.startsWith('/avatar_') || path.startsWith('/avatars/')) {
        const cleanAvatar = path.startsWith('/avatars/') ? path.slice('/avatars'.length) : path;
        fullUrl = `${BACKEND_URL.replace(/\/+$/, '')}/api/media/icons${cleanAvatar.startsWith('/') ? cleanAvatar : `/${cleanAvatar}`}`;
      } else {
        fullUrl = trimmed;
      }
    } else {
      fullUrl = trimmed;
    }
  } else {
    let cleanPath = trimmed.startsWith('/') ? trimmed : `/${trimmed}`;
    if (
      cleanPath.startsWith('/icons/') ||
      cleanPath.startsWith('/community/') ||
      cleanPath.startsWith('/comunity/') ||
      cleanPath.startsWith('/uploads/') ||
      cleanPath.startsWith('/storage/') ||
      cleanPath.startsWith('/game_images/') ||
      cleanPath.startsWith('/nft/')
    ) {
      cleanPath = `/api/media${cleanPath}`;
    } else if (cleanPath.startsWith('/avatar_') || cleanPath.startsWith('/avatars/')) {
      const cleanAvatar = cleanPath.startsWith('/avatars/') ? cleanPath.slice('/avatars'.length) : cleanPath;
      cleanPath = `/api/media/icons${cleanAvatar.startsWith('/') ? cleanAvatar : `/${cleanAvatar}`}`;
    }
    fullUrl = `${BACKEND_URL.replace(/\/+$/, '')}${cleanPath}`;
  }

  
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
