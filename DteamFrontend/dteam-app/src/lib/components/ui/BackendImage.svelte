<script lang="ts">
  import { ImageOff, Loader2 } from 'lucide-svelte';
  import { api } from '../../services/api';
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
  let blobUrl = $state<string | null>(null);

  const resolvedSrc = $derived.by(() => {
    if (!src || !src.trim()) return null;
    let url = src.trim();
    if (!url.startsWith('http://') && !url.startsWith('https://') && !url.startsWith('blob:') && !url.startsWith('data:')) {
      let cleanPath = url.startsWith('/') ? url : `/${url}`;
      if (
        cleanPath.startsWith('/icons/') ||
        cleanPath.startsWith('/community/') ||
        cleanPath.startsWith('/comunity/') ||
        cleanPath.startsWith('/uploads/') ||
        cleanPath.startsWith('/storage/') ||
        cleanPath.startsWith('/game_images/') ||
        cleanPath.startsWith('/nft/')
      ) {
        cleanPath = `/api/media${cleanPath}`;
      }
      url = BACKEND_URL ? `${BACKEND_URL.replace(/\/+$/, '')}${cleanPath}` : cleanPath;
    } else if (BACKEND_URL && url.startsWith(BACKEND_URL)) {
      const path = url.slice(BACKEND_URL.length);
      if (
        path.startsWith('/icons/') ||
        path.startsWith('/community/') ||
        path.startsWith('/comunity/') ||
        path.startsWith('/uploads/') ||
        path.startsWith('/storage/') ||
        path.startsWith('/game_images/') ||
        path.startsWith('/nft/')
      ) {
        url = `${BACKEND_URL.replace(/\/+$/, '')}/api/media${path}`;
      }
    }
    return url;
  });

  const letterFallback = $derived(
    initial || (alt && alt !== 'Зображення' ? alt.charAt(0).toUpperCase() : 'U')
  );

  $effect(() => {
    let active = true;
    let createdUrl: string | null = null;

    async function fetchImageBlob(url: string | null) {
      if (!url) {
        blobUrl = null;
        isLoaded = false;
        isError = false;
        return;
      }

      if (url.startsWith('blob:') || url.startsWith('data:')) {
        blobUrl = url;
        isLoaded = true;
        isError = false;
        return;
      }

      const isBackendUrl = url.includes('/api/') ||
                           url.startsWith('/') ||
                           (BACKEND_URL && url.startsWith(BACKEND_URL));

      if (!isBackendUrl) {
        blobUrl = url;
        return;
      }

      isLoaded = false;
      isError = false;

      try {
        const headers: Record<string, string> = {};
        const token = api.getToken();
        if (token) {
          headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch(url, { headers });
        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`);
        }

        const blob = await response.blob();
        if (!active) return;

        createdUrl = URL.createObjectURL(blob);
        blobUrl = createdUrl;
      } catch (err) {
        if (!active) return;
        isError = true;
        isLoaded = true;
      }
    }

    fetchImageBlob(resolvedSrc);

    return () => {
      active = false;
      if (createdUrl) {
        URL.revokeObjectURL(createdUrl);
      }
    };
  });
</script>

<div class="relative overflow-hidden bg-slate-900/60 {className}">
  {#if !isLoaded && !isError && blobUrl}
    <div class="absolute inset-0 bg-slate-800/80 animate-pulse flex items-center justify-center z-10">
      <Loader2 class="w-4 h-4 text-cyan-400/50 animate-spin" />
    </div>
  {/if}

  {#if !blobUrl || isError}
    {#if avatar || initial}
      <div class="w-full h-full flex items-center justify-center bg-gradient-to-tr from-cyan-600 via-teal-600 to-blue-600 text-white font-bold select-none text-xs">
        {letterFallback}
      </div>
    {:else}
      <div class="w-full h-full min-h-[100px] flex flex-col items-center justify-center gap-1.5 p-3 bg-slate-900/80 text-slate-400 border border-slate-800 rounded-xl">
        <ImageOff class="{fallbackIconSize} text-slate-500" />
        {#if fallbackText}
          <span class="text-[10px] text-slate-400 text-center font-medium line-clamp-1">{fallbackText}</span>
        {/if}
      </div>
    {/if}
  {:else}
    <img
      src={blobUrl}
      {alt}
      loading="lazy"
      decoding="async"
      onload={() => (isLoaded = true)}
      onerror={() => { isError = true; isLoaded = true; }}
      class="w-full h-full object-cover transition-opacity duration-200 {isLoaded ? 'opacity-100' : 'opacity-0'}"
      {...restProps}
    />
  {/if}
</div>
