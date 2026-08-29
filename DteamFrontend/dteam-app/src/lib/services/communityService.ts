import { api } from './api';

export interface CommunityPost {
  id: string;
  gameId: string;
  author: {
    id: string;
    username: string;
    avatarUrl: string;
  };
  createdAt: string;
  category: 'all' | 'forum' | 'screenshots' | 'videos' | 'guides' | 'news';
  title: string;
  content: string;
  media: {
    type: 'image' | 'video' | 'none';
    url: string;
    thumbnailUrl?: string;
  };
  stats: {
    likesCount: number;
    commentsCount: number;
    isLiked: boolean;
  };
}

export interface CommunityComment {
  id: string;
  postId: string;
  author: {
    id: string;
    username: string;
    avatarUrl: string;
  };
  createdAt: string;
  content: string;
  likesCount: number;
  isLiked: boolean;
  replies: {
    id: string;
    author: {
      id: string;
      username: string;
      avatarUrl: string;
    };
    createdAt: string;
    content: string;
  }[];
}

export interface GetPostsResponse {
  gameTitle: string;
  subscribersCount: number;
  onlineCount: number;
  posts: CommunityPost[];
}

export interface GetPostDetailsResponse {
  post: CommunityPost;
  comments: CommunityComment[];
}

export const communityService = {
  getPosts: async (gameId: string | null, category = 'all', search = '', sortBy = 'newest'): Promise<GetPostsResponse> => {
    const params = new URLSearchParams({ category, search, sortBy });
    // Убрали дублирующий /api во всех путях
    const url = gameId
      ? `community/${gameId}/posts?${params.toString()}`
      : `community/posts?${params.toString()}`;
    return await api.get<GetPostsResponse>(url);
  },

  getPostDetails: async (postId: string): Promise<GetPostDetailsResponse> => {
    return await api.get<GetPostDetailsResponse>(`community/posts/${postId}`);
  },

  createPost: async (gameId: string | null, post: { category: string; title: string; content: string; mediaType: string; mediaUrl: string; mediaThumbnailUrl?: string }): Promise<CommunityPost> => {
    // Вызываем с /community/... вместо /api/community/...
    if (gameId) {
      return await api.post<CommunityPost>(`community/${gameId}/posts`, post);
    }
    return await api.post<CommunityPost>(`community/posts`, post);
  },

  toggleLikePost: async (postId: string): Promise<{ liked: boolean; likesCount: number }> => {
    return await api.post<{ liked: boolean; likesCount: number }>(`community/posts/${postId}/like`);
  },

  addComment: async (postId: string, content: string): Promise<CommunityComment> => {
    return await api.post<CommunityComment>(`community/posts/${postId}/comments`, { content });
  },

  addReply: async (commentId: string, content: string): Promise<any> => {
    return await api.post<any>(`community/comments/${commentId}/reply`, { content });
  }
};
