export const BACKEND_URL = (import.meta.env.VITE_BACKEND_URL as string)
  || (import.meta.env.VITE_API_URL ? (import.meta.env.VITE_API_URL as string).replace(/\/api\/?$/, '') : '')
  || 'https://dteam-backend-production.up.railway.app';

export const API_BASE_URL = (import.meta.env.VITE_API_URL as string)
  || `${BACKEND_URL.replace(/\/+$/, '')}/api`;

export const GENRES = [
  'All Games',
  'Action',
  'RPG',
  'Strategy',
  'Cyberpunk',
  'Indie',
  'Multiplayer',
  'Sci-Fi',
  'Web3 / Strategy',
];

export const MOCK_AVATARS = [
  'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150&auto=format&fit=crop&q=80',
  'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&auto=format&fit=crop&q=80',
  'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=150&auto=format&fit=crop&q=80',
  'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
];

export function resolveMediaUrl(url?: string | null): string | null {
  if (!url || !url.trim()) return null;
  const trimmed = url.trim();
  if (trimmed.startsWith('blob:') || trimmed.startsWith('data:')) return trimmed;

  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    if (BACKEND_URL && trimmed.startsWith(BACKEND_URL)) {
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
        return `${BACKEND_URL.replace(/\/+$/, '')}/api/media${path}`;
      } else if (path.startsWith('/avatar_') || path.startsWith('/avatars/')) {
        const cleanAvatar = path.startsWith('/avatars/') ? path.slice('/avatars'.length) : path;
        return `${BACKEND_URL.replace(/\/+$/, '')}/api/media/icons${cleanAvatar.startsWith('/') ? cleanAvatar : `/${cleanAvatar}`}`;
      }
    }
    return trimmed;
  }

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

  return BACKEND_URL ? `${BACKEND_URL.replace(/\/+$/, '')}${cleanPath}` : cleanPath;
}

