<script lang="ts">
  import { ImageOff, Loader2 } from 'lucide-svelte';
  import { api } from '../../services/api';

  let {
    src,
    alt = 'Зображення',
    class: className = '',
    fallbackIconSize = 'w-6 h-6',
    fallbackText = 'Зображення недоступне',
    ...restProps
  }: {
    src?: string | null;
    alt?: string;
    class?: string;
    fallbackIconSize?: string;
    fallbackText?: string;
    [key: string]: any;
  } = $props();

  let isLoaded = $state(false);
  let isError = $state(false);

  // Резолв относительного пути бэкенда
  const resolvedSrc = $derived.by(() => {
    if (!src) return null;
    let url = src;
    if (!url.startsWith('http://') && !url.startsWith('https://') && !url.startsWith('blob:') && !url.startsWith('data:')) {
      url = url.startsWith('/') ? url : `/${url}`;
    }

    if (url.includes('/api/chat/media') || url.includes('/api/chat/uploads')) {
      const token = api.getToken();
      if (token && !url.includes('access_token=') && !url.includes('token=')) {
        const separator = url.includes('?') ? '&' : '?';
        url = `${url}${separator}access_token=${encodeURIComponent(token)}`;
      }
    }

    return url;
  });
</script>

<div class="relative overflow-hidden bg-slate-900/60 {className}">
  {#if !isLoaded && !isError && resolvedSrc}
    <div class="absolute inset-0 bg-slate-800/80 animate-pulse flex items-center justify-center z-10">
      <Loader2 class="w-4 h-4 text-cyan-400/50 animate-spin" />
    </div>
  {/if}

  {#if !resolvedSrc || isError}
    <!-- Стилизованный Fallback вместо битого изображения -->
    <div class="w-full h-full min-h-[100px] flex flex-col items-center justify-center gap-1.5 p-3 bg-slate-900/80 text-slate-400 border border-slate-800 rounded-xl">
      <ImageOff class="{fallbackIconSize} text-slate-500" />
      {#if fallbackText}
        <span class="text-[10px] text-slate-400 text-center font-medium line-clamp-1">{fallbackText}</span>
      {/if}
    </div>
  {:else}
    <img
      src={resolvedSrc}
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
