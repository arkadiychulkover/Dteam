import { writable } from 'svelte/store';

export interface NewsArticle {
  id: string;
  gameTag: string;
  headline: string;
  summary: string;
  bannerUrl: string;
  likes: number;
  comments: number;
  date: string;
}

export interface CommunityHighlight {
  id: string;
  authorName: string;
  authorAvatar: string;
  mediaType: 'image' | 'video';
  mediaUrl: string;
  caption: string;
  likes: number;
  comments: number;
  date: string;
}

const SAMPLE_NEWS: NewsArticle[] = [
  {
    id: 'news-1',
    gameTag: 'Fallout 4',
    headline: 'Велике оновлення 1.5: Нові локації та події',
    summary: 'Досліджуйте розширені запущені зони та отримайте унікальні комплекти спорядження.',
    bannerUrl: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=800&auto=format&fit=crop&q=80',
    likes: 2500,
    comments: 420,
    date: '25 Серпня 2026',
  },
  {
    id: 'news-2',
    gameTag: 'The Witcher 3',
    headline: 'Турнір Спільноти: Велике полювання на чудовиськ',
    summary: 'Приєднуйтесь до щотижневих випробувань з нагородами у TON.',
    bannerUrl: 'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80',
    likes: 3100,
    comments: 680,
    date: '24 Серпня 2026',
  },
  {
    id: 'news-3',
    gameTag: "No Man's Sky",
    headline: 'Експедиція "Орбітальний Кордон"',
    summary: 'Нові космічні станції, кастомізація зорельотів та групові місії.',
    bannerUrl: 'https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?w=800&auto=format&fit=crop&q=80',
    likes: 1950,
    comments: 310,
    date: '22 Серпня 2026',
  },
];

const SAMPLE_HIGHLIGHTS: CommunityHighlight[] = [
  {
    id: 'hl-1',
    authorName: 'NikaNii',
    authorAvatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&auto=format&fit=crop&q=80',
    mediaType: 'image',
    mediaUrl: 'https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80',
    caption: 'Неймовірний світанок над новим астероїдом! 🚀✨ #NoMansSky #DTEAM',
    likes: 2500,
    comments: 142,
    date: '25 Серпня 2026',
  },
  {
    id: 'hl-2',
    authorName: 'CyberViper',
    authorAvatar: 'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=150&auto=format&fit=crop&q=80',
    mediaType: 'video',
    mediaUrl: 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80',
    caption: 'Финальний бос пройдений без ушкоджень! Оцініть таймінги ⚔️',
    likes: 3400,
    comments: 290,
    date: '24 Серпня 2026',
  },
  {
    id: 'hl-3',
    authorName: 'AstraWalker',
    authorAvatar: 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150&auto=format&fit=crop&q=80',
    mediaType: 'image',
    mediaUrl: 'https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=800&auto=format&fit=crop&q=80',
    caption: 'Моя нова неонова база готова до прийому гостей у Neocity',
    likes: 1820,
    comments: 95,
    date: '23 Серпня 2026',
  },
];

export const mockNews = SAMPLE_NEWS;
export const mockCommunityHighlights = SAMPLE_HIGHLIGHTS;
