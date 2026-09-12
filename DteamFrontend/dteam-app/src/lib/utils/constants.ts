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
