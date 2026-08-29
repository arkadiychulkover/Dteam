export interface DailyMetricPoint {
  date: string;
  downloads: number;
  earningsInTon: number;
}

export interface DeveloperStats {
  totalEarningsInNanoTons: number;
  totalDownloads: number;
  totalGames: number;
  averageRating: number;
  totalReviews: number;
  earningsLast30DaysInNanoTons?: number;
  earningsLast7DaysInNanoTons?: number;
  earningsLast24HoursInNanoTons?: number;
  downloadsLast30Days?: number;
  downloadsLast7Days?: number;
  downloadsLast24Hours?: number;
  dailyDynamics?: DailyMetricPoint[];
}

export interface CreateDeveloperGameRequest {
  title: string;
  description: string;
  shortDescription?: string;
  priceInNanoTons: number;
  discountPercentage?: number;
  serverArchivePath?: string;
  isDlc?: boolean;
  parentGameId?: string;
  genres?: string[];
  platforms?: string[];
  features?: string[];
  tags?: string[];
  version?: string;
  sizeInBytes?: number;
  isPublished?: boolean;
  headerImageUrl?: string;
  coverImageUrl?: string;
  screenshotUrls?: string[];
  trailerUrl?: string;
}

export interface UpdateDeveloperGameRequest {
  title?: string;
  description?: string;
  shortDescription?: string;
  priceInNanoTons?: number;
  discountPercentage?: number;
  serverArchivePath?: string;
  isDlc?: boolean;
  parentGameId?: string;
  genres?: string[];
  platforms?: string[];
  features?: string[];
  tags?: string[];
  version?: string;
  sizeInBytes?: number;
  isPublished?: boolean;
  headerImageUrl?: string;
  coverImageUrl?: string;
  screenshotUrls?: string[];
  trailerUrl?: string;
}
