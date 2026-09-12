<script lang="ts">
  import { settingsStore } from '../../../stores/settingsStore';
  import { currentUser } from '../../../stores/authStore';
  import SettingsPageHeader from '../primitives/SettingsPageHeader.svelte';
  import { AlertTriangle, Loader2 } from 'lucide-svelte';

  const { uiState } = settingsStore;

  let usernameConfirmation = $state('');
  let password = $state('');
  let confirmPassword = $state('');
  let showConfirmModal = $state(false);

  const canDelete = $derived(
    usernameConfirmation.trim().length > 0 &&
    password.length > 0 &&
    confirmPassword.length > 0 &&
    password === confirmPassword &&
    $currentUser &&
    usernameConfirmation.trim().toLowerCase() === $currentUser.username.toLowerCase()
  );

  async function executeDelete() {
    if (!canDelete || $uiState.isSaving) return;
    showConfirmModal = false;

    await settingsStore.deleteAccount({
      usernameConfirmation: usernameConfirmation.trim(),
      password,
      confirmPassword,
    });
  }
</script>

<div class="w-full">
  <SettingsPageHeader
    title="Видалення акаунта"
    subtitle="Ця дія є остаточною та незворотною. Будь ласка, уважно ознайомтеся з наслідками."
  />

  <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">

    <form onsubmit={(e) => { e.preventDefault(); if (canDelete) showConfirmModal = true; }} class="lg:col-span-7 space-y-4">

      <div>
        <label for="del-username" class="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-2 font-mono">
          Нікнейм
        </label>
        <input
          id="del-username"
          type="text"
          bind:value={usernameConfirmation}
          placeholder="Введіть ваш поточний нікнейм..."
          required
          class="w-full px-4 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-rose-400 focus:ring-1 focus:ring-rose-400 transition-all font-medium"
        />
        {#if $currentUser}
          <span class="block text-[11px] text-slate-400 mt-1 font-mono">
            Введіть точно: <span class="text-white font-bold">{$currentUser.username}</span>
          </span>
        {/if}
      </div>

      <div>
        <label for="del-password" class="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-2 font-mono">
          Пароль
        </label>
        <input
          id="del-password"
          type="password"
          bind:value={password}
          placeholder="Введіть ваш поточний пароль..."
          required
          class="w-full px-4 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-rose-400 focus:ring-1 focus:ring-rose-400 transition-all font-medium"
        />
      </div>

      <div>
        <label for="del-confirm-password" class="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-2 font-mono">
          Підтвердження пароля
        </label>
        <input
          id="del-confirm-password"
          type="password"
          bind:value={confirmPassword}
          placeholder="Повторіть пароль..."
          required
          class="w-full px-4 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-rose-400 focus:ring-1 focus:ring-rose-400 transition-all font-medium"
        />
      </div>

      <div class="pt-4 flex justify-start">
        <button
          type="submit"
          disabled={!canDelete || $uiState.isSaving}
          class="w-full sm:w-auto px-10 py-2.5 rounded-full text-xs font-black bg-[#ff5b79] hover:bg-[#e11d48] text-white hover:scale-105 active:scale-95 shadow-lg shadow-rose-900/30 transition-all cursor-pointer disabled:opacity-40 disabled:hover:scale-100 disabled:cursor-not-allowed flex items-center justify-center gap-2"
        >
          {#if $uiState.isSaving}
            <Loader2 class="w-3.5 h-3.5 animate-spin" />
          {/if}
          <span>Видалити мій акаунт</span>
        </button>
      </div>
    </form>

    <div class="lg:col-span-5 space-y-4">
      <div class="p-5 rounded-2xl bg-[#1f0a10] border border-rose-500/30 text-xs text-rose-200 leading-relaxed space-y-3 shadow-lg">
        <div class="flex items-center gap-2 text-rose-400 font-bold">
          <AlertTriangle class="w-5 h-5 shrink-0" />
          <span class="uppercase tracking-wider font-mono">Увага! Незворотна дія</span>
        </div>
        <p class="text-slate-300">
          Натиснувши кнопку підтвердження, ви назавжди видалите акаунт. Після видалення буде неможливо відновити:
        </p>
        <ul class="space-y-2 text-rose-300/90 text-xs font-medium">
          <li class="flex items-center gap-2">
            <span class="text-rose-400">•</span>
            <span>Бібліотеку придбаних ігор та досягнення</span>
          </li>
          <li class="flex items-center gap-2">
            <span class="text-rose-400">•</span>
            <span>Залишок балансу TON у вашому гаманці</span>
          </li>
          <li class="flex items-center gap-2">
            <span class="text-rose-400">•</span>
            <span>Список друзів, листування та коментарі</span>
          </li>
          <li class="flex items-center gap-2">
            <span class="text-rose-400">•</span>
            <span>Анулювання всіх активних сесій</span>
          </li>
        </ul>
      </div>
    </div>
  </div>

  {#if showConfirmModal}
    <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
      <div class="w-full max-w-md bg-[#09151e] border border-rose-500/40 rounded-3xl p-6 shadow-2xl space-y-4">
        <div class="flex items-center gap-3 text-rose-400 font-bold text-lg">
          <AlertTriangle class="w-6 h-6" />
          <span>Підтвердження видалення</span>
        </div>
        <p class="text-xs text-slate-300 leading-relaxed font-medium">
          Ви впевнені, що хочете назавжди видалити свій акаунт? Відновити доступ до куплених ігор, друзів та балансу буде неможливо.
        </p>
        <div class="flex items-center justify-end gap-3 pt-2">
          <button
            type="button"
            onclick={() => showConfirmModal = false}
            class="px-5 py-2 rounded-full text-xs font-bold text-slate-300 hover:text-white hover:bg-slate-800 transition-colors cursor-pointer"
          >
            Скасувати
          </button>
          <button
            type="button"
            onclick={executeDelete}
            class="px-6 py-2 rounded-full text-xs font-black bg-rose-600 hover:bg-rose-500 text-white shadow-lg shadow-rose-900/40 transition-all cursor-pointer"
          >
            Так, видалити
          </button>
        </div>
      </div>
    </div>
  {/if}
</div>
