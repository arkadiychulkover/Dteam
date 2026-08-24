<script lang="ts">
  import { adminService, type UpdateGameDto } from '../../services/adminService';
  import { gamesService } from '../../services/gamesService';
  import { uiStore } from '../../stores/uiStore';
  import { formatPrice } from '../../utils/formatters';
  import type { Game } from '../../types';
  import { X, Edit, CheckCircle2, Film, FileArchive, DollarSign, Layers, Plus, ExternalLink, Unlink, Percent, Tag, Upload, Trash2, Image } from 'lucide-svelte';

  interface Props {
    game: Game | null;
    allGames?: Game[];
    isOpen: boolean;
    onClose: () => void;
    onGameUpdated: () => void;
    onOpenEditOtherGame?: (otherGame: Game) => void;
  }

  let { game, allGames = [], isOpen, onClose, onGameUpdated, onOpenEditOtherGame }: Props = $props();

  let title = $state('');
  let description = $state('');
  let shortDescription = $state('');
  let priceInTon = $state('0');
  let discountPercentage = $state(0);
  let serverArchivePath = $state('');
  let version = $state('1.0.0');
  let sizeInGb = $state('10');
  let isPublished = $state(true);
  let isDlc = $state(false);
  let parentGameId = $state<string>('');
  let selectedGenres = $state<string[]>([]);
  let selectedPlatforms = $state<string[]>(['Windows']);
  let selectedFeatures = $state<string[]>([]);
  let tagsText = $state('');
  
  let headerImageUrl = $state('');
  let coverImageUrl = $state('');
  let screenshotUrls = $state<string[]>([]);
  let trailerUrl = $state('');

  let isUploadingCover = $state(false);
  let isUploadingHeader = $state(false);
  let isUploadingScreens = $state(false);
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  const availableGenres = [
    'Action', 'RPG', 'Strategy', 'Adventure', 'Shooter', 
    'Indie', 'Simulation', 'MMO', 'Horror', 'Sports', 'Racing', 'Cyberpunk', 'SciFi', 'Fantasy'
  ];

  const availablePlatforms = ['Windows', 'MacOS', 'Linux'];
  const availableFeatures = ['SinglePlayer', 'MultiPlayer', 'CoOp', 'VR', 'Achievements', 'CloudSaves'];

  const availableBaseGames = $derived(
    allGames.filter((g) => !g.isDlc && g.id !== game?.id)
  );

  const attachedDlcs = $derived(
    game ? allGames.filter((g) => g.isDlc && g.parentGameId === game.id) : []
  );

  $effect(() => {
    if (game) {
      title = game.title || '';
      description = game.description || '';
      shortDescription = game.shortDescription || '';
      priceInTon = ((Number(game.priceInNanoTons) || 0) / 1_000_000_000).toFixed(2);
      discountPercentage = game.discountPercentage || 0;
      serverArchivePath = game.serverArchivePath || '';
      version = game.version || '1.0.0';
      sizeInGb = ((Number(game.sizeInBytes) || 0) / (1024 * 1024 * 1024)).toFixed(1);
      isPublished = game.isPublished ?? true;
      isDlc = !!game.isDlc;
      parentGameId = game.parentGameId || '';
      selectedGenres = game.genres ? [...game.genres] : [];
      selectedPlatforms = game.platforms ? [...game.platforms] : ['Windows'];
      selectedFeatures = game.features ? [...game.features] : [];
      tagsText = (game.tags || []).join(', ');
      headerImageUrl = game.headerImageUrl || '';
      coverImageUrl = game.coverImageUrl || '';
      screenshotUrls = game.screenshotUrls ? [...game.screenshotUrls] : [];
      trailerUrl = game.trailerUrl || '';
    }
  });

  function toggleGenre(genre: string) {
    if (selectedGenres.includes(genre)) {
      selectedGenres = selectedGenres.filter(g => g !== genre);
    } else {
      selectedGenres = [...selectedGenres, genre];
    }
  }

  function togglePlatform(platform: string) {
    if (selectedPlatforms.includes(platform)) {
      selectedPlatforms = selectedPlatforms.filter(p => p !== platform);
    } else {
      selectedPlatforms = [...selectedPlatforms, platform];
    }
  }

  function toggleFeature(feature: string) {
    if (selectedFeatures.includes(feature)) {
      selectedFeatures = selectedFeatures.filter(f => f !== feature);
    } else {
      selectedFeatures = [...selectedFeatures, feature];
    }
  }

  async function handleCoverUpload(e: Event) {
    const input = e.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];
    isUploadingCover = true;
    try {
      const res = await gamesService.uploadImage(file);
      coverImageUrl = res.url;
      uiStore.addToast({ title: 'Постер завантажено', message: 'Зображення постера успішно оновлено.', type: 'success' });
    } catch (err: any) {
      uiStore.addToast({ title: 'Помилка завантаження', message: err.message, type: 'error' });
    } finally {
      isUploadingCover = false;
      input.value = '';
    }
  }

  async function handleHeaderUpload(e: Event) {
    const input = e.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];
    isUploadingHeader = true;
    try {
      const res = await gamesService.uploadImage(file);
      headerImageUrl = res.url;
      uiStore.addToast({ title: 'Банер завантажено', message: 'Зображення банера успішно оновлено.', type: 'success' });
    } catch (err: any) {
      uiStore.addToast({ title: 'Помилка завантаження', message: err.message, type: 'error' });
    } finally {
      isUploadingHeader = false;
      input.value = '';
    }
  }

  async function handleScreenshotsUpload(e: Event) {
    const input = e.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const files = Array.from(input.files);
    isUploadingScreens = true;
    try {
      for (const file of files) {
        const res = await gamesService.uploadImage(file);
        screenshotUrls = [...screenshotUrls, res.url];
      }
      uiStore.addToast({ title: 'Скріншоти завантажено', message: `Завантажено ${files.length} нових скріншот(ів).`, type: 'success' });
    } catch (err: any) {
      uiStore.addToast({ title: 'Помилка завантаження', message: err.message, type: 'error' });
    } finally {
      isUploadingScreens = false;
      input.value = '';
    }
  }

  function removeScreenshot(index: number) {
    screenshotUrls = screenshotUrls.filter((_, i) => i !== index);
  }

  async function handleSubmit(e: Event) {
    e.preventDefault();
    if (!game) return;

    isSubmitting = true;
    errorMessage = '';

    try {
      const priceInNanoTons = Math.floor((parseFloat(priceInTon) || 0) * 1_000_000_000);
      const sizeInBytes = Math.floor((parseFloat(sizeInGb) || 0) * 1024 * 1024 * 1024);
      const tags = tagsText
        .split(',')
        .map((t) => t.trim())
        .filter(Boolean);

      const payload: UpdateGameDto = {
        title: title.trim() || undefined,
        description: description.trim() || undefined,
        shortDescription: shortDescription.trim() || undefined,
        priceInNanoTons,
        discountPercentage,
        serverArchivePath: serverArchivePath.trim() || undefined,
        version: version.trim() || undefined,
        sizeInBytes,
        isPublished,
        isDlc,
        parentGameId: isDlc && parentGameId ? parentGameId : undefined,
        genres: selectedGenres,
        platforms: selectedPlatforms,
        features: selectedFeatures,
        tags,
        headerImageUrl: headerImageUrl.trim() || undefined,
        coverImageUrl: coverImageUrl.trim() || undefined,
        screenshotUrls,
        trailerUrl: trailerUrl.trim() || undefined,
      };

      await adminService.updateGame(game.id, payload);

      uiStore.addToast({
        title: 'Гра оновлена',
        message: `Гра '${title}' успішно оновлена!`,
        type: 'success',
      });

      onGameUpdated();
      onClose();
    } catch (err: any) {
      errorMessage = err.message || 'Помилка оновлення гри.';
    } finally {
      isSubmitting = false;
    }
  }

  async function handleDetachDlc(dlcGame: Game) {
    try {
      await adminService.updateGame(dlcGame.id, { parentGameId: undefined, isDlc: false });
      uiStore.addToast({
        title: 'DLC відв\'язано',
        message: `DLC '${dlcGame.title}' відв'язано від базової гри.`,
        type: 'info',
      });
      onGameUpdated();
    } catch (err: any) {
      uiStore.addToast({ title: 'Помилка', message: err.message, type: 'error' });
    }
  }
</script>

{#if isOpen && game}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in">
    <div class="relative w-full max-w-3xl bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl shadow-cyan-950/50 p-6 overflow-hidden max-h-[90vh] flex flex-col">
      <div class="flex items-center justify-between pb-4 border-b border-cyan-950">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-500/20 to-blue-500/20 border border-cyan-500/40 flex items-center justify-center text-cyan-400">
            <Edit class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-white tracking-wide flex items-center gap-2">
              Редагування гри
              <span class="text-xs px-2 py-0.5 rounded-full bg-cyan-950 text-cyan-400 border border-cyan-500/30">
                ID: {game.id.slice(0, 8)}...
              </span>
              {#if isDlc}
                <span class="text-xs px-2 py-0.5 rounded-full bg-purple-950 text-purple-300 border border-purple-500/40 font-bold uppercase">
                  DLC Mode
                </span>
              {/if}
            </h3>
            <p class="text-xs text-slate-400">Завантаження зображень, налаштування знижок, жанрів та статусу гри</p>
          </div>
        </div>
        <button
          onclick={onClose}
          class="p-2 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800/60 transition-colors cursor-pointer"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <form onsubmit={handleSubmit} class="flex-1 overflow-y-auto pr-1 py-4 space-y-4">
        {#if errorMessage}
          <div class="p-3 rounded-xl bg-rose-950/50 border border-rose-500/40 text-rose-300 text-xs flex items-center gap-2">
            <span>⚠</span>
            <span>{errorMessage}</span>
          </div>
        {/if}

        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div class="md:col-span-2">
            <label for="edit-game-title" class="block text-xs font-semibold text-slate-300 mb-1">Назва гри *</label>
            <input
              id="edit-game-title"
              type="text"
              required
              bind:value={title}
              class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white"
            />
          </div>

          <div>
            <label for="edit-game-price" class="block text-xs font-semibold text-slate-300 mb-1">Ціна (TON)</label>
            <div class="relative">
              <DollarSign class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-cyan-400" />
              <input
                id="edit-game-price"
                type="number"
                step="0.1"
                min="0"
                bind:value={priceInTon}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-cyan-300 font-mono"
              />
            </div>
          </div>

          <div>
            <label for="edit-game-discount" class="block text-xs font-semibold text-slate-300 mb-1">Знижка (%)</label>
            <div class="relative">
              <Percent class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-rose-400" />
              <input
                id="edit-game-discount"
                type="number"
                min="0"
                max="100"
                bind:value={discountPercentage}
                placeholder="40"
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-rose-500/30 focus:border-rose-400 focus:outline-none text-xs text-rose-300 font-mono font-bold"
              />
            </div>
          </div>
        </div>

        <div>
          <label for="edit-game-short-desc" class="block text-xs font-semibold text-slate-300 mb-1">Короткий опис / Теглайн</label>
          <input
            id="edit-game-short-desc"
            type="text"
            bind:value={shortDescription}
            class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200"
          />
        </div>

        <div>
          <label for="edit-game-desc" class="block text-xs font-semibold text-slate-300 mb-1">Повний опис</label>
          <textarea
            id="edit-game-desc"
            rows="3"
            bind:value={description}
            class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 resize-none"
          ></textarea>
        </div>

        <div>
          <div class="block text-xs font-semibold text-slate-300 mb-1.5">Жанри гри (виберіть зі списку):</div>
          <div class="flex flex-wrap gap-1.5 p-3 rounded-xl bg-[#0a232c] border border-cyan-500/20">
            {#each availableGenres as g}
              <button
                type="button"
                onclick={() => toggleGenre(g)}
                class="px-2.5 py-1 rounded-lg text-xs font-medium transition-all cursor-pointer
                  {selectedGenres.includes(g)
                    ? 'bg-gradient-to-r from-cyan-500 to-emerald-500 text-black font-bold shadow-md shadow-cyan-500/20'
                    : 'bg-[#061820] text-slate-400 border border-slate-700 hover:text-white'}"
              >
                {g}
              </button>
            {/each}
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="p-3 rounded-xl bg-[#0a232c] border border-cyan-500/20">
            <div class="block text-xs font-semibold text-slate-300 mb-2">Підтримувані платформи:</div>
            <div class="flex gap-4">
              {#each availablePlatforms as plat}
                <label class="flex items-center gap-2 text-xs text-slate-300 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={selectedPlatforms.includes(plat)}
                    onchange={() => togglePlatform(plat)}
                    class="accent-cyan-400 cursor-pointer"
                  />
                  <span>{plat}</span>
                </label>
              {/each}
            </div>
          </div>

          <div class="p-3 rounded-xl bg-[#0a232c] border border-cyan-500/20">
            <div class="block text-xs font-semibold text-slate-300 mb-2">Особливості:</div>
            <div class="flex flex-wrap gap-2">
              {#each availableFeatures as feat}
                <label class="flex items-center gap-1.5 text-xs text-slate-300 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={selectedFeatures.includes(feat)}
                    onchange={() => toggleFeature(feat)}
                    class="accent-cyan-400 cursor-pointer"
                  />
                  <span>{feat}</span>
                </label>
              {/each}
            </div>
          </div>
        </div>

        <div>
          <label for="edit-game-tags" class="block text-xs font-semibold text-slate-300 mb-1">Пошукові теги (через кому)</label>
          <div class="relative">
            <Tag class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
            <input
              id="edit-game-tags"
              type="text"
              placeholder="Open World, Story Rich, Ray Tracing, Cyberpunk..."
              bind:value={tagsText}
              class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200"
            />
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label for="edit-game-archive" class="block text-xs font-semibold text-slate-300 mb-1">Шлях до архіву гри</label>
            <div class="relative">
              <FileArchive class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="edit-game-archive"
                type="text"
                bind:value={serverArchivePath}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 font-mono"
              />
            </div>
          </div>

          <div>
            <label for="edit-game-version" class="block text-xs font-semibold text-slate-300 mb-1">Версія</label>
            <input
              id="edit-game-version"
              type="text"
              bind:value={version}
              class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 font-mono"
            />
          </div>

          <div>
            <label for="edit-game-size" class="block text-xs font-semibold text-slate-300 mb-1">Розмір (GB)</label>
            <input
              id="edit-game-size"
              type="number"
              step="0.1"
              min="0"
              bind:value={sizeInGb}
              class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 font-mono"
            />
          </div>
        </div>

        <div class="p-4 rounded-xl bg-[#08202b] border border-cyan-500/20 space-y-4">
          <div class="text-xs font-bold text-white flex items-center gap-2">
            <Image class="w-4 h-4 text-cyan-400" />
            <span>Медіафайли гри (Завантаження власних фото)</span>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="p-3 rounded-xl bg-[#0a232c] border border-cyan-500/20 space-y-2">
              <label for="edit-cover-file-input" class="block text-xs font-semibold text-slate-300">
                Постер гри (3:4)
              </label>

              <div class="flex items-center gap-3">
                <div class="relative w-14 h-20 rounded-lg overflow-hidden bg-slate-950 border border-cyan-500/40 shrink-0">
                  {#if coverImageUrl}
                    <img src={coverImageUrl} alt="Cover preview" class="w-full h-full object-cover" />
                  {:else}
                    <div class="w-full h-full flex items-center justify-center text-slate-500 text-[10px]">3:4</div>
                  {/if}
                  {#if isUploadingCover}
                    <div class="absolute inset-0 bg-black/70 flex items-center justify-center">
                      <span class="animate-spin text-cyan-400 text-xs">⟳</span>
                    </div>
                  {/if}
                </div>

                <div class="flex-1 space-y-1.5">
                  <label class="flex items-center justify-center gap-2 px-3 py-2 rounded-xl bg-cyan-500/20 hover:bg-cyan-500 text-cyan-300 hover:text-black border border-cyan-500/40 text-xs font-bold transition-all cursor-pointer">
                    <Upload class="w-3.5 h-3.5" />
                    <span>{isUploadingCover ? 'Завантаження...' : 'Завантажити постер'}</span>
                    <input id="edit-cover-file-input" type="file" accept="image/*" onchange={handleCoverUpload} class="hidden" disabled={isUploadingCover} />
                  </label>
                  <input
                    type="text"
                    placeholder="Або введіть URL..."
                    bind:value={coverImageUrl}
                    class="w-full px-2.5 py-1.5 rounded-lg bg-[#061820] border border-cyan-500/20 text-[11px] text-slate-300"
                  />
                </div>
              </div>
            </div>

            <div class="p-3 rounded-xl bg-[#0a232c] border border-cyan-500/20 space-y-2">
              <label for="edit-header-file-input" class="block text-xs font-semibold text-slate-300">
                Банер гри (16:9)
              </label>

              <div class="flex items-center gap-3">
                <div class="relative w-24 h-14 rounded-lg overflow-hidden bg-slate-950 border border-cyan-500/40 shrink-0">
                  {#if headerImageUrl}
                    <img src={headerImageUrl} alt="Header preview" class="w-full h-full object-cover" />
                  {:else}
                    <div class="w-full h-full flex items-center justify-center text-slate-500 text-[10px]">16:9</div>
                  {/if}
                  {#if isUploadingHeader}
                    <div class="absolute inset-0 bg-black/70 flex items-center justify-center">
                      <span class="animate-spin text-cyan-400 text-xs">⟳</span>
                    </div>
                  {/if}
                </div>

                <div class="flex-1 space-y-1.5">
                  <label class="flex items-center justify-center gap-2 px-3 py-2 rounded-xl bg-cyan-500/20 hover:bg-cyan-500 text-cyan-300 hover:text-black border border-cyan-500/40 text-xs font-bold transition-all cursor-pointer">
                    <Upload class="w-3.5 h-3.5" />
                    <span>{isUploadingHeader ? 'Завантаження...' : 'Завантажити банер'}</span>
                    <input id="edit-header-file-input" type="file" accept="image/*" onchange={handleHeaderUpload} class="hidden" disabled={isUploadingHeader} />
                  </label>
                  <input
                    type="text"
                    placeholder="Або введіть URL..."
                    bind:value={headerImageUrl}
                    class="w-full px-2.5 py-1.5 rounded-lg bg-[#061820] border border-cyan-500/20 text-[11px] text-slate-300"
                  />
                </div>
              </div>
            </div>
          </div>

          <div class="p-3 rounded-xl bg-[#0a232c] border border-cyan-500/20 space-y-3">
            <div class="flex items-center justify-between">
              <label for="edit-screens-file-input" class="text-xs font-semibold text-slate-300">
                Скріншоти гри ({screenshotUrls.length})
              </label>

              <label class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-cyan-500/20 hover:bg-cyan-500 text-cyan-300 hover:text-black border border-cyan-500/40 text-xs font-bold transition-all cursor-pointer">
                <Upload class="w-3.5 h-3.5" />
                <span>{isUploadingScreens ? 'Завантаження...' : '+ Додати скріншоти'}</span>
                <input id="edit-screens-file-input" type="file" accept="image/*" multiple onchange={handleScreenshotsUpload} class="hidden" disabled={isUploadingScreens} />
              </label>
            </div>

            {#if screenshotUrls.length > 0}
              <div class="grid grid-cols-2 sm:grid-cols-4 gap-2.5 max-h-36 overflow-y-auto pr-1">
                {#each screenshotUrls as url, idx}
                  <div class="relative group rounded-lg overflow-hidden border border-cyan-500/30 aspect-video bg-slate-950">
                    <img src={url} alt="Screenshot {idx + 1}" class="w-full h-full object-cover" />
                    <button
                      type="button"
                      onclick={() => removeScreenshot(idx)}
                      class="absolute top-1 right-1 p-1 rounded-md bg-rose-600/90 text-white opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer"
                      title="Видалити скріншот"
                    >
                      <Trash2 class="w-3 h-3" />
                    </button>
                  </div>
                {/each}
              </div>
            {:else}
              <div class="text-center py-4 bg-[#061820] rounded-lg border border-dashed border-slate-700 text-slate-500 text-xs">
                Скріншоти ще не завантажені. Натисніть кнопку вище, щоб вибрати файли зі свого комп'ютера.
              </div>
            {/if}
          </div>

          <div>
            <label for="edit-game-trailer" class="block text-xs font-semibold text-slate-300 mb-1">URL трейлера (YouTube / MP4)</label>
            <div class="relative">
              <Film class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="edit-game-trailer"
                type="text"
                bind:value={trailerUrl}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200"
              />
            </div>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-3 pt-2">
          <div class="p-3.5 rounded-xl bg-[#081e26] border border-cyan-500/20 flex items-center justify-between">
            <div>
              <div class="text-xs font-bold text-white">Видимість у крамниці</div>
              <div class="text-[10px] text-slate-400">Опубліковано для всіх гравців</div>
            </div>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" bind:checked={isPublished} class="sr-only peer" />
              <div class="w-10 h-5 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-emerald-500"></div>
            </label>
          </div>

          <div class="p-3.5 rounded-xl bg-[#081e26] border border-purple-500/30 flex items-center justify-between">
            <div>
              <div class="text-xs font-bold text-white flex items-center gap-1.5">
                <span>DLC / Доповнення</span>
                <span class="text-[9px] px-1.5 py-0.2 rounded bg-purple-950 text-purple-300 border border-purple-500/40 uppercase font-bold">DLC</span>
              </div>
              <div class="text-[10px] text-slate-400">Позначити гру як аддон до іншої гри</div>
            </div>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" bind:checked={isDlc} class="sr-only peer" />
              <div class="w-10 h-5 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-purple-600"></div>
            </label>
          </div>
        </div>

        {#if isDlc}
          <div class="p-4 rounded-xl bg-purple-950/30 border border-purple-500/40 animate-in fade-in">
            <label for="parent-game-select" class="block text-xs font-bold text-purple-300 mb-1.5 flex items-center gap-1.5">
              <Layers class="w-4 h-4 text-purple-400" />
              <span>Базова гра (Parent Base Game) *</span>
            </label>
            <p class="text-[11px] text-slate-400 mb-2">Виберіть основну гру, до якої належить це доповнення:</p>
            <select
              id="parent-game-select"
              bind:value={parentGameId}
              class="w-full px-3 py-2.5 rounded-xl bg-[#0a232c] border border-purple-500/40 focus:border-purple-400 focus:outline-none text-xs text-white"
            >
              <option value="">-- Виберіть базову гру --</option>
              {#each availableBaseGames as baseGame}
                <option value={baseGame.id}>{baseGame.title} (v{baseGame.version})</option>
              {/each}
            </select>
          </div>
        {/if}

        {#if !isDlc}
          <div class="p-4 rounded-xl bg-[#08202b] border border-cyan-500/20">
            <div class="flex items-center justify-between mb-3">
              <div class="text-xs font-bold text-white flex items-center gap-2">
                <Layers class="w-4 h-4 text-cyan-400" />
                <span>Прив'язані DLC та доповнення ({attachedDlcs.length}):</span>
              </div>
            </div>

            {#if attachedDlcs.length > 0}
              <div class="space-y-2 max-h-48 overflow-y-auto pr-1">
                {#each attachedDlcs as dlc}
                  <div class="p-2.5 rounded-xl bg-[#0a232c] border border-purple-500/30 flex items-center justify-between gap-3">
                    <div class="flex items-center gap-2.5">
                      <img src={dlc.coverImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=100'} alt={dlc.title} class="w-8 h-10 rounded-lg object-cover border border-purple-500/40 shrink-0" />
                      <div>
                        <div class="text-xs font-bold text-white flex items-center gap-1.5">
                          <span>{dlc.title}</span>
                          <span class="text-[9px] px-1 py-0.2 rounded bg-purple-950 text-purple-300 font-mono">DLC</span>
                        </div>
                        <div class="text-[10px] text-cyan-300 font-mono">{formatPrice(dlc.priceInNanoTons)} • v{dlc.version}</div>
                      </div>
                    </div>

                    <div class="flex items-center gap-1.5">
                      {#if onOpenEditOtherGame}
                        <button
                          type="button"
                          onclick={() => onOpenEditOtherGame(dlc)}
                          class="p-1.5 rounded-lg bg-slate-800 hover:bg-cyan-500 hover:text-black text-slate-300 transition-colors cursor-pointer"
                          title="Редагувати це DLC"
                        >
                          <Edit class="w-3.5 h-3.5" />
                        </button>
                      {/if}
                      <button
                        type="button"
                        onclick={() => handleDetachDlc(dlc)}
                        class="p-1.5 rounded-lg bg-slate-800 hover:bg-rose-600 hover:text-white text-slate-400 transition-colors cursor-pointer"
                        title="Відв'язати DLC від цієї гри"
                      >
                        <Unlink class="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>
                {/each}
              </div>
            {:else}
              <div class="text-center py-4 bg-[#061820] rounded-xl border border-dashed border-slate-700/60 text-slate-500 text-xs">
                До цієї гри поки не прив'язано жодного DLC. Ви можете створити або змінити гру, увімкнувши прапорець 'DLC / Доповнення'.
              </div>
            {/if}
          </div>
        {/if}

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-cyan-950">
          <button
            type="button"
            onclick={onClose}
            class="px-4 py-2 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-xs font-semibold text-slate-300 transition-colors cursor-pointer"
          >
            Скасувати
          </button>
          <button
            type="submit"
            disabled={isSubmitting || isUploadingCover || isUploadingHeader || isUploadingScreens}
            class="flex items-center gap-2 px-5 py-2 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-bold text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer disabled:opacity-50"
          >
            {#if isSubmitting}
              <span class="animate-spin">⟳</span>
              <span>Збереження...</span>
            {:else}
              <CheckCircle2 class="w-4 h-4" />
              <span>Зберегти зміни</span>
            {/if}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}
