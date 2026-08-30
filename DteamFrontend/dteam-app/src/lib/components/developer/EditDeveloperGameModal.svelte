<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { developerStore } from '../../stores/developerStore';
  import { tonToNanoTon, nanoTonToTon } from '../../utils/formatters';
  import { mediaService } from '../../services/mediaService';
  import {
    X,
    Upload,
    Edit3,
    Sparkles,
    FileArchive,
    Loader2
  } from 'lucide-svelte';

  let title = $state('');
  let description = $state('');
  let shortDescription = $state('');
  let priceTon = $state('0');
  let discountPercentage = $state(0);
  let version = $state('1.0.0');
  let archivePath = $state('');
  let isPublished = $state(true);

  // Media
  let coverImageUrl = $state('');
  let headerImageUrl = $state('');
  let screenshotUrls = $state<string[]>([]);
  let isUploadingCover = $state(false);
  let isUploadingScreenshot = $state(false);

  // Genres & Platforms
  const availableGenres = [
    'Action',
    'RPG',
    'Strategy',
    'Cyberpunk',
    'Indie',
    'Adventure',
    'Shooter',
    'Casual',
    'Racing',
    'Simulation'
  ];
  let selectedGenres = $state<string[]>([]);

  const availablePlatforms = ['Windows', 'macOS', 'Linux'];
  let selectedPlatforms = $state<string[]>([]);

  let tagsInput = $state('');

  let lastLoadedGameId = $state<string | null>(null);

  $effect(() => {
    const isOpen = $uiStore.isEditGameModalOpen;
    const game = $uiStore.editingGame;

    if (isOpen && game && game.id !== lastLoadedGameId) {
      lastLoadedGameId = game.id;
      title = game.title || '';
      description = game.description || '';
      shortDescription = game.shortDescription || '';
      priceTon = nanoTonToTon(game.priceInNanoTons).toString();
      discountPercentage = game.discountPercentage || 0;
      version = game.version || '1.0.0';
      archivePath = game.serverArchivePath || '';
      isPublished = game.isPublished ?? true;
      coverImageUrl = game.coverImageUrl || '';
      headerImageUrl = game.headerImageUrl || '';
      screenshotUrls = game.screenshotUrls ? [...game.screenshotUrls] : [];
      selectedGenres = game.genres ? [...game.genres] : ['Indie'];
      selectedPlatforms = game.platforms ? [...game.platforms] : ['Windows'];
      tagsInput = game.tags ? game.tags.join(', ') : '';
    } else if (!isOpen) {
      lastLoadedGameId = null;
    }
  });

  function close() {
    uiStore.setEditGameModal(false);
  }

  function toggleGenre(genre: string) {
    if (selectedGenres.includes(genre)) {
      selectedGenres = selectedGenres.filter((g) => g !== genre);
    } else {
      selectedGenres = [...selectedGenres, genre];
    }
  }

  function togglePlatform(platform: string) {
    if (selectedPlatforms.includes(platform)) {
      if (selectedPlatforms.length > 1) {
        selectedPlatforms = selectedPlatforms.filter((p) => p !== platform);
      }
    } else {
      selectedPlatforms = [...selectedPlatforms, platform];
    }
  }

  async function handleCoverUpload(e: Event) {
    const input = e.target as HTMLInputElement;
    const files = input.files;
    if (!files || files.length === 0) return;
    isUploadingCover = true;
    try {
      const res = await mediaService.upload(files[0]);
      coverImageUrl = res.url;
      headerImageUrl = res.url;
      uiStore.addToast({
        title: 'Обкладинку завантажено',
        message: 'Зображення додано. Натисніть «Зберегти зміни» для застосування.',
        type: 'success',
      });
    } catch (err: any) {
      uiStore.addToast({
        title: 'Помилка завантаження',
        message: err?.message || 'Не вдалося завантажити зображення.',
        type: 'error',
      });
    } finally {
      isUploadingCover = false;
      input.value = '';
    }
  }

  async function handleScreenshotUpload(e: Event) {
    const input = e.target as HTMLInputElement;
    const files = input.files;
    if (!files || files.length === 0) return;
    isUploadingScreenshot = true;
    try {
      for (let i = 0; i < files.length; i++) {
        const res = await mediaService.upload(files[i]);
        screenshotUrls = [...screenshotUrls, res.url];
      }
      uiStore.addToast({
        title: 'Скріншот додано',
        message: 'Зображення додано до галереї. Натисніть «Зберегти зміни» для застосування.',
        type: 'success',
      });
    } catch (err: any) {
      uiStore.addToast({
        title: 'Помилка завантаження',
        message: err?.message || 'Не вдалося завантажити скріншот.',
        type: 'error',
      });
    } finally {
      isUploadingScreenshot = false;
      input.value = '';
    }
  }

  function removeScreenshot(index: number) {
    screenshotUrls = screenshotUrls.filter((_, i) => i !== index);
  }

  async function handleSave() {
    if (!$uiStore.editingGame) return;
    if (!title.trim() || !description.trim()) {
      uiStore.addToast({
        title: 'Обов\'язкові поля',
        message: 'Будь ласка, вкажіть назву та опис гри.',
        type: 'warning',
      });
      return;
    }

    if (isPublished && !archivePath.trim()) {
      uiStore.addToast({
        title: 'Потрібен файл білду',
        message: 'Неможливо опублікувати гру без прикріпленого архіву (.zip). Додайте архів або переведіть гру у статус чернетки.',
        type: 'warning',
      });
      return;
    }

    const priceNum = parseFloat(priceTon) || 0;
    const priceInNanoTons = tonToNanoTon(priceNum);
    const tags = tagsInput.split(',').map((t) => t.trim()).filter(Boolean);

    try {
      await developerStore.updateGame($uiStore.editingGame.id, {
        title: title.trim(),
        description: description.trim(),
        shortDescription: shortDescription.trim() || description.slice(0, 120),
        priceInNanoTons: Number(priceInNanoTons),
        discountPercentage: Math.max(0, Math.min(100, discountPercentage)),
        serverArchivePath: archivePath,
        genres: selectedGenres,
        platforms: selectedPlatforms,
        tags,
        version: version.trim() || '1.0.0',
        isPublished,
        coverImageUrl,
        headerImageUrl: headerImageUrl || coverImageUrl,
        screenshotUrls,
      });

      close();
    } catch {
      // Handled in store
    }
  }
</script>

{#if $uiStore.isEditGameModalOpen && $uiStore.editingGame}
  <div class="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4 overflow-y-auto">
    <div class="bg-[#051c27] border border-cyan-500/30 rounded-3xl w-full max-w-2xl shadow-2xl overflow-hidden my-8 animate-in fade-in zoom-in-95">
      <!-- Header -->
      <div class="p-6 bg-[#072432] border-b border-cyan-500/20 flex items-center justify-between">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-400 to-blue-600 text-black flex items-center justify-center font-black shadow-lg shadow-cyan-500/20">
            <Edit3 class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-white font-display">Редагування гри</h3>
            <span class="text-xs text-cyan-400/80 truncate block max-w-xs font-mono">
              {$uiStore.editingGame.title}
            </span>
          </div>
        </div>
        <button
          onclick={close}
          class="w-8 h-8 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-400 hover:text-white flex items-center justify-center transition-colors cursor-pointer"
        >
          <X class="w-4 h-4" />
        </button>
      </div>

      <!-- Form Body -->
      <form onsubmit={(e) => { e.preventDefault(); handleSave(); }} class="p-6 space-y-5 text-xs">
        <!-- Title & Version -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="sm:col-span-2">
            <label for="editGameTitle" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Назва гри <span class="text-cyan-400">*</span>
            </label>
            <input
              id="editGameTitle"
              type="text"
              bind:value={title}
              required
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>
          <div>
            <label for="editGameVersion" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Версія
            </label>
            <input
              id="editGameVersion"
              type="text"
              bind:value={version}
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white font-mono text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>
        </div>

        <!-- Short Description -->
        <div>
          <label for="editGameShortDesc" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Короткий опис
          </label>
          <input
            id="editGameShortDesc"
            type="text"
            bind:value={shortDescription}
            class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white text-xs focus:border-cyan-400 focus:outline-none transition-colors"
          />
        </div>

        <!-- Full Description -->
        <div>
          <label for="editGameDesc" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Повний опис <span class="text-cyan-400">*</span>
          </label>
          <textarea
            id="editGameDesc"
            rows="3"
            bind:value={description}
            required
            class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white text-xs focus:border-cyan-400 focus:outline-none transition-colors"
          ></textarea>
        </div>

        <!-- Price & Discount -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label for="editGamePrice" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Ціна в TON (0 для Free)
            </label>
            <input
              id="editGamePrice"
              type="number"
              step="0.01"
              min="0"
              bind:value={priceTon}
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white font-mono text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>

          <div>
            <label for="editGameDiscount" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Знижка (%)
            </label>
            <input
              id="editGameDiscount"
              type="number"
              min="0"
              max="100"
              bind:value={discountPercentage}
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white font-mono text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>
        </div>

        <!-- Cover Image -->
        <div>
          <span class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Обкладинка гри
          </span>
          <div class="flex items-center gap-4">
            {#if coverImageUrl}
              <img
                src={coverImageUrl}
                alt="Cover Preview"
                class="w-24 h-16 rounded-xl object-cover border border-cyan-500/40 shadow"
              />
            {/if}
            <label class="flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-xl border border-dashed border-cyan-500/40 hover:border-cyan-400 bg-[#072535]/60 hover:bg-[#072535] transition-colors cursor-pointer text-cyan-300 font-semibold">
              {#if isUploadingCover}
                <Loader2 class="w-4 h-4 animate-spin text-cyan-400" />
                <span>Завантаження...</span>
              {:else}
                <Upload class="w-4 h-4" />
                <span>Змінити обкладинку</span>
              {/if}
              <input type="file" accept="image/*" onchange={handleCoverUpload} class="hidden" />
            </label>
          </div>
        </div>

        <!-- Screenshots -->
        <div>
          <div class="flex items-center justify-between mb-1.5">
            <span class="font-bold text-slate-300 uppercase tracking-wider">
              Скріншоти ({screenshotUrls.length})
            </span>
            <label class="text-cyan-400 hover:text-cyan-300 cursor-pointer font-bold flex items-center gap-1">
              {#if isUploadingScreenshot}
                <Loader2 class="w-3.5 h-3.5 animate-spin" />
              {:else}
                <Upload class="w-3.5 h-3.5" />
              {/if}
              <span>Додати скріншоти</span>
              <input type="file" accept="image/*" multiple onchange={handleScreenshotUpload} class="hidden" />
            </label>
          </div>
          {#if screenshotUrls.length > 0}
            <div class="flex flex-wrap gap-2 pt-1">
              {#each screenshotUrls as url, idx}
                <div class="relative group w-20 h-14 rounded-lg overflow-hidden border border-cyan-500/30">
                  <img src={url} alt="Screenshot" class="w-full h-full object-cover" />
                  <button
                    type="button"
                    onclick={() => removeScreenshot(idx)}
                    class="absolute inset-0 bg-black/70 opacity-0 group-hover:opacity-100 flex items-center justify-center text-red-400 transition-opacity cursor-pointer"
                  >
                    <X class="w-4 h-4" />
                  </button>
                </div>
              {/each}
            </div>
          {/if}
        </div>

        <!-- Genres Selection -->
        <div>
          <span class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Жанри
          </span>
          <div class="flex flex-wrap gap-1.5">
            {#each availableGenres as genre}
              <button
                type="button"
                onclick={() => toggleGenre(genre)}
                class="px-2.5 py-1 rounded-lg border text-xs font-semibold transition-all cursor-pointer {selectedGenres.includes(genre) ? 'bg-[#0df2c9] text-black border-[#0df2c9] shadow-sm' : 'bg-[#072535] text-slate-400 border-cyan-500/20 hover:text-white'}"
              >
                {genre}
              </button>
            {/each}
          </div>
        </div>

        <!-- Platforms & Tags -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <span class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Платформи
            </span>
            <div class="flex gap-2">
              {#each availablePlatforms as plat}
                <button
                  type="button"
                  onclick={() => togglePlatform(plat)}
                  class="px-2.5 py-1 rounded-lg border text-xs font-semibold transition-all cursor-pointer {selectedPlatforms.includes(plat) ? 'bg-cyan-500/30 text-cyan-300 border-cyan-400' : 'bg-[#072535] text-slate-400 border-cyan-500/20'}"
                >
                  {plat}
                </button>
              {/each}
            </div>
          </div>

          <div>
            <label for="editGameTags" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Теги
            </label>
            <input
              id="editGameTags"
              type="text"
              bind:value={tagsInput}
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>
        </div>

        <!-- Publish toggle -->
        <div class="flex items-center justify-between p-3.5 rounded-xl bg-[#072535] border border-cyan-500/20">
          <div>
            <span class="font-bold text-white block">Статус публікації</span>
            <span class="text-[11px] text-slate-400">
              {isPublished ? 'Гра доступна у крамниці' : 'Гра прихована (чернетка)'}
            </span>
          </div>
          <input
            type="checkbox"
            bind:checked={isPublished}
            class="w-5 h-5 accent-[#0df2c9] cursor-pointer"
          />
        </div>

        <!-- Action Buttons -->
        <div class="flex items-center justify-end gap-3 pt-2">
          <button
            type="button"
            onclick={close}
            class="px-5 py-3 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-bold transition-all cursor-pointer"
          >
            Скасувати
          </button>

          <button
            type="submit"
            disabled={$developerStore.isSaving}
            class="px-6 py-3 rounded-xl bg-gradient-to-r from-cyan-400 to-blue-600 hover:from-cyan-300 hover:to-blue-500 text-black font-extrabold text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center gap-2 disabled:opacity-50"
          >
            {#if $developerStore.isSaving}
              <Loader2 class="w-4 h-4 animate-spin" />
              <span>Збереження...</span>
            {:else}
              <Sparkles class="w-4 h-4" />
              <span>Зберегти зміни</span>
            {/if}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}
