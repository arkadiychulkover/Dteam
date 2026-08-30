<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { developerStore } from '../../stores/developerStore';
  import { tonToNanoTon } from '../../utils/formatters';
  import { mediaService } from '../../services/mediaService';
  import {
    X,
    Upload,
    Gamepad2,
    Sparkles,
    FileArchive,
    Image as ImageIcon,
    Loader2,
    Check,
    AlertCircle
  } from 'lucide-svelte';

  let title = $state('');
  let description = $state('');
  let shortDescription = $state('');
  let priceTon = $state('1.5');
  let discountPercentage = $state(0);
  let version = $state('1.0.0');
  let archiveFileName = $state<string | null>(null);
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
  let selectedGenres = $state<string[]>(['Indie', 'Action']);

  const availablePlatforms = ['Windows', 'macOS', 'Linux'];
  let selectedPlatforms = $state<string[]>(['Windows']);

  let tagsInput = $state('Web3, TON, Cyberpunk');

  function close() {
    uiStore.setPublishGameModal(false);
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
    const files = (e.target as HTMLInputElement).files;
    if (!files || files.length === 0) return;
    const file = files[0];
    isUploadingCover = true;
    try {
      const res = await mediaService.upload(file);
      coverImageUrl = res.url;
      if (!headerImageUrl) headerImageUrl = res.url;
      uiStore.addToast({
        title: 'Обкладинку завантажено',
        message: 'Зображення успішно прикріплено.',
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
    }
  }

  async function handleScreenshotUpload(e: Event) {
    const files = (e.target as HTMLInputElement).files;
    if (!files || files.length === 0) return;
    isUploadingScreenshot = true;
    try {
      for (let i = 0; i < files.length; i++) {
        const res = await mediaService.upload(files[i]);
        screenshotUrls = [...screenshotUrls, res.url];
      }
      uiStore.addToast({
        title: 'Скріншот додано',
        message: 'Зображення додано до галереї гри.',
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
    }
  }

  function removeScreenshot(index: number) {
    screenshotUrls = screenshotUrls.filter((_, i) => i !== index);
  }

  function handleArchiveChange(e: Event) {
    const files = (e.target as HTMLInputElement).files;
    if (files && files.length > 0) {
      archiveFileName = files[0].name;
      archivePath = `/storage/games/${files[0].name}`;
    }
  }

  async function handleSubmit() {
    if (!title.trim() || !description.trim()) {
      uiStore.addToast({
        title: 'Обов\'язкові поля',
        message: 'Будь ласка, введіть назву та опис гри.',
        type: 'warning',
      });
      return;
    }

    if (isPublished && !archivePath && !archiveFileName) {
      uiStore.addToast({
        title: 'Потрібен файл білду',
        message: 'Неможливо опублікувати гру без архіву з грою (.zip). Виберіть файл або вимкніть публікацію (збережіть як чернетку).',
        type: 'warning',
      });
      return;
    }

    const priceNum = parseFloat(priceTon) || 0;
    const priceInNanoTons = tonToNanoTon(priceNum);

    const tags = tagsInput
      .split(',')
      .map((t) => t.trim())
      .filter(Boolean);

    try {
      await developerStore.createGame({
        title: title.trim(),
        description: description.trim(),
        shortDescription: shortDescription.trim() || description.slice(0, 120),
        priceInNanoTons: Number(priceInNanoTons),
        discountPercentage: Math.max(0, Math.min(100, discountPercentage)),
        serverArchivePath: archivePath || '',
        genres: selectedGenres,
        platforms: selectedPlatforms,
        tags,
        version: version.trim() || '1.0.0',
        isPublished,
        coverImageUrl: coverImageUrl || 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&auto=format&fit=crop&q=80',
        headerImageUrl: headerImageUrl || coverImageUrl || 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&auto=format&fit=crop&q=80',
        screenshotUrls,
      });

      close();
    } catch {
      // Handled in store
    }
  }

  function fillDemoData() {
    title = 'White Punk';
    version = '1.0.0';
    shortDescription = 'Динамічний неоновий екшн-рогалик у засніженому мегаполісі майбутнього з битвами на виживання.';
    description = 'White Punk — це стильний ізометричний екшн-рогалик, події якого розгортаються на уламках засніженого мегаполіса майбутнього. Досліджуйте процедурно генеровані рівні, покращуйте кібернетичні імпланти, комбінуйте десятки видів плазмової та холодної зброї та боріться за енергетичні кристали TON проти синдикату автоматонів. Кожен забіг унікальний завдяки нелінійній прокачці та атмосферному саундтреку в стилі синтвейв.';
    priceTon = '2.5';
    discountPercentage = 15;
    tagsInput = 'Roguelike, Cyberpunk, Sci-Fi, Fast-Paced, Web3, TON';
    selectedGenres = ['Action', 'Cyberpunk', 'Indie'];
    selectedPlatforms = ['Windows', 'macOS'];
    coverImageUrl = 'https://images.unsplash.com/photo-1579373903781-fd5c0c30c4cd?w=800&auto=format&fit=crop&q=80';
    headerImageUrl = 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1200&auto=format&fit=crop&q=80';
    screenshotUrls = [
      'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80',
      'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80'
    ];
    archivePath = '/storage/games/white-punk-v1.zip';
    archiveFileName = 'white-punk-v1.zip';
  }
</script>

{#if $uiStore.isPublishGameModalOpen}
  <div class="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4">
    <div class="bg-[#051c27] border border-cyan-500/30 rounded-3xl w-full max-w-2xl max-h-[500px] shadow-2xl overflow-hidden flex flex-col animate-in fade-in zoom-in-95">
      <!-- Modal Header -->
      <div class="p-6 bg-[#072432] border-b border-cyan-500/20 flex items-center justify-between shrink-0">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-[#0df2c9] to-cyan-600 text-black flex items-center justify-center font-black shadow-lg shadow-cyan-500/20">
            <Gamepad2 class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-white font-display">Публікація нової гри</h3>
            <span class="text-xs text-slate-400">Розмістіть свій проект у каталозі Dteam та заробляйте TON</span>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <button
            type="button"
            onclick={fillDemoData}
            class="px-2.5 py-1.5 rounded-xl bg-cyan-500/20 hover:bg-cyan-500/30 text-cyan-300 border border-cyan-500/40 text-[11px] font-bold flex items-center gap-1.5 transition-all cursor-pointer hover:scale-105 active:scale-95"
            title="Заповнити всі поля тестовими даними"
          >
            <Sparkles class="w-3.5 h-3.5 text-[#0df2c9]" />
            <span>Демо-дані</span>
          </button>
          <button
            onclick={close}
            class="w-8 h-8 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-400 hover:text-white flex items-center justify-center transition-colors cursor-pointer"
          >
            <X class="w-4 h-4" />
          </button>
        </div>
      </div>

      <!-- Form Body -->
      <form onsubmit={(e) => { e.preventDefault(); handleSubmit(); }} class="p-6 space-y-5 text-xs overflow-y-auto grow">
        <!-- Title & Version -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="sm:col-span-2">
            <label for="newGameTitle" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Назва гри <span class="text-cyan-400">*</span>
            </label>
            <input
              id="newGameTitle"
              type="text"
              placeholder="наприклад: Cyber Realm 2099"
              bind:value={title}
              required
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>
          <div>
            <label for="newGameVersion" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Версія
            </label>
            <input
              id="newGameVersion"
              type="text"
              placeholder="1.0.0"
              bind:value={version}
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white font-mono text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>
        </div>

        <!-- Short Description -->
        <div>
          <label for="newGameShortDesc" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Короткий опис (для списків та карток)
          </label>
          <input
            id="newGameShortDesc"
            type="text"
            placeholder="Захоплива кіберпанк-пригода у відкритому світі..."
            bind:value={shortDescription}
            class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white text-xs focus:border-cyan-400 focus:outline-none transition-colors"
          />
        </div>

        <!-- Full Description -->
        <div>
          <label for="newGameDesc" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Повний опис гри <span class="text-cyan-400">*</span>
          </label>
          <textarea
            id="newGameDesc"
            rows="3"
            placeholder="Детальний опис ігроладу, механік, сюжету та особливостей..."
            bind:value={description}
            required
            class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white text-xs focus:border-cyan-400 focus:outline-none transition-colors"
          ></textarea>
        </div>

        <!-- Price & Discount -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label for="newGamePrice" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Ціна в TON (0 для безкоштовної)
            </label>
            <input
              id="newGamePrice"
              type="number"
              step="0.01"
              min="0"
              placeholder="1.5"
              bind:value={priceTon}
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white font-mono text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>

          <div>
            <label for="newGameDiscount" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Знижка (%)
            </label>
            <input
              id="newGameDiscount"
              type="number"
              min="0"
              max="100"
              placeholder="0"
              bind:value={discountPercentage}
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white font-mono text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>
        </div>

        <!-- Cover Image Upload -->
        <div>
          <span class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Обкладинка гри (Cover)
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
                <span>{coverImageUrl ? 'Змінити обкладинку' : 'Завантажити зображення (JPG, PNG, WEBP)'}</span>
              {/if}
              <input type="file" accept="image/*" onchange={handleCoverUpload} class="hidden" />
            </label>
          </div>
        </div>

        <!-- Screenshots Gallery -->
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
            <label for="newGameTags" class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
              Теги (через кому)
            </label>
            <input
              id="newGameTags"
              type="text"
              placeholder="Web3, RPG, Co-op"
              bind:value={tagsInput}
              class="w-full px-3.5 py-2.5 rounded-xl bg-[#072535] border border-cyan-500/20 text-white text-xs focus:border-cyan-400 focus:outline-none transition-colors"
            />
          </div>
        </div>

        <!-- Server Archive File -->
        <div>
          <span class="block font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Файл білду гри (.zip / .tar.gz)
          </span>
          <label class="flex flex-col items-center justify-center p-4 border-2 border-dashed border-cyan-500/30 hover:border-cyan-400 rounded-2xl bg-[#072535]/40 hover:bg-[#072535]/80 cursor-pointer transition-colors">
            <FileArchive class="w-8 h-8 text-cyan-400 mb-1" />
            <span class="text-xs text-slate-300 font-semibold">
              {archiveFileName || 'Натисніть для вибору архіву з грою'}
            </span>
            <span class="text-[10px] text-slate-500 mt-0.5">Підтримуються архіви .zip, .tar, .gz, .rar</span>
            <input type="file" accept=".zip,.tar,.gz,.rar" onchange={handleArchiveChange} class="hidden" />
          </label>
        </div>

        <!-- Publish toggle -->
        <div class="space-y-2 p-3.5 rounded-xl bg-[#072535] border border-cyan-500/20">
          <div class="flex items-center justify-between">
            <div>
              <span class="font-bold text-white block">Опублікувати одразу у каталозі</span>
              <span class="text-[11px] text-slate-400">Гра стане доступною для перегляду та купівлі користувачами</span>
            </div>
            <input
              type="checkbox"
              bind:checked={isPublished}
              class="w-5 h-5 accent-[#0df2c9] cursor-pointer"
            />
          </div>
          {#if !archiveFileName && !archivePath}
            <div class="flex items-center gap-2 pt-2 border-t border-cyan-950/80 text-[11px] text-amber-300">
              <AlertCircle class="w-3.5 h-3.5 shrink-0 text-amber-400" />
              <span>
                {isPublished
                  ? 'Увага: для публікації необхідно обрати файл архіву вище. Без архіву збережіть гру як чернетку.'
                  : 'Режим чернетки: гра буде збережена тільки у вашому кабінеті. Архів можна завантажити пізніше.'}
              </span>
            </div>
          {/if}
        </div>

        <!-- Submit Button -->
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
            class="px-6 py-3 rounded-xl bg-gradient-to-r from-[#0df2c9] via-cyan-500 to-blue-600 hover:from-[#25fed7] hover:to-blue-500 text-black font-extrabold text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center gap-2 disabled:opacity-50"
          >
            {#if $developerStore.isSaving}
              <Loader2 class="w-4 h-4 animate-spin" />
              <span>Збереження...</span>
            {:else}
              <Sparkles class="w-4 h-4" />
              <span>{isPublished ? 'Опублікувати гру' : 'Зберегти як чернетку'}</span>
            {/if}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}