import { writable, derived } from 'svelte/store';
import type { Game } from '../types';
import { gamesService } from '../services/gamesService';

const SAMPLE_GAMES: Game[] = [
  {
    id: '1',
    title: 'No Man\'s Sky',
    description: 'No Man\'s Sky це гра про дослідження та виживання у нескінченному процедурно згенерованому всесвіті.',
    shortDescription: 'Нескінченний космос, дослідження планет та будівництво баз.',
    priceInNanoTons: 4_500_000_000,
    serverArchivePath: '/storage/games/nms.zip',
    ownerId: 'dev-1',
    downloadCount: 14200,
    averageRating: 4.8,
    reviewsCount: 389,
    isDlc: false,
    version: '4.5.0',
    sizeInBytes: 10_737_418_240, // 10 GB
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['Sci-Fi', 'Action', 'RPG'],
    createdAt: '2026-01-15T10:00:00Z',
  },
  {
    id: '2',
    title: 'The Witcher 3: Wild Hunt',
    description: 'Ви — Ґеральт з Рівії, найманий мисливець на монстрів. У вашому розпорядженні нищівна зброя та бойова магія.',
    shortDescription: 'Культова фентезійна RPG з величезним відкритим світом.',
    priceInNanoTons: 6_000_000_000,
    serverArchivePath: '/storage/games/witcher3.zip',
    ownerId: 'dev-2',
    downloadCount: 28900,
    averageRating: 4.9,
    reviewsCount: 812,
    isDlc: false,
    version: '4.0.4',
    sizeInBytes: 53_687_091_200,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['RPG', 'Action'],
    createdAt: '2026-02-01T12:00:00Z',
  },
  {
    id: '3',
    title: 'Lords of the Manor',
    description: 'Середньовічний містобудівний стратегічний симулятор з тактичними битвами та глибокою економікою.',
    shortDescription: 'Будуйте власне середньовічне замок-місто.',
    priceInNanoTons: 3_500_000_000,
    serverArchivePath: '/storage/games/lords.zip',
    ownerId: 'dev-3',
    downloadCount: 8900,
    averageRating: 4.6,
    reviewsCount: 210,
    isDlc: false,
    version: '1.0.2',
    sizeInBytes: 16_106_127_360,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1579783902614-a3fb3927b675?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1579783902614-a3fb3927b675?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['Strategy', 'Indie'],
    createdAt: '2026-03-01T10:00:00Z',
  },
  {
    id: '4',
    title: 'Sekiro: Shadows Die Twice',
    description: 'Хардкорний екшн від FromSoftware у сеттінгу феодальної Японії.',
    shortDescription: 'Шлях однорукого воїна-сінобі.',
    priceInNanoTons: 5_000_000_000,
    serverArchivePath: '/storage/games/sekiro.zip',
    ownerId: 'dev-4',
    downloadCount: 19500,
    averageRating: 4.9,
    reviewsCount: 650,
    isDlc: false,
    version: '1.0.6',
    sizeInBytes: 26_843_545_600,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1578632767115-351597cf2477?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1578632767115-351597cf2477?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['Action', 'RPG'],
    createdAt: '2026-03-10T14:00:00Z',
  },
  {
    id: '5',
    title: 'Dark Souls III',
    description: 'Похмура атмосфера та виклики у класичному темному фентезі.',
    shortDescription: 'Залишки згасаючого світу.',
    priceInNanoTons: 4_000_000_000,
    serverArchivePath: '/storage/games/ds3.zip',
    ownerId: 'dev-4',
    downloadCount: 32000,
    averageRating: 4.8,
    reviewsCount: 920,
    isDlc: false,
    version: '1.1.5',
    sizeInBytes: 21_474_836_480,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['RPG', 'Action'],
    createdAt: '2026-03-15T11:00:00Z',
  },
  {
    id: '6',
    title: 'Another Crab\'s Treasure',
    description: 'Підводна пригода краба-самітника у світі екологічного занепаду.',
    shortDescription: 'Підводний Soulslike з гумором.',
    priceInNanoTons: 2_500_000_000,
    serverArchivePath: '/storage/games/crab.zip',
    ownerId: 'dev-5',
    downloadCount: 7400,
    averageRating: 4.7,
    reviewsCount: 150,
    isDlc: false,
    version: '1.0.1',
    sizeInBytes: 5_368_709_120,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['Indie', 'Action'],
    createdAt: '2026-04-01T09:00:00Z',
  },
  {
    id: '7',
    title: 'Tunic',
    description: 'Ізометрична пригода про маленького лиса у великому світі таємниць.',
    shortDescription: 'Загадкова пригодницька гра.',
    priceInNanoTons: 2_000_000_000,
    serverArchivePath: '/storage/games/tunic.zip',
    ownerId: 'dev-6',
    downloadCount: 11200,
    averageRating: 4.8,
    reviewsCount: 290,
    isDlc: false,
    version: '1.0.3',
    sizeInBytes: 3_221_225_472,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['Indie', 'RPG'],
    createdAt: '2026-04-05T12:00:00Z',
  },
  {
    id: '8',
    title: 'Minecraft Dungeons',
    description: 'Екшн-пригода у стилі dungeoncrawler у всесвіті Minecraft.',
    shortDescription: 'Кооперативний підземельний екшн.',
    priceInNanoTons: 3_000_000_000,
    serverArchivePath: '/storage/games/mcd.zip',
    ownerId: 'dev-7',
    downloadCount: 25000,
    averageRating: 4.5,
    reviewsCount: 410,
    isDlc: false,
    version: '1.17.0',
    sizeInBytes: 6_442_450_944,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['Action', 'Multiplayer'],
    createdAt: '2026-04-10T15:00:00Z',
  },
  {
    id: '9',
    title: 'Cat Quest III',
    description: 'Піратська котяча пригода у відкритому котячому морі.',
    shortDescription: 'Весела RPG про кота-пірата.',
    priceInNanoTons: 1_800_000_000,
    serverArchivePath: '/storage/games/catquest3.zip',
    ownerId: 'dev-8',
    downloadCount: 6300,
    averageRating: 4.7,
    reviewsCount: 130,
    isDlc: false,
    version: '1.0.0',
    sizeInBytes: 2_147_483_648,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['Indie', 'RPG'],
    createdAt: '2026-04-12T16:00:00Z',
  },
  {
    id: '10',
    title: 'Fallout 4',
    description: 'Пост-апокаліптична RPG від Bethesda Game Studios.',
    shortDescription: 'Виживайте у пустці Співдружності.',
    priceInNanoTons: 4_000_000_000,
    serverArchivePath: '/storage/games/fallout4.zip',
    ownerId: 'dev-9',
    downloadCount: 48000,
    averageRating: 4.8,
    reviewsCount: 1200,
    isDlc: false,
    version: '1.10.40',
    sizeInBytes: 32_212_254_720,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1552824796-a831bfb66782?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1552824796-a831bfb66782?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['RPG', 'Action', 'Sci-Fi'],
    createdAt: '2026-04-15T18:00:00Z',
  },
  {
    id: '11',
    title: 'Cyberpunk 2077',
    description: 'Пригодницький екшн у відкритому світі Найт-Сіті.',
    shortDescription: 'Станьте кібернаймачником V.',
    priceInNanoTons: 7_000_000_000,
    serverArchivePath: '/storage/games/cp2077.zip',
    ownerId: 'dev-2',
    downloadCount: 52000,
    averageRating: 4.8,
    reviewsCount: 1540,
    isDlc: false,
    version: '2.12',
    sizeInBytes: 75_161_927_680,
    isPublished: true,
    headerImageUrl: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1200&auto=format&fit=crop&q=80',
    coverImageUrl: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600&auto=format&fit=crop&q=80',
    screenshotUrls: [],
    tags: ['Cyberpunk', 'RPG', 'Action'],
    createdAt: '2026-04-18T20:00:00Z',
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
