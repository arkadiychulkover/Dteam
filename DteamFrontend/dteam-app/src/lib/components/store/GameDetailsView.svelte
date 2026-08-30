<script lang="ts">
  import { onMount } from 'svelte';
  import { gamesStore } from '../../stores/gamesStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { cartStore } from '../../stores/cartStore';
  import { uiStore } from '../../stores/uiStore';
  import { currentUser } from '../../stores/authStore';
  import { gamesService } from '../../services/gamesService';
  import { formatPrice, formatBasePrice, formatBytes } from '../../utils/formatters';
  import type { Game, Review } from '../../types';
  import { 
    Star, 
    ShoppingCart, 
    Heart, 
    Share2, 
    Flag, 
    ChevronLeft, 
    ChevronRight, 
    ChevronDown, 
    ChevronUp,
    Monitor, 
    Apple, 
    Layers, 
    MessageSquare, 
    ThumbsUp, 
    ArrowLeft,
    Check,
    Plus,
    Loader2,
    Cpu,
    HardDrive,
    ShieldCheck,
    Globe,
    Newspaper,
    X
  } from 'lucide-svelte';
  import { communityService, type CommunityPost } from '../../services/communityService';
  import { router } from '../../services/router';
  import ReviewCommentsModal from './ReviewCommentsModal.svelte';

  const game = $derived($gamesStore.selectedGame || $gamesStore.games[0]);
  const isWishlisted = $derived(game ? $wishlistStore.wishlistGameIds.has(game.id) : false);

  let selectedReviewForComments = $state<Review | null>(null);
  let isReviewCommentsModalOpen = $state(false);

  let activeSubTab = $state<'about' | 'specs' | 'community'>('about');
  let selectedMediaIndex = $state(0);
  let isDescriptionExpanded = $state(false);
  let isTagsExpanded = $state(false);
  
  let reviews = $state<Review[]>([]);
  let isLoadingReviews = $state(false);
  let isReviewModalOpen = $state(false);
  let isSubmittingReview = $state(false);
  let newReviewRating = $state(5);
  let newReviewText = $state('');

  let dlcs = $state<Game[]>([]);
  let isLoadingDlcs = $state(false);

  let gameNews = $state<CommunityPost[]>([]);
  let isLoadingNews = $state(false);
  let selectedNewsModal = $state<CommunityPost | null>(null);

  async function loadReviews(gameId: string) {
    isLoadingReviews = true;
    try {
      reviews = await gamesService.getReviews(gameId);
    } catch (e) {
      console.warn('[GameDetails] Failed to load reviews:', e);
      reviews = [];
    } finally {
      isLoadingReviews = false;
    }
  }

  async function loadDlcs(gameId: string) {
    isLoadingDlcs = true;
    try {
      dlcs = await gamesService.getDlcs(gameId);
    } catch (e) {
      console.warn('[GameDetails] Failed to load DLCs:', e);
      dlcs = [];
    } finally {
      isLoadingDlcs = false;
    }
  }

  async function loadGameNews(gameId: string) {
    isLoadingNews = true;
    try {
      const res = await communityService.getPosts(gameId, 'news');
      gameNews = res.posts || [];
    } catch (e) {
      console.warn('[GameDetails] Failed to load news:', e);
      gameNews = [];
    } finally {
      isLoadingNews = false;
    }
  }

  $effect(() => {
    if (game?.id) {
      selectedMediaIndex = 0;
      loadReviews(game.id);
      loadDlcs(game.id);
      loadGameNews(game.id);
    }
  });

  const mediaList = $derived.by(() => {
    if (!game) return [];
    const list: { type: 'image' | 'video'; url: string; thumb: string }[] = [];
    if (game.headerImageUrl) {
      list.push({ type: 'image', url: game.headerImageUrl, thumb: game.headerImageUrl });
    }
    if (game.coverImageUrl && game.coverImageUrl !== game.headerImageUrl) {
      list.push({ type: 'image', url: game.coverImageUrl, thumb: game.coverImageUrl });
    }
    if (game.screenshotUrls && game.screenshotUrls.length > 0) {
      game.screenshotUrls.forEach(url => {
        list.push({ type: 'image', url, thumb: url });
      });
    }
    if (list.length === 0) {
      list.push({ 
        type: 'image', 
        url: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1600&auto=format&fit=crop&q=80', 
        thumb: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=400' 
      });
    }
    return list;
  });

  const activeMedia = $derived(mediaList[selectedMediaIndex] || mediaList[0]);

  const allTags = $derived.by(() => {
    if (game?.tags && game.tags.length > 0) return game.tags;
    if (game?.genres && game.genres.length > 0) return game.genres;
    return ['Ігри', 'Web3', 'Dteam'];
  });

  const visibleTags = $derived(
    isTagsExpanded ? allTags : allTags.slice(0, 6)
  );

  const gameEffectivePriceNanoTons = $derived.by(() => {
    if (!game) return 0;
    const price = Number(game.priceInNanoTons) || 0;
    const discount = Number(game.discountPercentage) || 0;
    return discount > 0 ? Math.round(price * (100 - discount) / 100) : price;
  });

  const effectiveTotalDlcsNanoTons = $derived.by(() => {
    return dlcs.reduce((acc, d) => {
      const price = Number(d.priceInNanoTons) || 0;
      const discount = Number(d.discountPercentage) || 0;
      const effective = discount > 0 ? Math.round(price * (100 - discount) / 100) : price;
      return acc + effective;
    }, 0);
  });

  const baseTotalDlcsNanoTons = $derived.by(() => {
    return dlcs.reduce((acc, d) => acc + (Number(d.priceInNanoTons) || 0), 0);
  });

  const completeEditionEffectiveNanoTons = $derived.by(() => {
    return gameEffectivePriceNanoTons + effectiveTotalDlcsNanoTons;
  });

  const completeEditionBaseNanoTons = $derived.by(() => {
    if (!game) return 0;
    return (Number(game.priceInNanoTons) || 0) + baseTotalDlcsNanoTons;
  });

  function prevMedia() {
    if (mediaList.length === 0) return;
    selectedMediaIndex = (selectedMediaIndex - 1 + mediaList.length) % mediaList.length;
  }

  function nextMedia() {
    if (mediaList.length === 0) return;
    selectedMediaIndex = (selectedMediaIndex + 1) % mediaList.length;
  }

  async function handleBuy(bundleName?: string, redirectToCart: boolean = false) {
    if (!game) return;
    await cartStore.addToCart(game, redirectToCart);
  }

  async function handleAddAllDLC() {
    if (dlcs.length === 0) return;
    for (const dlc of dlcs) {
      await cartStore.addToCart(dlc, false);
    }
    uiStore.setTab('cart');
  }

  async function handleAddReview() {
    if (!$currentUser) {
      uiStore.addToast({
        title: 'Потрібна авторизація',
        message: 'Будь ласка, увійдіть в акаунт, щоб залишити відгук.',
        type: 'warning'
      });
      uiStore.setLoginModal(true);
      return;
    }

    if (!newReviewText.trim()) return;

    isSubmittingReview = true;
    try {
      const createdReview = await gamesService.addReview(game.id, {
        rating: newReviewRating,
        content: newReviewText.trim(),
        isRecommended: newReviewRating >= 4
      });

      const existingIdx = reviews.findIndex(r => r.userId === $currentUser!.id);
      if (existingIdx >= 0) {
        reviews[existingIdx] = createdReview;
      } else {
        reviews = [createdReview, ...reviews];
      }

      newReviewText = '';
      isReviewModalOpen = false;

      uiStore.addToast({
        title: 'Рецензію опубліковано',
        message: 'Ваш відгук успішно збережено на сервері!',
        type: 'success'
      });

      const updatedGame = await gamesService.getGameById(game.id);
      gamesStore.selectGame(updatedGame);
    } catch (e: any) {
      uiStore.addToast({
        title: 'Помилка публікації',
        message: e?.message || 'Не вдалося зберегти відгук. Перевірте авторизацію.',
        type: 'error'
      });
    } finally {
      isSubmittingReview = false;
    }
  }

  function openReviewComments(review: Review) {
    selectedReviewForComments = review;
    isReviewCommentsModalOpen = true;
  }

  async function handleToggleReviewCardLike(review: Review) {
    if (!$currentUser) {
      uiStore.addToast({
        title: 'Потрібна авторизація',
        message: 'Будь ласка, увійдіть в акаунт, щоб поставити вподобайку.',
        type: 'warning'
      });
      uiStore.setLoginModal(true);
      return;
    }

    const wasLiked = review.isLiked ?? false;
    const currentLikes = review.likesCount ?? 0;

    // Optimistic
    reviews = reviews.map(r => {
      if (r.id === review.id) {
        return {
          ...r,
          isLiked: !wasLiked,
          likesCount: wasLiked ? Math.max(0, currentLikes - 1) : currentLikes + 1
        };
      }
      return r;
    });

    try {
      const res = await gamesService.toggleReviewLike(game.id, review.id);
      reviews = reviews.map(r => {
        if (r.id === review.id) {
          return {
            ...r,
            isLiked: res.liked,
            likesCount: res.likesCount
          };
        }
        return r;
      });
    } catch {
      // Revert
      reviews = reviews.map(r => {
        if (r.id === review.id) {
          return {
            ...r,
            isLiked: wasLiked,
            likesCount: currentLikes
          };
        }
        return r;
      });
    }
  }

  function handleReviewUpdatedFromModal(updated: Review) {
    reviews = reviews.map(r => r.id === updated.id ? { ...r, ...updated } : r);
    if (selectedReviewForComments?.id === updated.id) {
      selectedReviewForComments = { ...selectedReviewForComments, ...updated };
    }
  }

  function openDlc(dlc: Game) {
    router.navigateToGame(dlc);
  }

  function formatReviewDate(dateStr?: string): string {
    if (!dateStr) return 'Нещодавно';
    try {
      const d = new Date(dateStr);
      return d.toLocaleDateString('uk-UA', { day: '2-digit', month: '2-digit', year: 'numeric' });
    } catch {
      return dateStr;
    }
  }

  function scrollToSection(sectionId: 'section-about' | 'section-specs' | 'section-community', tab: 'about' | 'specs' | 'community') {
    activeSubTab = tab;
    if (typeof document === 'undefined') return;
    const element = document.getElementById(sectionId);
    if (element) {
      const yOffset = -70;
      const y = element.getBoundingClientRect().top + window.pageYOffset + yOffset;
      window.scrollTo({ top: y, behavior: 'smooth' });
    }
  }
</script>

{#if game}
  <div class="max-w-7xl mx-auto px-4 lg:px-8 py-6 space-y-8 animate-in fade-in duration-300">

    <div class="sticky top-14 z-30 bg-[#030d12]/95 backdrop-blur-xl -mx-4 lg:-mx-8 px-4 lg:px-8 py-3 border-b border-cyan-950/80 flex items-center justify-between shadow-lg">
      <div class="flex items-center gap-8">
        <button
          onclick={() => scrollToSection('section-about', 'about')}
          class="text-sm font-extrabold pb-3 -mb-3 transition-all cursor-pointer relative
            {activeSubTab === 'about' ? 'text-cyan-400 border-b-2 border-cyan-400 shadow-[0_4px_12px_rgba(13,242,201,0.3)]' : 'text-slate-400 hover:text-white'}"
        >
          Про гру
        </button>
        <button
          onclick={() => scrollToSection('section-specs', 'specs')}
          class="text-sm font-extrabold pb-3 -mb-3 transition-all cursor-pointer relative
            {activeSubTab === 'specs' ? 'text-cyan-400 border-b-2 border-cyan-400 shadow-[0_4px_12px_rgba(13,242,201,0.3)]' : 'text-slate-400 hover:text-white'}"
        >
          Характеристики
        </button>
        <button
          onclick={() => scrollToSection('section-community', 'community')}
          class="text-sm font-extrabold pb-3 -mb-3 transition-all cursor-pointer relative
            {activeSubTab === 'community' ? 'text-cyan-400 border-b-2 border-cyan-400 shadow-[0_4px_12px_rgba(13,242,201,0.3)]' : 'text-slate-400 hover:text-white'}"
        >
          Спільнота
        </button>
      </div>

      <button
        onclick={() => uiStore.setTab('store')}
        class="inline-flex items-center gap-1.5 text-xs font-bold text-slate-400 hover:text-cyan-400 transition-colors cursor-pointer"
      >
        <ArrowLeft class="w-3.5 h-3.5" />
        <span>Назад до крамниці</span>
      </button>
    </div>

    <div id="section-about" class="space-y-6 pt-2">
      
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h1 class="text-3xl sm:text-4xl font-black text-white font-display tracking-wide">
          {game.title}
        </h1>

        <div class="flex items-center gap-2 text-rose-400 font-extrabold text-base">
          <span class="text-white font-mono">{game.averageRating > 0 ? game.averageRating.toFixed(1) : '5.0'}</span>
          <div class="flex items-center gap-0.5 text-rose-500">
            {#each Array(5) as _, idx}
              <Star class="w-4 h-4 fill-rose-500 text-rose-500" />
            {/each}
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">

        <div class="lg:col-span-8 space-y-6">

          <div class="relative w-full aspect-[16/9] rounded-3xl overflow-hidden bg-slate-950 border border-cyan-500/20 shadow-2xl shadow-cyan-950/40 group">
            <img
              src={activeMedia.url}
              alt={game.title}
              class="w-full h-full object-cover transition-all duration-300"
            />

            <div class="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent pointer-events-none"></div>

            {#if mediaList.length > 1}
              <button
                onclick={prevMedia}
                class="absolute left-3 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-black/60 hover:bg-cyan-500 text-white hover:text-black border border-white/20 flex items-center justify-center backdrop-blur-md transition-all cursor-pointer shadow-lg"
                aria-label="Previous image"
              >
                <ChevronLeft class="w-5 h-5" />
              </button>
              <button
                onclick={nextMedia}
                class="absolute right-3 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-black/60 hover:bg-cyan-500 text-white hover:text-black border border-white/20 flex items-center justify-center backdrop-blur-md transition-all cursor-pointer shadow-lg"
                aria-label="Next image"
              >
                <ChevronRight class="w-5 h-5" />
              </button>
            {/if}
          </div>

          {#if mediaList.length > 1}
            <div class="relative flex items-center gap-2 sm:gap-3 overflow-x-auto pb-2 scrollbar-thin scrollbar-thumb-cyan-500/30">
              <button
                onclick={prevMedia}
                class="w-7 h-14 rounded-xl bg-[#061820] hover:bg-cyan-500/20 border border-cyan-500/30 text-slate-300 hover:text-white flex items-center justify-center shrink-0 cursor-pointer"
              >
                <ChevronLeft class="w-4 h-4" />
              </button>

              {#each mediaList as item, idx}
                <button
                  onclick={() => selectedMediaIndex = idx}
                  class="relative w-24 sm:w-28 h-14 rounded-xl overflow-hidden border-2 shrink-0 transition-all cursor-pointer
                    {selectedMediaIndex === idx 
                      ? 'border-cyan-400 scale-105 shadow-md shadow-cyan-500/30' 
                      : 'border-transparent opacity-60 hover:opacity-100'}"
                >
                  <img src={item.thumb} alt="Preview {idx}" class="w-full h-full object-cover" />
                </button>
              {/each}

              <button
                onclick={nextMedia}
                class="w-7 h-14 rounded-xl bg-[#061820] hover:bg-cyan-500/20 border border-cyan-500/30 text-slate-300 hover:text-white flex items-center justify-center shrink-0 cursor-pointer"
              >
                <ChevronRight class="w-4 h-4" />
              </button>
            </div>
          {/if}

          <div class="flex flex-wrap items-center gap-2 pt-2">
            {#each visibleTags as tag}
              <button
                onclick={() => { gamesStore.setFilters({ search: tag }); uiStore.setTab('catalog'); }}
                class="px-3 py-1 rounded-full bg-[#061e28] hover:bg-cyan-950/80 border border-cyan-500/30 text-xs font-semibold text-cyan-300 hover:border-cyan-400 transition-all cursor-pointer"
              >
                {tag}
              </button>
            {/each}
            
            {#if allTags.length > 6}
              <button
                onclick={() => isTagsExpanded = !isTagsExpanded}
                class="p-1 rounded-full bg-[#061e28] hover:bg-cyan-950/80 border border-cyan-500/30 text-slate-400 hover:text-white transition-colors cursor-pointer"
                title={isTagsExpanded ? 'Згорнути теги' : 'Показати більше тегів'}
              >
                {#if isTagsExpanded}
                  <ChevronUp class="w-4 h-4" />
                {:else}
                  <ChevronDown class="w-4 h-4" />
                {/if}
              </button>
            {/if}
          </div>

          <div class="space-y-3 pt-2">
            <p class="text-sm text-slate-300 leading-relaxed {isDescriptionExpanded ? '' : 'line-clamp-3'}">
              {game.description || game.shortDescription || `${game.title} — захоплива гра на платформі Dteam Gaming Hub.`}
            </p>

            <button
              onclick={() => isDescriptionExpanded = !isDescriptionExpanded}
              class="flex items-center gap-1 text-xs font-bold text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer"
            >
              <span>{isDescriptionExpanded ? 'Згорнути' : 'Читати далі'}</span>
              {#if isDescriptionExpanded}
                <ChevronUp class="w-3.5 h-3.5" />
              {:else}
                <ChevronDown class="w-3.5 h-3.5" />
              {/if}
            </button>
          </div>
        </div>

        <div class="lg:col-span-4 space-y-6">

          <div class="bg-[#061820]/90 backdrop-blur-xl border border-cyan-500/25 rounded-3xl p-5 sm:p-6 shadow-2xl shadow-cyan-950/50 space-y-5">

            <div class="w-full aspect-[21/9] rounded-2xl overflow-hidden bg-slate-950 border border-cyan-500/20 shadow-inner">
              <img
                src={game.coverImageUrl || game.headerImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=800&auto=format&fit=crop&q=80'}
                alt={game.title}
                class="w-full h-full object-cover"
              />
            </div>

            <div class="flex items-baseline justify-between pt-1">
              <div class="flex items-center gap-2">
                <span class="text-3xl font-black text-white font-display tracking-tight">
                  {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                </span>
                {#if (game.discountPercentage || 0) > 0}
                  <span class="px-2 py-0.5 rounded-lg bg-rose-600 text-white font-extrabold text-xs">
                    -{game.discountPercentage}%
                  </span>
                {/if}
              </div>

              {#if (game.discountPercentage || 0) > 0}
                <span class="text-sm text-slate-400 line-through font-mono">
                  {formatBasePrice(game.priceInNanoTons)}
                </span>
              {/if}
            </div>

            <button
              onclick={() => handleBuy(game.title, true)}
              class="w-full py-3.5 rounded-2xl bg-gradient-to-r from-emerald-400 via-teal-400 to-cyan-400 hover:from-emerald-300 hover:to-cyan-300 text-black font-black text-base tracking-wide shadow-lg shadow-emerald-500/25 hover:shadow-cyan-400/40 transition-all cursor-pointer flex items-center justify-center gap-2"
            >
              <span>Купити</span>
            </button>

            <div class="flex items-center gap-2">
              <button
                onclick={() => handleBuy(game.title, false)}
                class="flex-1 py-3 rounded-2xl bg-[#0b2834] hover:bg-[#0f3444] border border-cyan-500/30 text-cyan-300 hover:text-white font-bold text-xs transition-all cursor-pointer flex items-center justify-center gap-2"
              >
                <ShoppingCart class="w-4 h-4 text-cyan-400" />
                <span>Додати у кошик</span>
              </button>

              <button
                onclick={() => wishlistStore.toggleWishlist(game)}
                class="p-3 rounded-2xl border transition-all cursor-pointer flex items-center justify-center
                  {isWishlisted
                    ? 'bg-rose-950/80 border-rose-500 text-rose-500 shadow-md shadow-rose-500/20'
                    : 'bg-[#0b2834] hover:bg-[#0f3444] border-cyan-500/30 text-slate-300 hover:text-rose-400'}"
                title={isWishlisted ? 'Видалити зі списку бажань' : 'Додати до списку бажань'}
              >
                <Heart class="w-4 h-4 {isWishlisted ? 'fill-rose-500 text-rose-500' : ''}" />
              </button>
            </div>

            <div class="flex items-center justify-between pt-2 border-t border-cyan-950/80 text-xs font-semibold text-slate-400">
              <button
                onclick={() => {
                  if (navigator.clipboard) {
                    navigator.clipboard.writeText(window.location.href);
                    uiStore.addToast({ title: 'Посилання скопійовано', message: 'Посилання на гру скопійовано в буфер!', type: 'success' });
                  }
                }}
                class="flex items-center gap-1.5 hover:text-cyan-300 transition-colors cursor-pointer"
              >
                <Share2 class="w-3.5 h-3.5" />
                <span>Репост</span>
              </button>

              <button
                onclick={() => uiStore.addToast({ title: 'Скарга надіслана', message: 'Дякуємо за пильність. Модератори перевірять сторінку.', type: 'info' })}
                class="flex items-center gap-1.5 hover:text-rose-400 transition-colors cursor-pointer"
              >
                <Flag class="w-3.5 h-3.5" />
                <span>Поскаржитись</span>
              </button>
            </div>

            <div class="space-y-2.5 pt-3 border-t border-cyan-950/80 text-xs">
              <div class="flex justify-between text-slate-400">
                <span>Дата виходу:</span>
                <span class="text-white font-medium">
                  {game.createdAt ? new Date(game.createdAt).toLocaleDateString('uk-UA') : '10.12.2020'}
                </span>
              </div>
              <div class="flex justify-between text-slate-400">
                <span>Розробник:</span>
                <span class="text-cyan-300 font-medium truncate max-w-[180px]">
                  {game.owner?.username || 'Dteam Dev'}
                </span>
              </div>
              <div class="flex justify-between text-slate-400">
                <span>Видавець:</span>
                <span class="text-cyan-300 font-medium">Dteam Games</span>
              </div>
              <div class="flex justify-between items-center text-slate-400">
                <span>Платформи:</span>
                <div class="flex items-center gap-2 text-white">
                  {#if !game.platforms || game.platforms.includes('Windows')}
                    <span title="Windows"><Monitor class="w-4 h-4 text-slate-300" /></span>
                  {/if}
                  {#if game.platforms && game.platforms.includes('MacOS')}
                    <span title="MacOS"><Apple class="w-4 h-4 text-slate-300" /></span>
                  {/if}
                </div>
              </div>
            </div>
          </div>

        </div>
      </div>

      <div class="space-y-4 pt-6">
        <h2 class="text-2xl font-black text-white font-display tracking-wide">
          Комплекти
        </h2>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">

          <div class="bg-[#061820]/90 border border-cyan-500/25 rounded-3xl p-6 shadow-xl flex flex-col justify-between space-y-4">
            <div>
              <h3 class="text-lg font-black text-white">{game.title} (Стандартне видання)</h3>
              <p class="text-xs text-slate-300 mt-2 leading-relaxed">
                {game.shortDescription || game.description || `${game.title} — базова версія гри.`}
              </p>

              <div class="mt-4 pt-4 border-t border-cyan-950/80 text-xs text-slate-400 space-y-1.5">
                <span class="font-bold text-slate-300">Вміст:</span>
                <ul class="list-disc list-inside space-y-1 text-slate-300 text-xs">
                  <li>{game.title} <span class="text-slate-400">(базова гра)</span></li>
                  {#if game.features && game.features.length > 0}
                    {#each game.features.slice(0, 3) as feat}
                      <li>{feat}</li>
                    {/each}
                  {:else}
                    <li>Повний доступ до сюжетної лінії</li>
                  {/if}
                </ul>
              </div>
            </div>

            <div class="flex items-center justify-between pt-4 border-t border-cyan-950/80">
              <span class="text-xl font-black text-white font-mono">
                {formatPrice(game.priceInNanoTons, game.discountPercentage)}
              </span>

              <button
                onclick={() => handleBuy(game.title)}
                class="px-5 py-2 rounded-xl bg-gradient-to-r from-emerald-400 to-cyan-400 hover:from-emerald-300 hover:to-cyan-300 text-black font-extrabold text-xs tracking-wide shadow-md transition-all cursor-pointer flex items-center gap-1.5"
              >
                <span>У кошик</span>
              </button>
            </div>
          </div>

          {#if dlcs.length > 0}
            <div class="bg-[#061820]/90 border border-cyan-500/25 rounded-3xl p-6 shadow-xl flex flex-col justify-between space-y-4">
              <div>
                <div class="flex items-center justify-between">
                  <h3 class="text-lg font-black text-white">{game.title}: Повне видання</h3>
                  {#if (game.discountPercentage || 0) > 0}
                    <span class="px-2 py-0.5 rounded-lg bg-rose-600 text-white font-extrabold text-[10px]">
                      -{game.discountPercentage}%
                    </span>
                  {/if}
                </div>

                <div class="mt-4 pt-4 border-t border-cyan-950/80 text-xs text-slate-400 space-y-1.5">
                  <span class="font-bold text-slate-300">Вміст:</span>
                  <ul class="list-disc list-inside space-y-1 text-slate-300 text-xs">
                    <li>{game.title} <span class="text-slate-400">(базова гра)</span></li>
                    {#each dlcs as dlc}
                      <li>
                        <button
                          type="button"
                          onclick={() => openDlc(dlc)}
                          class="hover:text-cyan-300 transition-colors cursor-pointer text-left"
                        >
                          {dlc.title} <span class="text-purple-400">(DLC)</span>
                        </button>
                      </li>
                    {/each}
                  </ul>
                </div>
              </div>

              <div class="flex items-center justify-between pt-4 border-t border-cyan-950/80">
                <div class="flex items-center gap-2">
                  <span class="text-xl font-black text-white font-mono">
                    {formatPrice(completeEditionEffectiveNanoTons)}
                  </span>
                  {#if completeEditionBaseNanoTons > completeEditionEffectiveNanoTons}
                    <span class="text-xs text-slate-500 line-through font-mono">
                      {formatBasePrice(completeEditionBaseNanoTons)}
                    </span>
                  {/if}
                </div>

                <button
                  onclick={() => handleBuy(`${game.title}: Повне видання`)}
                  class="px-5 py-2 rounded-xl bg-gradient-to-r from-emerald-400 to-cyan-400 hover:from-emerald-300 hover:to-cyan-300 text-black font-extrabold text-xs tracking-wide shadow-md transition-all cursor-pointer flex items-center gap-1.5"
                >
                  <span>У кошик</span>
                </button>
              </div>
            </div>
          {/if}

        </div>
      </div>

      {#if dlcs.length > 0}
        <div class="space-y-4 pt-6">
          <div class="flex items-center justify-between">
            <h2 class="text-2xl font-black text-white font-display tracking-wide">
              Інші DLC
            </h2>

            <button
              type="button"
              onclick={() => router.navigateToAllDlcs(game)}
              class="flex items-center gap-1 text-sm font-bold text-slate-300 hover:text-cyan-400 transition-colors cursor-pointer group"
            >
              <span>Усі DLC</span>
              <ChevronRight class="w-4 h-4 text-slate-400 group-hover:text-cyan-400 group-hover:translate-x-0.5 transition-all" />
            </button>
          </div>

          <div class="space-y-2.5">
            {#each dlcs as dlc}
              <div
                role="button"
                tabindex="0"
                onclick={() => openDlc(dlc)}
                onkeydown={(e) => e.key === 'Enter' && openDlc(dlc)}
                class="group flex items-center justify-between px-5 py-3.5 rounded-2xl bg-[#08222d] hover:bg-[#0c3140] border border-cyan-500/20 hover:border-cyan-400/60 transition-all cursor-pointer shadow-md"
              >
                <span class="text-sm font-bold text-white group-hover:text-cyan-300 transition-colors truncate">
                  {dlc.title}
                </span>

                <span class="text-xs font-bold {Number(dlc.priceInNanoTons) === 0 ? 'text-emerald-400' : 'text-cyan-300 font-mono'}">
                  {Number(dlc.priceInNanoTons) === 0 ? 'Безкоштовно' : formatPrice(dlc.priceInNanoTons, dlc.discountPercentage)}
                </span>
              </div>
            {/each}
          </div>
        </div>
      {/if}

      <!-- Developer News Section -->
      {#if gameNews.length > 0}
        <div class="space-y-4 pt-6">
          <div class="flex items-center justify-between">
            <h2 class="text-2xl font-black text-white font-display tracking-wide flex items-center gap-2.5">
              <Newspaper class="w-6 h-6 text-cyan-400" />
              <span>Офіційні новини та оновлення ({gameNews.length})</span>
            </h2>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            {#each gameNews as newsItem}
              <div
                role="button"
                tabindex="0"
                onclick={() => selectedNewsModal = newsItem}
                onkeydown={(e) => e.key === 'Enter' && (selectedNewsModal = newsItem)}
                class="group bg-[#061820]/90 border border-cyan-500/20 hover:border-cyan-400/50 rounded-2xl p-5 shadow-lg hover:shadow-xl hover:shadow-cyan-950/40 transition-all cursor-pointer flex flex-col justify-between space-y-3"
              >
                <div class="space-y-2">
                  <div class="flex items-center justify-between text-[11px] text-slate-400">
                    <span class="px-2 py-0.5 rounded bg-cyan-950/80 text-cyan-300 font-bold border border-cyan-800/40">
                      Офіційний патч-ноут
                    </span>
                    <span class="font-mono">{new Date(newsItem.createdAt).toLocaleDateString('uk-UA')}</span>
                  </div>

                  <h3 class="text-base font-bold text-white group-hover:text-cyan-300 transition-colors line-clamp-1">
                    {newsItem.title}
                  </h3>

                  <p class="text-xs text-slate-300 line-clamp-3 leading-relaxed">
                    {newsItem.content}
                  </p>
                </div>

                {#if newsItem.media?.url}
                  <div class="relative rounded-xl overflow-hidden max-h-36 bg-black/40 border border-cyan-950">
                    <img src={newsItem.media.url} alt={newsItem.title} class="w-full h-32 object-cover group-hover:scale-105 transition-transform duration-300" />
                  </div>
                {/if}

                <div class="pt-2 border-t border-cyan-950/60 flex items-center justify-between text-xs text-cyan-400 font-semibold">
                  <span>Читати детальніше</span>
                  <ChevronRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
                </div>
              </div>
            {/each}
          </div>
        </div>
      {/if}
    </div>

    <div id="section-specs" class="space-y-4 pt-8 border-t border-cyan-950/80">
      <h2 class="text-2xl font-black text-white font-display tracking-wide flex items-center gap-2.5">
        <Cpu class="w-6 h-6 text-cyan-400" />
        <span>Характеристики та системні вимоги</span>
      </h2>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        
        <div class="bg-[#061820]/90 border border-cyan-500/25 rounded-3xl p-6 shadow-xl space-y-4">
          <h3 class="text-sm font-extrabold text-cyan-300 uppercase tracking-wider flex items-center gap-2">
            <Monitor class="w-4 h-4 text-cyan-400" />
            <span>Мінімальні системні вимоги</span>
          </h3>

          <div class="space-y-2.5 text-xs text-slate-300">
            <div class="flex justify-between py-1 border-b border-cyan-950/60">
              <span class="text-slate-400">ОС:</span>
              <span class="font-medium text-white">{game.platforms?.join(', ') || 'Windows 10 / 11 (64-bit)'}</span>
            </div>
            <div class="flex justify-between py-1 border-b border-cyan-950/60">
              <span class="text-slate-400">Процесор:</span>
              <span class="font-medium text-white">Intel Core i5-8400 / AMD Ryzen 5 2600</span>
            </div>
            <div class="flex justify-between py-1 border-b border-cyan-950/60">
              <span class="text-slate-400">Оперативна пам'ять:</span>
              <span class="font-medium text-white">12 GB ОЗП</span>
            </div>
            <div class="flex justify-between py-1 border-b border-cyan-950/60">
              <span class="text-slate-400">Відеокарта:</span>
              <span class="font-medium text-white">NVIDIA GeForce GTX 1060 6GB / AMD Radeon RX 580</span>
            </div>
            <div class="flex justify-between py-1">
              <span class="text-slate-400">Вільне місце на диску:</span>
              <span class="font-mono font-bold text-cyan-300">{formatBytes(game.sizeInBytes || 75161927680)}</span>
            </div>
          </div>
        </div>

        <div class="bg-[#061820]/90 border border-cyan-500/25 rounded-3xl p-6 shadow-xl space-y-4">
          <h3 class="text-sm font-extrabold text-cyan-300 uppercase tracking-wider flex items-center gap-2">
            <ShieldCheck class="w-4 h-4 text-cyan-400" />
            <span>Особливості гри та версія</span>
          </h3>

          <div class="space-y-2.5 text-xs text-slate-300">
            <div class="flex justify-between py-1 border-b border-cyan-950/60">
              <span class="text-slate-400">Поточна версія:</span>
              <span class="font-mono text-cyan-300 font-bold">v{game.version || '1.0.0'}</span>
            </div>
            <div class="flex justify-between py-1 border-b border-cyan-950/60">
              <span class="text-slate-400">Жанри:</span>
              <span class="font-medium text-white">{game.genres?.join(', ') || 'Action, RPG'}</span>
            </div>
            <div class="flex justify-between py-1 border-b border-cyan-950/60">
              <span class="text-slate-400">Особливості:</span>
              <span class="font-medium text-white">
                {game.features && game.features.length > 0 ? game.features.join(', ') : 'Одиночна гра, Хмарні збереження, Досягнення'}
              </span>
            </div>
            <div class="flex justify-between py-1 border-b border-cyan-950/60">
              <span class="text-slate-400">Мови інтерфейсу:</span>
              <span class="font-medium text-white">Українська, English, Deutsch, Polski</span>
            </div>
            <div class="flex justify-between py-1">
              <span class="text-slate-400">Платформа дистрибуції:</span>
              <span class="font-bold text-cyan-400 font-mono">Dteam Web3 Gaming</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div id="section-community" class="space-y-6 pt-8 border-t border-cyan-950/80">
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 class="text-2xl font-black text-white font-display tracking-wide flex items-center gap-2.5">
            <MessageSquare class="w-6 h-6 text-cyan-400" />
            <span>Спільнота та рецензії ({reviews.length})</span>
          </h2>
          <p class="text-xs text-slate-400 mt-1">
            Сортування: <span class="text-cyan-400 font-semibold">Спочатку нові ▼</span>
          </p>
        </div>

        <button
          onclick={() => {
            if (!$currentUser) {
              uiStore.addToast({
                title: 'Потрібна авторизація',
                message: 'Будь ласка, увійдіть в акаунт, щоб залишити відгук.',
                type: 'warning'
              });
              uiStore.setLoginModal(true);
            } else {
              isReviewModalOpen = true;
            }
          }}
          class="px-5 py-2.5 rounded-2xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-xs tracking-wide shadow-lg shadow-cyan-500/20 transition-all cursor-pointer"
        >
          Написати рецензію
        </button>
      </div>

      {#if isLoadingReviews}
        <div class="flex items-center justify-center py-12">
          <Loader2 class="w-8 h-8 text-cyan-400 animate-spin" />
        </div>
      {:else if reviews.length > 0}
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          {#each reviews as review}
            <div class="bg-[#061820]/90 border border-cyan-500/25 rounded-3xl p-6 shadow-xl flex flex-col justify-between space-y-4 group hover:border-cyan-400/60 transition-all">
              <div class="space-y-3">
                
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-full bg-[#0a232c] border border-cyan-400/40 flex items-center justify-center text-cyan-300 font-bold overflow-hidden">
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
                      <h4 class="text-sm font-extrabold text-white">
                        {review.username || review.user?.username || 'Користувач'}
                      </h4>
                      <div class="flex items-center gap-0.5 text-rose-500 mt-0.5">
                        {#each Array(review.rating) as _}
                          <Star class="w-3.5 h-3.5 fill-rose-500 text-rose-500" />
                        {/each}
                      </div>
                    </div>
                  </div>

                  <span class="text-[11px] text-slate-500 font-mono">
                    {formatReviewDate(review.createdAt)}
                  </span>
                </div>

                <p class="text-xs text-slate-300 leading-relaxed whitespace-pre-wrap">
                  {review.content}
                </p>
              </div>

              <div class="flex items-center justify-between pt-3 border-t border-cyan-950/80 text-xs text-slate-400">
                <span class="text-[11px] font-semibold {review.isRecommended ? 'text-emerald-400' : 'text-slate-500'}">
                  {review.isRecommended ? '✓ Рекомендує гру' : 'Не рекомендує'}
                </span>

                <div class="flex items-center gap-3">
                  <button
                    type="button"
                    onclick={(e) => { e.stopPropagation(); handleToggleReviewCardLike(review); }}
                    class="flex items-center gap-1.5 text-xs transition-colors cursor-pointer {review.isLiked ? 'text-rose-400' : 'hover:text-rose-300'}"
                    title="Поставити вподобайку"
                  >
                    <Heart class="w-3.5 h-3.5 {review.isLiked ? 'fill-rose-400' : ''}" />
                    <span>{review.likesCount ?? 0}</span>
                  </button>

                  <button
                    type="button"
                    onclick={() => openReviewComments(review)}
                    class="flex items-center gap-1.5 text-xs text-cyan-400 hover:text-cyan-300 transition-colors cursor-pointer bg-cyan-950/40 hover:bg-cyan-900/60 px-2.5 py-1 rounded-xl border border-cyan-500/20"
                    title="Відкрити коментарі"
                  >
                    <MessageSquare class="w-3.5 h-3.5" />
                    <span>{review.repliesCount ?? review.replies?.length ?? 0}</span>
                  </button>
                </div>
              </div>
            </div>
          {/each}
        </div>
      {:else}
        <div class="text-center py-12 bg-[#061820]/60 rounded-3xl border border-cyan-500/20 shadow-xl space-y-3">
          <MessageSquare class="w-12 h-12 text-cyan-500/40 mx-auto" />
          <h4 class="text-base font-bold text-white">Рецензій поки немає</h4>
          <p class="text-xs text-slate-400 max-w-sm mx-auto">
            Будьте першим, хто поділиться своїми враженнями від гри!
          </p>
          <button
            onclick={() => {
              if (!$currentUser) {
                uiStore.addToast({ title: 'Потрібна авторизація', message: 'Будь ласка, увійдіть в акаунт, щоб залишити відгук.', type: 'warning' });
                uiStore.setLoginModal(true);
              } else {
                isReviewModalOpen = true;
              }
            }}
            class="mt-2 px-5 py-2 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 text-black font-bold text-xs shadow-md transition-all cursor-pointer"
          >
            Написати першу рецензію
          </button>
        </div>
      {/if}
    </div>

  </div>

  {#if isReviewModalOpen}
    <div class="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4 animate-in fade-in">
      <div class="relative w-full max-w-lg bg-[#061820] border border-cyan-500/30 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/80 space-y-5">
        <h3 class="text-xl font-black text-white">Написати рецензію на {game.title}</h3>

        <div class="space-y-2">
          <span class="block text-xs font-bold text-slate-300">Ваша оцінка</span>
          <div class="flex items-center gap-2">
            {#each [1, 2, 3, 4, 5] as star}
              <button
                type="button"
                onclick={() => newReviewRating = star}
                class="p-1 cursor-pointer transition-transform hover:scale-125"
              >
                <Star class="w-6 h-6 {star <= newReviewRating ? 'fill-rose-500 text-rose-500' : 'text-slate-600'}" />
              </button>
            {/each}
          </div>
        </div>

        <div class="space-y-2">
          <label for="review-text-input" class="block text-xs font-bold text-slate-300">Ваш відгук</label>
          <textarea
            id="review-text-input"
            rows="4"
            bind:value={newReviewText}
            placeholder="Поділіться вашими враженнями від гри..."
            class="w-full p-3.5 rounded-2xl bg-[#030d12] border border-cyan-500/20 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400"
          ></textarea>
        </div>

        <div class="flex items-center justify-end gap-3 pt-2">
          <button
            onclick={() => isReviewModalOpen = false}
            disabled={isSubmittingReview}
            class="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-xs font-bold text-slate-300 cursor-pointer disabled:opacity-50"
          >
            Скасувати
          </button>
          <button
            onclick={handleAddReview}
            disabled={isSubmittingReview || !newReviewText.trim()}
            class="px-5 py-2 rounded-xl bg-gradient-to-r from-emerald-400 to-cyan-400 text-black font-black text-xs cursor-pointer shadow-md disabled:opacity-50 flex items-center gap-2"
          >
            {#if isSubmittingReview}
              <Loader2 class="w-3.5 h-3.5 animate-spin" />
            {/if}
            <span>Опублікувати</span>
          </button>
        </div>
      </div>
    </div>
  {/if}
{/if}

{#if selectedNewsModal}
  <div
    role="presentation"
    class="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4"
    onclick={(e) => { if (e.target === e.currentTarget) selectedNewsModal = null; }}
  >
    <div
      role="dialog"
      aria-modal="true"
      class="bg-[#061820] border border-cyan-500/30 rounded-3xl p-6 max-w-xl w-full shadow-2xl space-y-4 max-h-[85vh] flex flex-col animate-in fade-in"
    >
      <div class="flex items-start justify-between border-b border-cyan-900/40 pb-3">
        <div>
          <span class="text-[10px] uppercase font-bold text-cyan-400 tracking-wider">Офіційна новина від розробника</span>
          <h3 class="text-base font-bold text-white mt-1">{selectedNewsModal.title}</h3>
          <span class="text-[11px] text-slate-400 font-mono">{new Date(selectedNewsModal.createdAt).toLocaleDateString('uk-UA')}</span>
        </div>
        <button
          onclick={() => selectedNewsModal = null}
          class="p-1.5 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors cursor-pointer"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <div class="flex-1 overflow-y-auto space-y-3 pr-1 text-sm text-slate-200 leading-relaxed whitespace-pre-line">
        {#if selectedNewsModal.media?.url}
          {#if selectedNewsModal.media.type === 'video'}
            <video src={selectedNewsModal.media.url} class="w-full rounded-2xl max-h-64 object-cover" controls></video>
          {:else}
            <img src={selectedNewsModal.media.url} alt="" class="w-full rounded-2xl max-h-64 object-cover" />
          {/if}
        {/if}
        <p>{selectedNewsModal.content}</p>
      </div>
    </div>
  </div>
{/if}

<ReviewCommentsModal
  isOpen={isReviewCommentsModalOpen}
  gameId={game.id}
  review={selectedReviewForComments}
  onClose={() => { isReviewCommentsModalOpen = false; selectedReviewForComments = null; }}
  onReviewUpdated={handleReviewUpdatedFromModal}
/>
