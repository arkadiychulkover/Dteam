<script lang="ts">
  import { ImageOff, Loader2 } from 'lucide-svelte';
  import { BACKEND_URL } from '../../utils/constants';

  let {
    src,
    alt = 'Зображення',
    class: className = '',
    fallbackIconSize = 'w-6 h-6',
    fallbackText = 'Зображення недоступне',
    avatar = false,
    initial = '',
    ...restProps
  }: {
    src?: string | null;
    alt?: string;
    class?: string;
    fallbackIconSize?: string;
    fallbackText?: string;
    avatar?: boolean;
    initial?: string;
    [key: string]: any;
  } = $props();

  let isLoaded = $state(false);
  let isError = $state(false);
  let currentSrc = $state<string | null>(null);

  const isAvatar = $derived(
    avatar ||
    Boolean(initial) ||
    Boolean(src && (src.includes('avatar') || src.includes('icons/avatar') || src.includes('/icons/')))
  );

  const letterFallback = $derived(
    initial || (alt && alt !== 'Зображення' && alt !== 'Avatar' && alt !== 'Avatar preview' ? alt.charAt(0).toUpperCase() : 'U')
  );

  const resolvedTarget = $derived.by(() => {
    if (!src || !src.trim()) return { primary: null, fallback: null };
    let url = src.trim();

    if (url.startsWith('blob:') || url.startsWith('data:')) {
      return { primary: url, fallback: null };
    }

    const backendBase = BACKEND_URL ? BACKEND_URL.replace(/\/+$/, '') : '';

    if (url.startsWith('http://') || url.startsWith('https://')) {
      if (backendBase && url.startsWith(backendBase)) {
        let path = url.slice(backendBase.length);
        if (
          path.startsWith('/icons/') ||
          path.startsWith('/community/') ||
          path.startsWith('/comunity/') ||
          path.startsWith('/uploads/') ||
          path.startsWith('/storage/') ||
          path.startsWith('/game_images/') ||
          path.startsWith('/nft/')
        ) {
          return {
            primary: `${backendBase}/api/media${path}`,
            fallback: `${backendBase}${path}`
          };
        }
        if (path.startsWith('/avatar_') || path.startsWith('/avatars/')) {
          const cleanAvatar = path.startsWith('/avatars/') ? path.slice('/avatars'.length) : path;
          const iconPath = `/icons${cleanAvatar.startsWith('/') ? cleanAvatar : `/${cleanAvatar}`}`;
          return {
            primary: `${backendBase}/api/media${iconPath}`,
            fallback: `${backendBase}${iconPath}`
          };
        }
      }
      return { primary: url, fallback: null };
    }

    let cleanPath = url.startsWith('/') ? url : `/${url}`;

    // Захищені чат-медіа: додаємо токен авторизації у параметри запиту
    if (cleanPath.startsWith('/api/chat/media') || cleanPath.startsWith('/api/chat/uploads')) {
      let token = '';
      if (typeof localStorage !== 'undefined') {
        token = localStorage.getItem('dteam_token') || '';
      }
      const separator = cleanPath.includes('?') ? '&' : '?';
      const authenticatedPath = token ? `${cleanPath}${separator}access_token=${encodeURIComponent(token)}` : cleanPath;
      return {
        primary: backendBase ? `${backendBase}${authenticatedPath}` : authenticatedPath,
        fallback: null
      };
    }

    if (
      cleanPath.startsWith('/icons/') ||
      cleanPath.startsWith('/community/') ||
      cleanPath.startsWith('/comunity/') ||
      cleanPath.startsWith('/uploads/') ||
      cleanPath.startsWith('/storage/') ||
      cleanPath.startsWith('/game_images/') ||
      cleanPath.startsWith('/nft/')
    ) {
      return {
        primary: backendBase ? `${backendBase}/api/media${cleanPath}` : `/api/media${cleanPath}`,
        fallback: backendBase ? `${backendBase}${cleanPath}` : cleanPath
      };
    } else if (cleanPath.startsWith('/avatar_') || cleanPath.startsWith('/avatars/')) {
      const cleanAvatar = cleanPath.startsWith('/avatars/') ? cleanPath.slice('/avatars'.length) : cleanPath;
      const iconPath = `/icons${cleanAvatar.startsWith('/') ? cleanAvatar : `/${cleanAvatar}`}`;
      return {
        primary: backendBase ? `${backendBase}/api/media${iconPath}` : `/api/media${iconPath}`,
        fallback: backendBase ? `${backendBase}${iconPath}` : iconPath
      };
    }

    return {
      primary: backendBase ? `${backendBase}${cleanPath}` : cleanPath,
      fallback: null
    };
  });

  $effect(() => {
    const target = resolvedTarget;
    if (!target.primary) {
      currentSrc = null;
      isLoaded = false;
      isError = false;
      return;
    }

    currentSrc = target.primary;
    isLoaded = false;
    isError = false;
  });

  function handleImageError() {
    const target = resolvedTarget;
    if (target.fallback && currentSrc !== target.fallback) {
      currentSrc = target.fallback;
    } else {
      isError = true;
      isLoaded = true;
    }
  }

  function handleImageLoad() {
    isLoaded = true;
    isError = false;
  }
</script>

<div class="relative overflow-hidden bg-slate-900/60 {className}">
  {#if !isLoaded && !isError && currentSrc}
    <div class="absolute inset-0 bg-slate-800/80 animate-pulse flex items-center justify-center z-10">
      <Loader2 class="w-4 h-4 text-cyan-400/50 animate-spin" />
    </div>
  {/if}

  {#if !currentSrc || isError}
    {#if isAvatar}
      <div class="w-full h-full flex items-center justify-center bg-gradient-to-tr from-cyan-600 via-teal-600 to-blue-600 text-white font-bold select-none text-xs">
        {letterFallback}
      </div>
    {:else}
      <div class="w-full h-full min-h-0 flex flex-col items-center justify-center gap-1.5 p-2 bg-slate-900/80 text-slate-400 border border-slate-800 rounded-xl">
        <ImageOff class="{fallbackIconSize} text-slate-500" />
        {#if fallbackText}
          <span class="text-[10px] text-slate-400 text-center font-medium line-clamp-1">{fallbackText}</span>
        {/if}
      </div>
    {/if}
  {:else}
    <img
      src={currentSrc}
      {alt}
      loading="lazy"
      decoding="async"
      onload={handleImageLoad}
      onerror={handleImageError}
      class="w-full h-full object-cover transition-opacity duration-200 {isLoaded ? 'opacity-100' : 'opacity-0'}"
      {...restProps}
    />
  {/if}
</div>
