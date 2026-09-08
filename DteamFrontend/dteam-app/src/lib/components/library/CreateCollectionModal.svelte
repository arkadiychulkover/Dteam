<script lang="ts">
  import { X, FolderPlus, Trash2, Check } from 'lucide-svelte';
  import { libraryStore } from '../../stores/libraryStore';
  import type { GameCollection, UserGame } from '../../types';

  let {
    collection = null,
    onClose,
  }: {
    collection?: GameCollection | null;
    onClose: () => void;
  } = $props();

  let name = $state('');
  let description = $state('');
  let selectedGameIds = $state<string[]>([]);
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  $effect(() => {
    name = collection?.name || '';
    description = collection?.description || '';
    selectedGameIds = collection?.items?.map((i) => i.gameId) || [];
  });

  function toggleGame(gameId: string) {
    if (selectedGameIds.includes(gameId)) {
      selectedGameIds = selectedGameIds.filter((id) => id !== gameId);
    } else {
      selectedGameIds = [...selectedGameIds, gameId];
    }
  }

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    if (!name.trim()) {
      errorMessage = 'Введіть назву колекції';
      return;
    }

    isSubmitting = true;
    errorMessage = '';

    try {
      if (collection) {
        // Update collection details
        await libraryStore.updateCollection(collection.id, name.trim(), description.trim() || undefined);
        // Sync items
        const colItems = collection.items ?? [];
        const currentIds = new Set(colItems.map((i) => i.gameId));
        const newIds = new Set(selectedGameIds);

        // Add missing
        for (const gId of selectedGameIds) {
          if (!currentIds.has(gId)) {
            await libraryStore.toggleGameInCollection(collection.id, gId);
          }
        }
        // Remove unselected
        for (const cItem of colItems) {
          if (!newIds.has(cItem.gameId)) {
            await libraryStore.toggleGameInCollection(collection.id, cItem.gameId);
          }
        }
      } else {
        await libraryStore.createCollection(
          name.trim(),
          description.trim() || undefined,
          selectedGameIds
        );
      }
      onClose();
    } catch (err: any) {
      errorMessage = err?.message || 'Не вдалося зберегти колекцію.';
    } finally {
      isSubmitting = false;
    }
  }

  async function handleDelete() {
    if (!collection) return;
    if (!confirm(`Ви дійсно бажаєте видалити колекцію "${collection.name}"?`)) return;

    isSubmitting = true;
    try {
      await libraryStore.deleteCollection(collection.id);
      onClose();
    } catch (err: any) {
      errorMessage = err?.message || 'Не вдалося видалити колекцію.';
    } finally {
      isSubmitting = false;
    }
  }
</script>

<div
  class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in"
  role="dialog"
  aria-modal="true"
  tabindex="-1"
  onclick={(e) => { if (e.target === e.currentTarget) onClose(); }}
  onkeydown={(e) => { if (e.key === 'Escape') onClose(); }}
>
  <div class="relative w-full max-w-lg bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl p-6 text-white space-y-5 overflow-hidden">
    <!-- Close button -->
    <button
      type="button"
      onclick={onClose}
      class="absolute top-4 right-4 p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
      aria-label="Закрити"
    >
      <X class="w-5 h-5" />
    </button>

    <!-- Header -->
    <div class="flex items-center gap-3">
      <div class="w-10 h-10 rounded-xl bg-cyan-500/20 border border-cyan-400/30 flex items-center justify-center text-cyan-300">
        <FolderPlus class="w-5 h-5" />
      </div>
      <div>
        <h3 class="text-base font-extrabold text-white">
          {collection ? 'Редагувати колекцію' : 'Створити нову колекцію'}
        </h3>
        <p class="text-xs text-slate-400">Організуйте ваші ігри за жанрами або вподобаннями</p>
      </div>
    </div>

    {#if errorMessage}
      <div class="p-3 bg-red-500/10 border border-red-500/30 rounded-xl text-xs text-red-400">
        {errorMessage}
      </div>
    {/if}

    <form onsubmit={handleSubmit} class="space-y-4">
      <div>
        <label for="collection-name" class="block text-xs font-bold text-slate-300 mb-1">
          Назва колекції <span class="text-cyan-400">*</span>
        </label>
        <input
          id="collection-name"
          type="text"
          bind:value={name}
          maxlength="60"
          placeholder="Наприклад: Пройдені, Кооператив, RPG..."
          class="w-full px-4 py-2.5 rounded-xl bg-[#030d12] border border-cyan-500/20 focus:border-cyan-400 text-sm text-white placeholder:text-slate-600 outline-none transition-colors"
          required
        />
      </div>

      <div>
        <label for="collection-desc" class="block text-xs font-bold text-slate-300 mb-1">
          Опис (необов'язково)
        </label>
        <textarea
          id="collection-desc"
          bind:value={description}
          rows="2"
          placeholder="Короткий опис колекції..."
          class="w-full px-4 py-2 rounded-xl bg-[#030d12] border border-cyan-500/20 focus:border-cyan-400 text-xs text-white placeholder:text-slate-600 outline-none transition-colors resize-none"
        ></textarea>
      </div>

      <!-- Games selector -->
      <div>
        <span class="block text-xs font-bold text-slate-300 mb-2">
          Додати ігри з бібліотеки ({selectedGameIds.length})
        </span>
        <div class="max-h-48 overflow-y-auto space-y-1.5 pr-1 rounded-xl bg-[#030d12] border border-cyan-500/15 p-2">
          {#if $libraryStore.items.length === 0}
            <p class="text-xs text-slate-500 text-center py-4">У вашій бібліотеці немає ігор.</p>
          {:else}
            {#each $libraryStore.items as item (item.gameId)}
              {#if item.game}
                {@const isSelected = selectedGameIds.includes(item.gameId)}
                <button
                  type="button"
                  onclick={() => toggleGame(item.gameId)}
                  class="w-full flex items-center justify-between p-2 rounded-lg text-left transition-colors cursor-pointer {isSelected ? 'bg-cyan-950/60 border border-cyan-500/40 text-cyan-200' : 'hover:bg-white/5 text-slate-300 border border-transparent'}"
                >
                  <div class="flex items-center gap-2.5 min-w-0">
                    <img
                      src={item.game.coverImageUrl || item.game.headerImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=100'}
                      alt={item.game.title}
                      class="w-7 h-7 rounded object-cover shrink-0"
                    />
                    <span class="text-xs font-medium truncate">{item.game.title}</span>
                  </div>
                  <div class="w-5 h-5 rounded border flex items-center justify-center shrink-0 {isSelected ? 'bg-cyan-500 border-cyan-400 text-black' : 'border-slate-700'}">
                    {#if isSelected}
                      <Check class="w-3.5 h-3.5" />
                    {/if}
                  </div>
                </button>
              {/if}
            {/each}
          {/if}
        </div>
      </div>

      <!-- Buttons -->
      <div class="flex items-center justify-between pt-2">
        {#if collection}
          <button
            type="button"
            onclick={handleDelete}
            disabled={isSubmitting}
            class="flex items-center gap-1.5 px-3 py-2 rounded-xl text-red-400 hover:text-red-300 hover:bg-red-500/10 text-xs font-bold transition-colors cursor-pointer"
          >
            <Trash2 class="w-3.5 h-3.5" />
            Видалити
          </button>
        {:else}
          <div></div>
        {/if}

        <div class="flex items-center gap-2.5">
          <button
            type="button"
            onclick={onClose}
            class="px-4 py-2 rounded-xl bg-white/5 hover:bg-white/10 text-xs font-bold text-slate-300 hover:text-white transition-colors cursor-pointer"
          >
            Скасувати
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            class="px-5 py-2 rounded-xl bg-cyan-500 hover:bg-cyan-400 text-black text-xs font-extrabold transition-all cursor-pointer shadow-lg shadow-cyan-500/20 disabled:opacity-50"
          >
            {isSubmitting ? 'Збереження...' : collection ? 'Зберегти зміни' : 'Створити'}
          </button>
        </div>
      </div>
    </form>
  </div>
</div>
