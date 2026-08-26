import { writable, derived } from 'svelte/store';
import type { Game } from '../types';
import { gamesService } from '../services/gamesService';

const SAMPLE_GAMES: Game[] = [
  {
    id: '1',
    title: 'Neon Odyssey 2088',
    description: 'An open-world cyberpunk RPG powered by decentralized blockchain economy. Explore the futuristic Megacity Neo-Tokyo, hack corporate networks, and trade rare in-game cyberware.',
    shortDescription: 'High-octane cyberpunk action RPG with next-gen raytracing and Web3 trading.',
    priceInNanoTons: 4_500_000_000, // 4.5 TON
    serverArchivePath: '/storage/games/neon-odyssey-v1.2.zip',
    ownerId: 'dev-1',
    downloadCount: 14200,
    averageRating: 4.8,
    reviewsCount: 389,
    isDlc: false,
    version: '1.2.0',
    sizeInBytes: 25_800_000_000, // ~25.8 GB
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [
      'https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80',
      'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80',
      'https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=800&auto=format&fit=crop&q=80',
    ],
    trailerUrl: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
    tags: ['Cyberpunk', 'Action', 'RPG', 'Sci-Fi'],
    createdAt: '2026-01-15T10:00:00Z',
  },
  {
    id: '2',
    title: 'Aetheria: Realm of Gods',
    description: 'Immerse yourself in a vast fantasy universe where heroes battle ancient mythical beasts, build guild fortresses, and forge legendary artifacts on-chain.',
    shortDescription: 'Epic MMORPG with dynamic open world and player-driven economy.',
    priceInNanoTons: 6_000_000_000, // 6.0 TON
    serverArchivePath: '/storage/games/aetheria-release.zip',
    ownerId: 'dev-2',
    downloadCount: 28900,
    averageRating: 4.9,
    reviewsCount: 812,
    isDlc: false,
    version: '2.0.1',
    sizeInBytes: 42_000_000_000,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [
      'https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80',
    ],
    tags: ['RPG', 'Multiplayer', 'Strategy'],
    createdAt: '2026-02-01T12:00:00Z',
  },
  {
    id: '3',
    title: 'Quantum Drift 0G',
    description: 'Zero gravity anti-grav racing at breakneck supersonic speeds across orbital platforms and neon-lit asteroid belts.',
    shortDescription: 'Adrenaline-packed supersonic anti-gravity racing tournament.',
    priceInNanoTons: 0, // Free to Play
    serverArchivePath: '/storage/games/quantum-drift.zip',
    ownerId: 'dev-1',
    downloadCount: 45100,
    averageRating: 4.6,
    reviewsCount: 520,
    isDlc: false,
    version: '1.0.4',
    sizeInBytes: 8_500_000_000,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1552824796-a831bfb66782?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1552824796-a831bfb66782?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [
      'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80',
    ],
    tags: ['Action', 'Indie', 'Sci-Fi'],
    createdAt: '2026-03-10T14:30:00Z',
  },
  {
    id: '4',
    title: 'Stellar Protocol: Command',
    description: 'Deep 4X space tactical strategy simulator. Expand star systems, research hyper-tech, and lead galactic fleets in tactical real-time warfare.',
    shortDescription: 'Galactic 4X tactical fleet strategy game with deep diplomacy.',
    priceInNanoTons: 3_200_000_000, // 3.2 TON
    serverArchivePath: '/storage/games/stellar-protocol.zip',
    ownerId: 'dev-3',
    downloadCount: 9300,
    averageRating: 4.7,
    reviewsCount: 194,
    isDlc: false,
    version: '1.1.0',
    sizeInBytes: 14_200_000_000,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['Strategy', 'Sci-Fi', 'Web3 / Strategy'],
    createdAt: '2026-04-05T09:00:00Z',
  },
];

function createGamesStore() {
  const { subscribe, set, update } = writable<{
    games: Game[];
    selectedGame: Game | null;
    searchQuery: string;
    selectedCategory: string;
    isLoading: boolean;
  }>({
    games: SAMPLE_GAMES,
    selectedGame: null,
    searchQuery: '',
    selectedCategory: 'All Games',
    isLoading: false,
  });

  return {
    subscribe,
    setCategory: (category: string) => update((s) => ({ ...s, selectedCategory: category })),
    setSearchQuery: (query: string) => update((s) => ({ ...s, searchQuery: query })),
    selectGame: (game: Game | null) => update((s) => ({ ...s, selectedGame: game })),
    loadGames: async () => {
      update((s) => ({ ...s, isLoading: true }));
      try {
        const fetched = await gamesService.getGames();
        if (fetched && fetched.length > 0) {
          update((s) => ({ ...s, games: fetched, isLoading: false }));
        } else {
          update((s) => ({ ...s, isLoading: false }));
        }
      } catch {
        update((s) => ({ ...s, isLoading: false }));
      }
    },
  };
}

export const gamesStore = createGamesStore();

export const filteredGames = derived(gamesStore, ($s) => {
  return $s.games.filter((g) => {
    const matchesCategory =
      $s.selectedCategory === 'All Games' ||
      (g.tags && g.tags.includes($s.selectedCategory));
    const matchesSearch =
      !$s.searchQuery ||
      g.title.toLowerCase().includes($s.searchQuery.toLowerCase()) ||
      g.description.toLowerCase().includes($s.searchQuery.toLowerCase());
    return matchesCategory && matchesSearch;
  });
});
