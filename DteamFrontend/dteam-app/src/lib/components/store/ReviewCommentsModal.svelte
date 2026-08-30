<script lang="ts">
  import { onMount } from 'svelte';
  import { 
    X, 
    Heart, 
    MessageSquare, 
    CornerDownRight, 
    MoreHorizontal, 
    Star, 
    Loader2, 
    Send,
    ChevronDown
  } from 'lucide-svelte';
  import { currentUser } from '../../stores/authStore';
  import { uiStore } from '../../stores/uiStore';
  import { gamesService } from '../../services/gamesService';
  import type { Review } from '../../types';

  interface Props {
    isOpen: boolean;
    gameId: string;
    review: Review | null;
    onClose: () => void;
    onReviewUpdated?: (updatedReview: Review) => void;
  }

  let { isOpen, gameId, review, onClose, onReviewUpdated }: Props = $props();

  let comments = $state<Review[]>([]);
  let isLoading = $state(false);
  let isSubmitting = $state(false);
  let commentText = $state('');
  let replyingTo = $state<Review | null>(null);
  let sortOrder = $state<'newest' | 'oldest'>('newest');
  let isSortDropdownOpen = $state(false);

  let rootReviewLikes = $state(0);
  let rootReviewIsLiked = $state(false);
  let rootReviewCommentsCount = $state(0);

  $effect(() => {
    if (isOpen && review && gameId) {
      rootReviewLikes = review.likesCount ?? 0;
      rootReviewIsLiked = review.isLiked ?? false;
      rootReviewCommentsCount = review.repliesCount ?? review.replies?.length ?? 0;
      loadComments();
    }
  });

  async function loadComments() {
    if (!review) return;
    isLoading = true;
    try {
      const details = await gamesService.getReviewDetails(gameId, review.id);
      comments = details.replies || [];
      rootReviewLikes = details.likesCount ?? rootReviewLikes;
      rootReviewIsLiked = details.isLiked ?? rootReviewIsLiked;
      rootReviewCommentsCount = details.repliesCount ?? comments.length;
    } catch (e) {
      console.warn('[ReviewCommentsModal] Failed to load comments:', e);
      comments = review.replies || [];
    } finally {
      isLoading = false;
    }
  }

  function formatDate(dateStr?: string): string {
    if (!dateStr) return '';
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('uk-UA', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
    } catch {
      return dateStr;
    }
  }

  function formatCount(num?: number): string {
    if (!num) return '0';
    if (num >= 1000) {
      return (num / 1000).toFixed(1).replace(/\.0$/, '') + 'k';
    }
    return num.toString();
  }

  let sortedComments = $derived(() => {
    const list = [...comments];
    if (sortOrder === 'newest') {
      list.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
    } else {
      list.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
    }
    return list;
  });

  async function handleToggleRootLike() {
    if (!$currentUser) {
      uiStore.addToast({
        title: 'Потрібна авторизація',
        message: 'Будь ласка, увійдіть в акаунт, щоб поставити вподобайку.',
        type: 'warning'
      });
      uiStore.setLoginModal(true);
      return;
    }
    if (!review) return;

    // Optimistic
    rootReviewIsLiked = !rootReviewIsLiked;
    rootReviewLikes += rootReviewIsLiked ? 1 : -1;

    try {
      const res = await gamesService.toggleReviewLike(gameId, review.id);
      rootReviewIsLiked = res.liked;
      rootReviewLikes = res.likesCount;
      if (onReviewUpdated && review) {
        onReviewUpdated({
          ...review,
          isLiked: rootReviewIsLiked,
          likesCount: rootReviewLikes,
          repliesCount: rootReviewCommentsCount
        });
      }
    } catch (e: any) {
      // Revert
      rootReviewIsLiked = !rootReviewIsLiked;
      rootReviewLikes += rootReviewIsLiked ? 1 : -1;
      uiStore.addToast({
        title: 'Помилка',
        message: e?.message || 'Не вдалося оновити вподобайку.',
        type: 'error'
      });
    }
  }

  async function handleToggleCommentLike(comment: Review) {
    if (!$currentUser) {
      uiStore.addToast({
        title: 'Потрібна авторизація',
        message: 'Будь ласка, увійдіть в акаунт, щоб поставити вподобайку.',
        type: 'warning'
      });
      uiStore.setLoginModal(true);
      return;
    }

    const wasLiked = comment.isLiked ?? false;
    const currentCount = comment.likesCount ?? 0;

    // Optimistic update
    comments = comments.map(c => {
      if (c.id === comment.id) {
        return {
          ...c,
          isLiked: !wasLiked,
          likesCount: wasLiked ? Math.max(0, currentCount - 1) : currentCount + 1
        };
      }
      return c;
    });

    try {
      const res = await gamesService.toggleReviewLike(gameId, comment.id);
      comments = comments.map(c => {
        if (c.id === comment.id) {
          return {
            ...c,
            isLiked: res.liked,
            likesCount: res.likesCount
          };
        }
        return c;
      });
    } catch (e: any) {
      // Revert
      comments = comments.map(c => {
        if (c.id === comment.id) {
          return {
            ...c,
            isLiked: wasLiked,
            likesCount: currentCount
          };
        }
        return c;
      });
      uiStore.addToast({
        title: 'Помилка',
        message: e?.message || 'Не вдалося оновити вподобайку.',
        type: 'error'
      });
    }
  }

  async function handleSubmitComment() {
    if (!$currentUser) {
      uiStore.addToast({
        title: 'Потрібна авторизація',
        message: 'Будь ласка, увійдіть в акаунт, щоб написати коментар.',
        type: 'warning'
      });
      uiStore.setLoginModal(true);
      return;
    }

    if (!commentText.trim() || !review) return;

    isSubmitting = true;
    const targetParent = replyingTo || review;

    try {
      const created = await gamesService.addReviewReply(gameId, targetParent.id, commentText.trim());
      comments = [created, ...comments];
      rootReviewCommentsCount++;
      commentText = '';
      replyingTo = null;

      if (onReviewUpdated && review) {
        onReviewUpdated({
          ...review,
          isLiked: rootReviewIsLiked,
          likesCount: rootReviewLikes,
          repliesCount: rootReviewCommentsCount
        });
      }

      uiStore.addToast({
        title: 'Коментар опубліковано',
        message: 'Ваш коментар успішно додано.',
        type: 'success'
      });
    } catch (e: any) {
      uiStore.addToast({
        title: 'Помилка',
        message: e?.message || 'Не вдалося опублікувати коментар.',
        type: 'error'
      });
    } finally {
      isSubmitting = false;
    }
  }
</script>

{#if isOpen && review}
  <div 
    class="fixed inset-0 bg-black/80 backdrop-blur-sm z-[110] flex items-center justify-center p-4 overflow-y-auto"
    onclick={(e) => { if (e.target === e.currentTarget) onClose(); }}
    onkeydown={(e) => { if (e.key === 'Escape') onClose(); }}
    role="dialog"
    aria-modal="true"
    tabindex="-1"
  >
    <div class="bg-[#061720] border border-cyan-500/30 rounded-3xl w-full max-w-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
      
      <!-- Modal Header -->
      <div class="px-6 py-4 border-b border-cyan-900/40 flex items-center justify-between bg-[#041017]/80">
        <h3 class="text-base font-extrabold text-white tracking-wide flex items-center gap-2">
          <span>Review - comments</span>
        </h3>
        <button
          type="button"
          onclick={onClose}
          class="w-8 h-8 rounded-full bg-slate-800/60 hover:bg-rose-500/20 text-slate-400 hover:text-rose-400 transition-colors flex items-center justify-center cursor-pointer"
          aria-label="Close modal"
        >
          <X class="w-4 h-4" />
        </button>
      </div>

      <!-- Modal Body (Scrollable) -->
      <div class="p-6 overflow-y-auto space-y-5 custom-scrollbar">

        <!-- 1. Main Review Card (exact layout from screenshot) -->
        <div class="bg-[#08222d] border border-cyan-500/30 rounded-2xl p-5 shadow-lg space-y-4">
          <!-- Author & Stars -->
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-full bg-[#030d12] border border-cyan-400/40 flex items-center justify-center text-cyan-300 font-bold overflow-hidden shrink-0">
              {#if review.userAvatarUrl || review.user?.avatarUrl}
                <img 
                  src={review.userAvatarUrl || review.user?.avatarUrl || ''} 
                  alt={review.username || review.user?.username || 'User'} 
                  class="w-full h-full object-cover" 
                />
              {:else}
                {(review.username || review.user?.username || 'U').charAt(0).toUpperCase()}
              {/if}
            </div>

            <div>
              <h4 class="text-sm font-bold text-white leading-tight">
                {review.username || review.user?.username || 'Юзернейм'}
              </h4>
              <div class="flex items-center gap-1 text-white mt-1">
                {#each [1, 2, 3, 4, 5] as star}
                  <Star class="w-3.5 h-3.5 {star <= review.rating ? 'fill-white text-white' : 'text-slate-600'}" />
                {/each}
              </div>
            </div>
          </div>

          <!-- Review Content -->
          <p class="text-xs text-slate-200 leading-relaxed whitespace-pre-wrap">
            {review.content}
          </p>

          <!-- Card Footer (Likes, Comments, Date) -->
          <div class="flex items-center justify-between pt-3 border-t border-cyan-900/30 text-xs text-slate-400">
            <div class="flex items-center gap-4">
              <button
                type="button"
                onclick={handleToggleRootLike}
                class="flex items-center gap-1.5 transition-colors cursor-pointer {rootReviewIsLiked ? 'text-rose-400' : 'hover:text-rose-300'}"
              >
                <Heart class="w-4 h-4 {rootReviewIsLiked ? 'fill-rose-400' : ''}" />
                <span class="font-medium text-xs">{formatCount(rootReviewLikes)}</span>
              </button>

              <span class="flex items-center gap-1.5 text-slate-400 font-medium">
                <MessageSquare class="w-4 h-4" />
                <span class="text-xs">{formatCount(rootReviewCommentsCount)}</span>
              </span>
            </div>

            <span class="text-[11px] text-slate-500 font-mono">
              {formatDate(review.createdAt)}
            </span>
          </div>
        </div>

        <!-- 2. Sorting and Input Bar -->
        <div class="space-y-2.5">
          <!-- Sorting Dropdown -->
          <div class="flex items-center justify-between text-xs">
            <div class="relative">
              <button
                type="button"
                onclick={() => isSortDropdownOpen = !isSortDropdownOpen}
                class="flex items-center gap-1.5 text-slate-400 hover:text-cyan-300 transition-colors font-medium cursor-pointer"
              >
                <span>Сортування:</span>
                <span class="text-cyan-400 font-semibold">{sortOrder === 'newest' ? 'Спочатку нові' : 'Спочатку старі'}</span>
                <ChevronDown class="w-3.5 h-3.5 text-cyan-400" />
              </button>

              {#if isSortDropdownOpen}
                <div class="absolute left-0 top-6 z-20 bg-[#071d27] border border-cyan-500/30 rounded-xl py-1 shadow-2xl min-w-[140px]">
                  <button
                    type="button"
                    onclick={() => { sortOrder = 'newest'; isSortDropdownOpen = false; }}
                    class="w-full text-left px-3 py-1.5 text-xs text-slate-300 hover:bg-cyan-950/60 hover:text-cyan-300 cursor-pointer {sortOrder === 'newest' ? 'text-cyan-400 font-bold' : ''}"
                  >
                    Спочатку нові
                  </button>
                  <button
                    type="button"
                    onclick={() => { sortOrder = 'oldest'; isSortDropdownOpen = false; }}
                    class="w-full text-left px-3 py-1.5 text-xs text-slate-300 hover:bg-cyan-950/60 hover:text-cyan-300 cursor-pointer {sortOrder === 'oldest' ? 'text-cyan-400 font-bold' : ''}"
                  >
                    Спочатку старі
                  </button>
                </div>
              {/if}
            </div>

            {#if replyingTo}
              <div class="flex items-center gap-1.5 bg-cyan-950/60 border border-cyan-500/30 px-2.5 py-0.5 rounded-full text-[11px] text-cyan-300">
                <span>Відповідь для <strong>@{replyingTo.username || 'Користувач'}</strong></span>
                <button
                  type="button"
                  onclick={() => replyingTo = null}
                  class="hover:text-rose-400 transition-colors cursor-pointer"
                  title="Скасувати відповідь"
                >
                  <X class="w-3 h-3" />
                </button>
              </div>
            {/if}
          </div>

          <!-- Input Bar (pill shape matching screenshot) -->
          <div class="relative flex items-center">
            <input
              type="text"
              placeholder={replyingTo ? `Відповідь для @${replyingTo.username || 'Користувач'}...` : "Написати коментар..."}
              bind:value={commentText}
              onkeydown={(e) => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSubmitComment(); } }}
              disabled={isSubmitting}
              class="w-full pl-4 pr-12 py-3 rounded-full bg-[#05141c] border border-cyan-900/60 focus:border-cyan-400 text-xs text-white placeholder-slate-500 outline-none transition-all"
            />
            <button
              type="button"
              onclick={handleSubmitComment}
              disabled={!commentText.trim() || isSubmitting}
              class="absolute right-2 p-2 rounded-full bg-cyan-500/20 text-cyan-400 hover:bg-cyan-500 hover:text-black transition-all disabled:opacity-40 disabled:hover:bg-transparent disabled:hover:text-cyan-400 cursor-pointer"
              aria-label="Send comment"
            >
              {#if isSubmitting}
                <Loader2 class="w-4 h-4 animate-spin" />
              {:else}
                <Send class="w-4 h-4" />
              {/if}
            </button>
          </div>
        </div>

        <!-- 3. List of Comments / Replies -->
        {#if isLoading}
          <div class="flex items-center justify-center py-8">
            <Loader2 class="w-6 h-6 text-cyan-400 animate-spin" />
          </div>
        {:else if comments.length > 0}
          <div class="space-y-3">
            {#each sortedComments() as comment (comment.id)}
              <div class="bg-[#08222d] border border-cyan-500/25 rounded-2xl p-4 shadow-md space-y-3 group hover:border-cyan-500/50 transition-all">
                
                <!-- Comment Header -->
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-2.5">
                    <div class="w-8 h-8 rounded-full bg-[#030d12] border border-cyan-400/40 flex items-center justify-center text-cyan-300 font-bold overflow-hidden shrink-0 text-xs">
                      {#if comment.userAvatarUrl || comment.user?.avatarUrl}
                        <img 
                          src={comment.userAvatarUrl || comment.user?.avatarUrl || ''} 
                          alt={comment.username || 'User'} 
                          class="w-full h-full object-cover" 
                        />
                      {:else}
                        {(comment.username || 'U').charAt(0).toUpperCase()}
                      {/if}
                    </div>

                    <div>
                      <h5 class="text-xs font-bold text-white">
                        {comment.username || comment.user?.username || 'Юзернейм'}
                      </h5>
                      <span class="text-[10px] text-slate-500 font-mono">
                        {formatDate(comment.createdAt)}
                      </span>
                    </div>
                  </div>

                  <button
                    type="button"
                    class="text-slate-500 hover:text-slate-300 transition-colors cursor-pointer p-1"
                    aria-label="More options"
                  >
                    <MoreHorizontal class="w-4 h-4" />
                  </button>
                </div>

                <!-- Inner Quote Block (if replying to another comment) -->
                {#if comment.parentReview}
                  <div class="bg-[#051720]/80 border-l-2 border-cyan-400 rounded-r-xl p-2.5 space-y-1 text-xs">
                    <div class="flex items-center gap-1.5 text-cyan-300 font-semibold text-[11px]">
                      <div class="w-4 h-4 rounded-full bg-cyan-900/60 flex items-center justify-center text-[9px] overflow-hidden shrink-0">
                        {#if comment.parentReview.userAvatarUrl}
                          <img src={comment.parentReview.userAvatarUrl} alt="" class="w-full h-full object-cover" />
                        {:else}
                          {(comment.parentReview.username || 'U').charAt(0).toUpperCase()}
                        {/if}
                      </div>
                      <span>{comment.parentReview.username}</span>
                    </div>
                    <p class="text-slate-400 text-[11px] line-clamp-2 italic">
                      "{comment.parentReview.content}"
                    </p>
                  </div>
                {/if}

                <!-- Comment Content -->
                <p class="text-xs text-slate-300 leading-relaxed whitespace-pre-wrap">
                  {comment.content}
                </p>

                <!-- Actions: Like & Reply -->
                <div class="flex items-center gap-4 pt-2 border-t border-cyan-900/20 text-xs text-slate-400">
                  <button
                    type="button"
                    onclick={() => handleToggleCommentLike(comment)}
                    class="flex items-center gap-1.5 transition-colors cursor-pointer {comment.isLiked ? 'text-rose-400' : 'hover:text-rose-300'}"
                  >
                    <Heart class="w-3.5 h-3.5 {comment.isLiked ? 'fill-rose-400' : ''}" />
                    <span class="text-xs">{formatCount(comment.likesCount)}</span>
                  </button>

                  <button
                    type="button"
                    onclick={() => { replyingTo = comment; }}
                    class="flex items-center gap-1.5 text-slate-400 hover:text-cyan-300 transition-colors cursor-pointer"
                  >
                    <CornerDownRight class="w-3.5 h-3.5" />
                    <span>Відповісти</span>
                  </button>
                </div>
              </div>
            {/each}
          </div>
        {:else}
          <div class="text-center py-8 text-slate-500 text-xs space-y-1">
            <MessageSquare class="w-8 h-8 mx-auto text-slate-600 opacity-60" />
            <p>Коментарів до цього відгуку ще немає.</p>
            <p class="text-[11px]">Станьте першим, хто напише коментар!</p>
          </div>
        {/if}

      </div>
    </div>
  </div>
{/if}

<style>
  .custom-scrollbar::-webkit-scrollbar {
    width: 6px;
  }
  .custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb {
    background: rgba(34, 211, 238, 0.2);
    border-radius: 3px;
  }
  .custom-scrollbar::-webkit-scrollbar-thumb:hover {
    background: rgba(34, 211, 238, 0.4);
  }
</style>
