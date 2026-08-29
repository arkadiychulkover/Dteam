<script lang="ts">
import { uiStore } from '../../stores/uiStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { tonToNanoTon } from '../../utils/formatters';
  import type { Game } from '../../types';
  import { X, Upload, PlusCircle, Gamepad2, FileArchive, Sparkles } from 'lucide-svelte';

  let title = $state('');
  let description = $state('');
  let priceTon = $state('2.5');
  let coverUrl = $state('https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&auto=format&fit=crop&q=80');
  let tags = $state('Action, Cyberpunk, Indie');
  let archiveFileName = $state<string | null>(null);

  function close() {
    uiStore.setTab('catalog');
  }

  function handleFileChange(e: Event) {
    const files = (e.target as HTMLInputElement).files;
    if (files && files.length > 0) {
      archiveFileName = files[0].name;
    }
  }

  function handlePublish() {
    if (!title || !description) {
      uiStore.addToast({
        title: 'Missing Fields',
        message: 'Please fill in game title and description.',
        type: 'warning',
      });
      return;
    }

    const newGame: Game = {
      id: Math.random().toString(36).substring(2, 9),
      title,
      description,
      shortDescription: description.slice(0, 100),
      priceInNanoTons: tonToNanoTon(parseFloat(priceTon) || 0).toString(),
      serverArchivePath: `/storage/games/${archiveFileName || 'game-build.zip'}`,
      ownerId: 'my-user-id',
      downloadCount: 0,
      averageRating: 5.0,
      reviewsCount: 1,
      isDlc: false,
      version: '1.0.0',
      sizeInBytes: 15_000_000_000,
      isPublished: true,
      coverImageUrl: coverUrl,
      headerImageUrl: coverUrl,
      screenshotUrls: [],
      tags: tags.split(',').map(t => t.trim()),
      createdAt: new Date().toISOString(),
    };

    gamesStore.subscribe(s => {
      s.games.unshift(newGame);
    })();

    uiStore.addToast({
      title: 'Game Published!',
      message: `${title} is now live in the Dteam store!`,
      type: 'success',
    });

    close();
  }
</script>

<div class="max-w-xl mx-auto px-4 py-8">
  <div class="p-6 md:p-8 bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl">
    <div class="flex items-center gap-3 mb-6">
      <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-500 to-blue-600 text-white flex items-center justify-center shadow-lg shadow-cyan-500/20">
        <Gamepad2 class="w-5 h-5" />
      </div>
      <div>
        <h3 class="text-xl font-bold text-white font-display">Publish New Web3 Game</h3>
        <span class="text-xs text-slate-400">Deploy your build and earn TON directly</span>
      </div>
    </div>

    <form onsubmit={(e) => { e.preventDefault(); handlePublish(); }} class="space-y-4 text-xs">
      <div>
        <label for="gameTitle" class="block font-bold text-slate-300 uppercase tracking-wider mb-1">Game Title</label>
        <input
          id="gameTitle"
          type="text"
          placeholder="e.g. Cyber Realm 2099"
          bind:value={title}
          required
          class="w-full px-3.5 py-2.5 rounded-xl bg-slate-900 border border-slate-700 text-white text-sm focus:border-cyan-500 focus:outline-none"
        />
      </div>

      <div>
        <label for="gameDescription" class="block font-bold text-slate-300 uppercase tracking-wider mb-1">Description</label>
        <textarea
          id="gameDescription"
          rows="3"
          placeholder="Describe your gameplay, story, features..."
          bind:value={description}
          required
          class="w-full px-3.5 py-2.5 rounded-xl bg-slate-900 border border-slate-700 text-white text-sm focus:border-cyan-500 focus:outline-none"
        ></textarea>
      </div>

      <div class="grid grid-cols-2 gap-4">
        <div>
          <label for="priceTonInput" class="block font-bold text-slate-300 uppercase tracking-wider mb-1">Price (TON)</label>
          <input
            id="priceTonInput"
            type="number"
            step="0.1"
            min="0"
            placeholder="0 for Free to Play"
            bind:value={priceTon}
            class="w-full px-3.5 py-2.5 rounded-xl bg-slate-900 border border-slate-700 text-white font-mono text-sm focus:border-cyan-500 focus:outline-none"
          />
        </div>
        <div>
          <label for="tagsInput" class="block font-bold text-slate-300 uppercase tracking-wider mb-1">Tags / Genres</label>
          <input
            id="tagsInput"
            type="text"
            placeholder="Action, RPG, Indie"
            bind:value={tags}
            class="w-full px-3.5 py-2.5 rounded-xl bg-slate-900 border border-slate-700 text-white text-sm focus:border-cyan-500 focus:outline-none"
          />
        </div>
      </div>

      <div>
        <span class="block font-bold text-slate-300 uppercase tracking-wider mb-1">Server Game Archive (.zip)</span>
        <label for="archiveFileInput" class="flex flex-col items-center justify-center p-4 border-2 border-dashed border-slate-700 hover:border-cyan-500 rounded-xl bg-slate-900/60 cursor-pointer transition-colors">
          <FileArchive class="w-8 h-8 text-cyan-400 mb-1" />
          <span class="text-xs text-slate-300 font-semibold">
            {archiveFileName || 'Click to select game archive (.zip / .tar.gz)'}
          </span>
          <span class="text-[10px] text-slate-500 mt-0.5">Supports builds up to 50 GB</span>
          <input id="archiveFileInput" type="file" accept=".zip,.tar,.gz,.rar" onchange={handleFileChange} class="hidden" />
        </label>
      </div>

      <button
        type="submit"
        class="w-full mt-4 py-3.5 rounded-xl bg-gradient-to-r from-cyan-500 via-blue-600 to-indigo-600 hover:from-cyan-400 hover:to-blue-500 text-white font-extrabold text-sm tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center justify-center gap-2"
      >
        <Sparkles class="w-4 h-4" />
        <span>Publish Game to Catalog</span>
      </button>
    </form>
  </div>
</div>

