<script lang="ts">
import { adminService, type CreateGameDto } from '../../services/adminService';
  import { gamesService } from '../../services/gamesService';
  import { uiStore } from '../../stores/uiStore';
  import type { Game } from '../../types';
  import { X, Gamepad2, Sparkles, Image, Film, FileArchive, Tag, DollarSign, Layers, Percent, Upload, Trash2, CheckCircle2 } from 'lucide-svelte';

  interface Props {
    allGames?: Game[];
    isOpen: boolean;
    onClose: () => void;
    onGameCreated: () => void;
  }

  let { allGames = [], isOpen, onClose, onGameCreated }: Props = $props();

  let title = $state('');
  let description = $state('');
  let shortDescription = $state('');
  let priceInTon = $state('5.0');
  let discountPercentage = $state(0);
  let serverArchivePath = $state('/storage/games/new-game-v1.0.0.zip');
  let version = $state('1.0.0');
  let sizeInGb = $state('12.5');
  let isPublished = $state(true);
  let isDlc = $state(false);
  let parentGameId = $state('');
  let selectedGenres = $state<string[]>(['Action', 'RPG']);
  let selectedPlatforms = $state<string[]>(['Windows']);
  let selectedFeatures = $state<string[]>(['SinglePlayer']);
  let tagsText = $state('Cyberpunk, Open World, Story Rich');

  let headerImageUrl = $state('https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1200&auto=format&fit=crop&q=80');
  let coverImageUrl = $state('https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=800&auto=format&fit=crop&q=80');
  let screenshotUrls = $state<string[]>([
    'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800',
    'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800'
  ]);
  let trailerUrl = $state('https://www.youtube.com/watch?v=dQw4w9WgXcQ');

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

  const baseGames = $derived(allGames.filter((g) => !g.isDlc));

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
      uiStore.addToast({ title: 'Постер завантажено', message: 'Зображення постера успішно збережено.', type: 'success' });
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
      uiStore.addToast({ title: 'Банер завантажено', message: 'Зображення банера успішно збережено.', type: 'success' });
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
      uiStore.addToast({ title: 'Скріншоти завантажено', message: `Завантажено ${files.length} скріншот(ів).`, type: 'success' });
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
    if (!title || !description || !serverArchivePath) {
      errorMessage = 'Назва, опис та шлях до архіву є обов\'язковими.';
      return;
    }

    isSubmitting = true;
    errorMessage = '';

    try {
      const priceInNanoTons = Math.floor((parseFloat(priceInTon) || 0) * 1_000_000_000);
      const sizeInBytes = Math.floor((parseFloat(sizeInGb) || 1) * 1024 * 1024 * 1024);
      const tags = tagsText
        .split(',')
        .map((t) => t.trim())
        .filter(Boolean);

      const payload: CreateGameDto = {
        title: title.trim(),
        description: description.trim(),
        shortDescription: shortDescription.trim() || undefined,
        priceInNanoTons,
        discountPercentage,
        serverArchivePath: serverArchivePath.trim(),
        version: version.trim() || '1.0.0',
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

      await adminService.createGame(payload);

      uiStore.addToast({
        title: 'Гру створено',
        message: `Гру '${title}' успішно додано до каталогу Dteam!`,
        type: 'success',
      });

      onGameCreated();
      onClose();
    } catch (err: any) {
      errorMessage = err.message || 'Помилка створення гри.';
    } finally {
      isSubmitting = false;
    }
  }
</script>

{#if isOpen}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in">
    <div class="relative w-full max-w-3xl bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl shadow-cyan-950/50 p-6 overflow-hidden max-h-[90vh] flex flex-col">
      <div class="flex items-center justify-between pb-4 border-b border-cyan-950">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-500/20 to-emerald-500/20 border border-cyan-500/40 flex items-center justify-center text-cyan-400">
            <Gamepad2 class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-white tracking-wide flex items-center gap-2">
              Створення нової гри
              <span class="text-xs px-2 py-0.5 rounded-full bg-cyan-950 text-cyan-400 border border-cyan-500/30">
                Каталог Dteam
              </span>
            </h3>
            <p class="text-xs text-slate-400">Завантаження зображень, налаштування знижок, платформ та тегів</p>
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
            <label for="create-game-title" class="block text-xs font-semibold text-slate-300 mb-1">Назва гри *</label>
            <input
              id="create-game-title"
              type="text"
              required
              placeholder="e.g. Cyberpunk Odyssey 2077"
              bind:value={title}
              class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-500"
            />
          </div>

          <div>
            <label for="create-game-price" class="block text-xs font-semibold text-slate-300 mb-1">Ціна (TON)</label>
            <div class="relative">
              <DollarSign class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-cyan-400" />
              <input
                id="create-game-price"
                type="number"
                step="0.1"
                min="0"
                placeholder="5.0"
                bind:value={priceInTon}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-cyan-300 font-mono"
              />
            </div>
          </div>

          <div>
            <label for="create-game-discount" class="block text-xs font-semibold text-slate-300 mb-1">Знижка (%)</label>
            <div class="relative">
              <Percent class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-rose-400" />
              <input
                id="create-game-discount"
                type="number"
                min="0"
                max="100"
                bind:value={discountPercentage}
                placeholder="0"
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-rose-500/30 focus:border-rose-400 focus:outline-none text-xs text-rose-300 font-mono font-bold"
              />
            </div>
          </div>
        </div>

        <div>
          <label for="create-game-short-desc" class="block text-xs font-semibold text-slate-300 mb-1">Короткий опис / Теглайн</label>
          <input
            id="create-game-short-desc"
            type="text"
            placeholder="Захоплююча тактична RPG у неоновому мегаполісі..."
            bind:value={shortDescription}
            class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 placeholder-slate-500"
          />
        </div>

        <div>
          <label for="create-game-desc" class="block text-xs font-semibold text-slate-300 mb-1">Повний опис *</label>
          <textarea
            id="create-game-desc"
            required
            rows="3"
            placeholder="Детальний опис ігроладу, сюжету та можливостей..."
            bind:value={description}
            class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 placeholder-slate-500 resize-none"
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
          <label for="create-game-tags" class="block text-xs font-semibold text-slate-300 mb-1">Теги (через кому)</label>
          <div class="relative">
            <Tag class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
            <input
              id="create-game-tags"
              type="text"
              placeholder="Action, Sci-Fi, Singleplayer..."
              bind:value={tagsText}
              class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200"
            />
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label for="create-game-archive" class="block text-xs font-semibold text-slate-300 mb-1">Шлях до архіву *</label>
            <div class="relative">
              <FileArchive class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="create-game-archive"
                type="text"
                required
                placeholder="/storage/games/..."
                bind:value={serverArchivePath}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 font-mono"
              />
            </div>
          </div>

          <div>
            <label for="create-game-version" class="block text-xs font-semibold text-slate-300 mb-1">Версія</label>
            <input
              id="create-game-version"
              type="text"
              placeholder="1.0.0"
              bind:value={version}
              class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 font-mono"
            />
          </div>

          <div>
            <label for="create-game-size" class="block text-xs font-semibold text-slate-300 mb-1">Розмір (GB)</label>
            <input
              id="create-game-size"
              type="number"
              step="0.5"
              min="0.1"
              placeholder="15.0"
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
              <label for="cover-file-input" class="block text-xs font-semibold text-slate-300">
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
                    <input id="cover-file-input" type="file" accept="image/*" onchange={handleCoverUpload} class="hidden" disabled={isUploadingCover} />
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
              <label for="header-file-input" class="block text-xs font-semibold text-slate-300">
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
                    <input id="header-file-input" type="file" accept="image/*" onchange={handleHeaderUpload} class="hidden" disabled={isUploadingHeader} />
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
              <label for="screens-file-input" class="text-xs font-semibold text-slate-300">
                Скріншоти гри ({screenshotUrls.length})
              </label>

              <label class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-cyan-500/20 hover:bg-cyan-500 text-cyan-300 hover:text-black border border-cyan-500/40 text-xs font-bold transition-all cursor-pointer">
                <Upload class="w-3.5 h-3.5" />
                <span>{isUploadingScreens ? 'Завантаження...' : '+ Додати скріншоти'}</span>
                <input id="screens-file-input" type="file" accept="image/*" multiple onchange={handleScreenshotsUpload} class="hidden" disabled={isUploadingScreens} />
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
            <label for="create-game-trailer" class="block text-xs font-semibold text-slate-300 mb-1">URL трейлера (YouTube / MP4)</label>
            <div class="relative">
              <Film class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="create-game-trailer"
                type="text"
                placeholder="https://www.youtube.com/watch?v=..."
                bind:value={trailerUrl}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200"
              />
            </div>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-3 pt-2">
          <div class="p-3.5 rounded-xl bg-[#081e26] border border-cyan-500/20 flex items-center justify-between">
            <div>
              <div class="text-xs font-bold text-white">Опублікувати в крамниці</div>
              <div class="text-[10px] text-slate-400">Гра буде видимою в каталозі відразу</div>
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
              <div class="text-[10px] text-slate-400">Позначити як додатковий контент</div>
            </div>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" bind:checked={isDlc} class="sr-only peer" />
              <div class="w-10 h-5 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-purple-600"></div>
            </label>
          </div>
        </div>

        {#if isDlc}
          <div class="p-4 rounded-xl bg-purple-950/30 border border-purple-500/40 animate-in fade-in">
            <label for="create-parent-game-select" class="block text-xs font-bold text-purple-300 mb-1.5 flex items-center gap-1.5">
              <Layers class="w-4 h-4 text-purple-400" />
              <span>Базова гра (Parent Base Game) *</span>
            </label>
            <p class="text-[11px] text-slate-400 mb-2">Виберіть основну гру для цього DLC:</p>
            <select
              id="create-parent-game-select"
              bind:value={parentGameId}
              class="w-full px-3 py-2.5 rounded-xl bg-[#0a232c] border border-purple-500/40 focus:border-purple-400 focus:outline-none text-xs text-white"
            >
              <option value="">-- Виберіть базову гру --</option>
              {#each baseGames as baseGame}
                <option value={baseGame.id}>{baseGame.title} (v{baseGame.version})</option>
              {/each}
            </select>
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
              <span>Публікація...</span>
            {:else}
              <Sparkles class="w-4 h-4" />
              <span>Опублікувати гру</span>
            {/if}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}

