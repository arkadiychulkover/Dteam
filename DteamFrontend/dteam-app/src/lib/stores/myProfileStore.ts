import { writable, get } from 'svelte/store';
import { userService, type MyReview } from '../services/userService';
import { communityService, type CommunityPost } from '../services/communityService';
import { uiStore } from './uiStore';
import { currentUser } from './authStore';

function createMyProfileStore() {
  const { subscribe, update, set } = writable<{
    posts: CommunityPost[];
    reviews: MyReview[];
    isLoadingPosts: boolean;
    isLoadingReviews: boolean;
  }>({
    posts: [],
    reviews: [],
    isLoadingPosts: false,
    isLoadingReviews: false,
  });

  return {
    subscribe,

    viewMyProfile: () => {
      uiStore.setTab('my-profile');
      loadMyPosts();
      loadMyReviews();
    },

    reload: () => {
      loadMyPosts();
      loadMyReviews();
    },
  };

  async function loadMyPosts() {
    const me = get(currentUser);
    if (!me?.id) return;
    update((s) => ({ ...s, isLoadingPosts: true }));
    try {
      const res = await communityService.getPosts(null as any, 'all');
      const mine = res.posts.filter((p) => p.author.id === me.id);
      update((s) => ({ ...s, posts: mine, isLoadingPosts: false }));
    } catch (e) {
      console.warn('[myProfileStore] Не вдалося завантажити пости:', e);
      update((s) => ({ ...s, isLoadingPosts: false }));
    }
  }

  async function loadMyReviews() {
    update((s) => ({ ...s, isLoadingReviews: true }));
    try {
      const reviews = await userService.getMyReviews();
      update((s) => ({ ...s, reviews: reviews || [], isLoadingReviews: false }));
    } catch (e) {
      console.warn('[myProfileStore] Не вдалося завантажити рецензії:', e);
      update((s) => ({ ...s, isLoadingReviews: false }));
    }
  }
}

export const myProfileStore = createMyProfileStore();
