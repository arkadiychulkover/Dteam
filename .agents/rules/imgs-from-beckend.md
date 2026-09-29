---
trigger: always_on
---

# Rule: Backend Static & Uploaded Media Handling

## 1. Область применения
- Правило применяется **исключительно** к статическим и медиа-файлам, отдаваемым бэкендом (пути `/uploads/...`, `/api/chat/media/...`, `/api/media/...`, `/storage/...` и любые локальные файлы сервера).
- Внешние CDN-ссылки (Unsplash, YouTube превью и сторонние хостинги) могут использоваться напрямую.

## 2. Строгий запрет на прямой тег `<img>` и обязательная загрузка через Blob
- **СТРОГО ЗАПРЕЩАЕТСЯ** использовать прямой тег `<img src={backendPath} />` для любых картинок и аватаров с бэкенда. Прямой `src` не передает заголовки авторизации (Bearer JWT) и вызывает 401/404 ошибки загрузки аватаров.
- **ОБЯЗАТЕЛЬНО**: Все картинки с бэкенда должны загружаться **исключительно через Blob-объект** (`fetch(url, { headers: { Authorization } })` -> `response.blob()` -> `URL.createObjectURL(blob)`).
- Для отображения картинок на фронтенде всегда используется компонент `<BackendImage />`, инкапсулирующий загрузку через Blob и освобождение памяти через `URL.revokeObjectURL`.

## 3. Обязательные требования к обработке статики с бэкенда:
1. **Загрузка через Blob**:
   - Запрос к эндпоінту бэкенда (`/api/media/...`, `/api/chat/...`) отправляется через `fetch` с добавлением токена авторизации, полученный бинарный поток превращается в `Blob` (`URL.createObjectURL(blob)`).
2. **Нормализация путей (URL Resolver)**:
   - Относительные пути (например, `/api/media/icons/...`) автоматически резолвятся с учетом текущего хоста бэкенда (`BACKEND_URL`).
3. **Graceful Fallback при 404 / 401 / удаленном файле**:
   - Если файл был удален или недоступен (404/500/401), компонент обязан показать стилизованный fallback-плэйсхолдер или аватар-инициалы вместо битой картинки.
4. **Очистка ресурсов (Memory Leak Prevention)**:
   - При размонтировании компонента или смене `src` созданный URL объекта должен освобождаться через `URL.revokeObjectURL(createdUrl)`.

---

### Эталонная реализация (`src/lib/components/ui/BackendImage.svelte`):

```svelte
<script lang="ts">
  import { ImageOff, Loader2 } from 'lucide-svelte';

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
    if (src.startsWith('http://') || src.startsWith('https://') || src.startsWith('blob:') || src.startsWith('data:')) {
      return src;
    }
    // Для путей вида /uploads/... или /api/...
    return src.startsWith('/') ? src : `/${src}`;
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
