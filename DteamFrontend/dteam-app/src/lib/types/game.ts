import type { Duser } from './user';

export interface ReviewParentInfo {
  id: string;
  userId: string;
  username: string;
  userAvatarUrl?: string | null;
  content: string;
}

export interface Review {
  id: string;
  parentReviewId?: string | null;
  userId: string;
  username?: string;
  userAvatarUrl?: string | null;
  user?: Duser;
  gameId: string;
  rating: number;
  content: string;
  isRecommended: boolean;
  playTimeHoursAtReview?: number;
  likesCount?: number;
  isLiked?: boolean;
  repliesCount?: number;
  replies?: Review[];
  parentReview?: ReviewParentInfo | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface GameLanguageSupport {
  language: string;
  interface: boolean;
  fullAudio: boolean;
  subtitles: boolean;
}

export interface Game {
  id: string;
  title: string;
  description: string;
  shortDescription?: string | null;
  priceInNanoTons: number | string;
  discountPercentage?: number;
  originalPriceInNanoTons?: number | string;
  serverArchivePath: string;
  ownerId: string;
  owner?: Duser;
  downloadCount: number;
  averageRating: number;
  reviewsCount: number;
  isDlc: boolean;
  parentGameId?: string | null;
  parentGameTitle?: string | null;
  parentGame?: Game | null;
  dlcs?: Game[];
  genres?: string[];
  platforms?: string[];
  features?: string[];
  supportedLanguages?: GameLanguageSupport[];
  tags?: string[];
  version: string;
  sizeInBytes: number;
  isPublished: boolean;
  headerImageUrl?: string | null;
  coverImageUrl?: string | null;
  screenshotUrls: string[];
  trailerUrl?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  reviews?: Review[];
}

export interface GameFilterParams {
  search?: string;
  genre?: string;
  isDlc?: boolean;
  isDiscounted?: boolean;
  isFree?: boolean;
  minPrice?: number;
  maxPrice?: number;
  platform?: string;
  feature?: string;
  tag?: string;
  sortBy?: 'relevance' | 'price_asc' | 'price_desc' | 'rating' | 'newest' | 'downloads';
}

export interface GameRecommendation {
  id: string;
  title: string;
  banner: string;
  bannerUrl: string;
  headerImageUrl?: string | null;
  coverImageUrl?: string | null;
}


