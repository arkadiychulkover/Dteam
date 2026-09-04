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
    headline: 'Fallout 4: Оновлення 1.10.40',
    summary: 'Досліджуйте розширені запущені зони, покращену продуктивність та нове спорядження у пустках Співдружності.',
    bannerUrl: 'https://images.unsplash.com/photo-1552824796-a831bfb66782?w=800&auto=format&fit=crop&q=80',
    likes: 2500,
    comments: 2500,
    date: '25.02.2024',
  },
  {
    id: 'news-2',
    gameTag: 'The Witcher 3',
    headline: 'Відьмак 3 REDkit вже доступний!',
    summary: 'Створюйте власні унікальні квести, локації та персонажів за допомогою офіційного інструментарію розробників.',
    bannerUrl: 'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80',
    likes: 3100,
    comments: 1800,
    date: '24.02.2024',
  },
  {
    id: 'news-3',
    gameTag: 'Dying Light',
    headline: 'Dying Light: Подія "Нічний Мисливець"',
    summary: 'Нові небезпечні зони висадки та унікальні креслення зброї для виживальників під час нічної варти.',
    bannerUrl: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=800&auto=format&fit=crop&q=80',
    likes: 1950,
    comments: 940,
    date: '22.02.2024',
  },
];

const SAMPLE_HIGHLIGHTS: CommunityHighlight[] = [
  {
    id: 'hl-1',
    authorName: 'NikaNii',
    authorAvatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&auto=format&fit=crop&q=80',
    mediaType: 'image',
    mediaUrl: 'https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?w=800&auto=format&fit=crop&q=80',
    caption: 'Неймовірний світанок над новим астероїдом у No Man\'s Sky! 🚀✨',
    likes: 2500,
    comments: 2500,
    date: '25.02.2024',
  },
  {
    id: 'hl-2',
    authorName: 'CyberViper',
    authorAvatar: 'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=150&auto=format&fit=crop&q=80',
    mediaType: 'video',
    mediaUrl: 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80',
    caption: 'Фінальний бос Sekiro пройдений без ушкоджень! Оцініть таймінги ⚔️',
    likes: 3400,
    comments: 1200,
    date: '24.02.2024',
  },
  {
    id: 'hl-3',
    authorName: 'AstraWalker',
    authorAvatar: 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150&auto=format&fit=crop&q=80',
    mediaType: 'image',
    mediaUrl: 'https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=800&auto=format&fit=crop&q=80',
    caption: 'Моя нова затишна середньовічна фортеця у Lords of the Manor',
    likes: 1820,
    comments: 650,
    date: '23.02.2024',
  },
];

export const mockNews = SAMPLE_NEWS;
export const mockCommunityHighlights = SAMPLE_HIGHLIGHTS;

