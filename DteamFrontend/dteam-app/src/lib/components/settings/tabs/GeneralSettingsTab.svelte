<script lang="ts">
  import { settingsStore } from '../../../stores/settingsStore';
  import SettingsPageHeader from '../primitives/SettingsPageHeader.svelte';
  import SettingsActionBar from '../primitives/SettingsActionBar.svelte';
  import BackendImage from '../../ui/BackendImage.svelte';
  import { Camera, Loader2, Globe } from 'lucide-svelte';

  const { draftState, uiState } = settingsStore;

  let avatarFileInput: HTMLInputElement | undefined = $state();
  let bannerFileInput: HTMLInputElement | undefined = $state();

  function handleAvatarChange(e: Event) {
    const input = e.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      settingsStore.uploadAvatar(input.files[0]);
    }
  }

  function handleBannerChange(e: Event) {
    const input = e.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      settingsStore.uploadBanner(input.files[0]);
    }
  }

  const languages = [
    { code: 'uk', label: 'Українська', flag: '🇺🇦' },
    { code: 'en', label: 'English', flag: '🇬🇧' },
  ];
</script>

<div>
  <SettingsPageHeader
    title="Загальні налаштування"
    subtitle="Керуйте персональною інформацією, аватаром та налаштуваннями мови вашого профілю."
  />

  <!-- Profile Banner & Avatar Hero Header -->
  <div class="relative mb-8">
    <!-- Banner -->
    <div class="w-full h-48 sm:h-56 rounded-2xl overflow-hidden relative bg-gradient-to-r from-cyan-950 via-[#032b38] to-[#011a24] border border-cyan-900/30 shadow-inner group">
      {#if $draftState.bannerUrl}
        <BackendImage
          src={$draftState.bannerUrl}
          alt="Profile Banner"
          class="w-full h-full object-cover"
        />
      {:else}
        <div class="w-full h-full flex items-center justify-center text-cyan-500/30 text-xs font-mono">
          Немає встановленого банера
        </div>
      {/if}

      <!-- Banner Upload Button -->
      <button
        type="button"
        onclick={() => bannerFileInput?.click()}
        disabled={$uiState.isUploadingBanner}
        class="absolute bottom-3 right-3 p-2.5 rounded-xl bg-black/60 hover:bg-black/80 backdrop-blur-md border border-cyan-500/30 text-cyan-300 hover:text-white transition-all cursor-pointer shadow-lg group-hover:scale-105 flex items-center gap-2 text-xs font-bold"
        title="Змінити банер"
      >
        {#if $uiState.isUploadingBanner}
          <Loader2 class="w-4 h-4 animate-spin text-cyan-400" />
        {:else}
          <Camera class="w-4 h-4 text-cyan-400" />
        {/if}
        <span class="hidden sm:inline">Змінити банер</span>
      </button>
      <input
        bind:this={bannerFileInput}
        type="file"
        accept="image/*"
        class="hidden"
        onchange={handleBannerChange}
      />
    </div>

    <!-- Avatar overlapping banner -->
    <div class="absolute -bottom-6 left-6 flex items-end gap-4">
      <div class="relative w-24 h-24 sm:w-28 sm:h-28 rounded-full border-4 border-[#021822] bg-gradient-to-br from-cyan-400 via-teal-500 to-emerald-400 p-0.5 shadow-2xl overflow-hidden group">
        <div class="w-full h-full rounded-full overflow-hidden bg-[#061820] flex items-center justify-center">
          {#if $draftState.avatarUrl}
            <BackendImage
              src={$draftState.avatarUrl}
              alt={$draftState.username}
              class="w-full h-full object-cover"
            />
          {:else}
            <span class="text-3xl font-black text-white font-display">
              {$draftState.username ? $draftState.username.charAt(0).toUpperCase() : 'U'}
            </span>
          {/if}
        </div>

        <!-- Avatar Upload Button -->
        <button
          type="button"
          onclick={() => avatarFileInput?.click()}
          disabled={$uiState.isUploadingAvatar}
          class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center text-white cursor-pointer"
          title="Змінити аватар"
        >
          {#if $uiState.isUploadingAvatar}
            <Loader2 class="w-6 h-6 animate-spin text-cyan-400" />
          {:else}
            <Camera class="w-6 h-6 text-cyan-300" />
          {/if}
        </button>
        <input
          bind:this={avatarFileInput}
          type="file"
          accept="image/*"
          class="hidden"
          onchange={handleAvatarChange}
        />
      </div>
    </div>
  </div>

  <!-- Form Fields -->
  <div class="pt-8 space-y-5">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-5">
      <!-- Username -->
      <div>
        <label for="settings-username" class="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-2 font-mono">
          Нікнейм
        </label>
        <input
          id="settings-username"
          type="text"
          value={$draftState.username}
          oninput={(e) => settingsStore.updateGeneralDraft({ username: (e.target as HTMLInputElement).value })}
          placeholder="Введіть ваш нікнейм..."
          maxlength="50"
          class="w-full px-4 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all font-medium"
        />
      </div>

      <!-- Email -->
      <div>
        <label for="settings-email" class="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-2 font-mono">
          Ел. пошта
        </label>
        <input
          id="settings-email"
          type="email"
          value={$draftState.email}
          oninput={(e) => settingsStore.updateGeneralDraft({ email: (e.target as HTMLInputElement).value })}
          placeholder="example@mail.com"
          maxlength="256"
          class="w-full px-4 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all font-medium"
        />
      </div>
    </div>

    <!-- Bio with character counter -->
    <div>
      <div class="flex items-center justify-between mb-2">
        <label for="settings-bio" class="block text-xs font-bold uppercase tracking-wider text-slate-300 font-mono">
          Про себе
        </label>
        <span class="text-[11px] font-mono font-bold {$draftState.bio.length >= 95 ? 'text-amber-400' : 'text-slate-400'}">
          {$draftState.bio.length}/100
        </span>
      </div>
      <textarea
        id="settings-bio"
        rows="3"
        value={$draftState.bio}
        oninput={(e) => settingsStore.updateGeneralDraft({ bio: (e.target as HTMLTextAreaElement).value })}
        placeholder="Розкажіть трохи про себе та ваші улюблені ігри..."
        maxlength="100"
        class="w-full px-4 py-3 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all resize-none font-medium leading-relaxed"
      ></textarea>
    </div>

    <!-- Language -->
    <div>
      <label for="settings-lang" class="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-2 font-mono flex items-center gap-1.5">
        <Globe class="w-3.5 h-3.5 text-cyan-400" />
        <span>Мова сайту</span>
      </label>
      <div class="relative w-full sm:w-64">
        <select
          id="settings-lang"
          value={$draftState.preferredLanguage}
          onchange={(e) => settingsStore.updateGeneralDraft({ preferredLanguage: (e.target as HTMLSelectElement).value })}
          class="w-full px-4 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 text-sm focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all cursor-pointer font-bold"
        >
          {#each languages as lang}
            <option value={lang.code} class="bg-[#031d27] text-white">
              {lang.flag} {lang.label}
            </option>
          {/each}
        </select>
      </div>
    </div>
  </div>

  <!-- Action Bar -->
  <SettingsActionBar
    isSaving={$uiState.isSaving}
    isDirty={$draftState.isDirty}
    onsave={settingsStore.saveGeneralSettings}
    oncancel={settingsStore.resetGeneralDraft}
  />
</div>
