export interface GameCollectionItem {
  gameId: string;
  title?: string;
  coverImageUrl?: string | null;
  addedAt?: string;
}

export interface GameCollection {
  id: string;
  userId?: string;
  name: string;
  description?: string | null;
  createdAt: string;
  updatedAt?: string;
  gamesCount?: number;
  items: GameCollectionItem[];
  games?: any[];
}

export interface CreateGameCollectionPayload {
  name: string;
  description?: string;
  gameIds?: string[];
  initialGameIds?: string[];
}

export interface UpdateGameCollectionPayload {
  name: string;
  description?: string;
}
