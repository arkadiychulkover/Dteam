<script lang="ts">
  import { settingsStore } from '../../../stores/settingsStore';
  import SettingsPageHeader from '../primitives/SettingsPageHeader.svelte';
  import { Lock, Eye, EyeOff, Check, X, Shield, ShieldCheck, Loader2 } from 'lucide-svelte';

  const { uiState } = settingsStore;

  let oldPassword = $state('');
  let newPassword = $state('');
  let confirmNewPassword = $state('');

  let showOld = $state(false);
  let showNew = $state(false);
  let showConfirm = $state(false);

  // Dynamic checklist
  const hasMinLength = $derived(newPassword.length >= 7);
  const hasLetter = $derived(/[a-zA-Zа-яА-ЯіїєґІЇЄҐ]/.test(newPassword));
  const hasDigit = $derived(/[0-9]/.test(newPassword));
  const hasNoSpaces = $derived(newPassword.length > 0 && !/\s/.test(newPassword));
  const passwordsMatch = $derived(newPassword.length > 0 && newPassword === confirmNewPassword);
  const notSameAsOld = $derived(newPassword.length === 0 || oldPassword.length === 0 || newPassword !== oldPassword);

  const isValid = $derived(
    oldPassword.length > 0 &&
    hasMinLength &&
    hasLetter &&
    hasDigit &&
    hasNoSpaces &&
    passwordsMatch &&
    notSameAsOld
  );

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    if (!isValid || $uiState.isSaving) return;

    const success = await settingsStore.changePassword({
      oldPassword: oldPassword.trim(),
      newPassword: newPassword.trim(),
      confirmNewPassword: confirmNewPassword.trim(),
    });

    if (success) {
      oldPassword = '';
      newPassword = '';
      confirmNewPassword = '';
    }
  }
</script>

<div class="w-full">
  <SettingsPageHeader
    title="Зміна паролю"
    subtitle="Використовуйте надійний пароль для захисту вашого акаунту та збереження коштів."
  />

  <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
    <!-- Left Column: Form Inputs (7 cols) -->
    <form onsubmit={handleSubmit} class="lg:col-span-7 space-y-4">
      <!-- Old Password -->
      <div>
        <label for="old-password" class="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-2 font-mono">
          Старий пароль
        </label>
        <div class="relative">
          <input
            id="old-password"
            type={showOld ? 'text' : 'password'}
            bind:value={oldPassword}
            placeholder="Введіть ваш поточний пароль..."
            required
            class="w-full pl-4 pr-11 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all font-medium"
          />
          <button
            type="button"
            onclick={() => showOld = !showOld}
            class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white transition-colors cursor-pointer"
          >
            {#if showOld}<EyeOff class="w-4 h-4" />{:else}<Eye class="w-4 h-4" />{/if}
          </button>
        </div>
      </div>

      <!-- New Password -->
      <div>
        <label for="new-password" class="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-2 font-mono">
          Новий пароль
        </label>
        <div class="relative">
          <input
            id="new-password"
            type={showNew ? 'text' : 'password'}
            bind:value={newPassword}
            placeholder="Введіть новий пароль..."
            required
            class="w-full pl-4 pr-11 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all font-medium"
          />
          <button
            type="button"
            onclick={() => showNew = !showNew}
            class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white transition-colors cursor-pointer"
          >
            {#if showNew}<EyeOff class="w-4 h-4" />{:else}<Eye class="w-4 h-4" />{/if}
          </button>
        </div>
      </div>

      <!-- Confirm New Password -->
      <div>
        <label for="confirm-password" class="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-2 font-mono">
          Підтвердження нового пароля
        </label>
        <div class="relative">
          <input
            id="confirm-password"
            type={showConfirm ? 'text' : 'password'}
            bind:value={confirmNewPassword}
            placeholder="Повторіть новий пароль..."
            required
            class="w-full pl-4 pr-11 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all font-medium"
          />
          <button
            type="button"
            onclick={() => showConfirm = !showConfirm}
            class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white transition-colors cursor-pointer"
          >
            {#if showConfirm}<EyeOff class="w-4 h-4" />{:else}<Eye class="w-4 h-4" />{/if}
          </button>
        </div>
      </div>

      <!-- Save Button -->
      <div class="pt-4 flex justify-start">
        <button
          type="submit"
          disabled={!isValid || $uiState.isSaving}
          class="w-full sm:w-auto px-10 py-2.5 rounded-full text-xs font-black bg-[#0df2c9] text-black hover:bg-[#00e5b0] hover:scale-105 active:scale-95 shadow-lg shadow-cyan-500/20 transition-all cursor-pointer disabled:opacity-50 disabled:hover:scale-100 disabled:cursor-not-allowed flex items-center justify-center gap-2"
        >
          {#if $uiState.isSaving}
            <Loader2 class="w-3.5 h-3.5 animate-spin" />
          {/if}
          <span>Зберегти зміни</span>
        </button>
      </div>
    </form>

    <!-- Right Column: Live checklist & Advice (5 cols) -->
    <div class="lg:col-span-5 space-y-4">
      <!-- Security rules info box -->
      <div class="p-5 rounded-2xl bg-[#011c27] border border-cyan-800/40 text-xs text-slate-300 space-y-2 font-medium leading-relaxed shadow-lg">
        <div class="flex items-center justify-between mb-2">
          <span class="text-[11px] font-bold uppercase tracking-wider text-cyan-400 font-mono">
            Вимоги безпеки:
          </span>
          {#if isValid}
            <span class="inline-flex items-center gap-1 text-[11px] font-mono text-emerald-400 font-bold">
              <ShieldCheck class="w-3.5 h-3.5" /> Надійний
            </span>
          {/if}
        </div>
        <div class="flex items-center gap-2 {newPassword.length > 0 ? (hasMinLength ? 'text-emerald-400 font-bold' : 'text-rose-400') : 'text-slate-400'}">
          <span>•</span>
          <span>Використовуйте 7+ символів</span>
          {#if newPassword.length > 0}
            {#if hasMinLength}<Check class="w-3.5 h-3.5 ml-auto text-emerald-400" />{:else}<X class="w-3.5 h-3.5 ml-auto text-rose-400" />{/if}
          {/if}
        </div>
        <div class="flex items-center gap-2 {newPassword.length > 0 ? (hasLetter ? 'text-emerald-400 font-bold' : 'text-rose-400') : 'text-slate-400'}">
          <span>•</span>
          <span>Використовуйте принаймні 1 літеру</span>
          {#if newPassword.length > 0}
            {#if hasLetter}<Check class="w-3.5 h-3.5 ml-auto text-emerald-400" />{:else}<X class="w-3.5 h-3.5 ml-auto text-rose-400" />{/if}
          {/if}
        </div>
        <div class="flex items-center gap-2 {newPassword.length > 0 ? (hasDigit ? 'text-emerald-400 font-bold' : 'text-rose-400') : 'text-slate-400'}">
          <span>•</span>
          <span>Використовуйте принаймні 1 цифру</span>
          {#if newPassword.length > 0}
            {#if hasDigit}<Check class="w-3.5 h-3.5 ml-auto text-emerald-400" />{:else}<X class="w-3.5 h-3.5 ml-auto text-rose-400" />{/if}
          {/if}
        </div>
        <div class="flex items-center gap-2 {newPassword.length > 0 ? (hasNoSpaces ? 'text-emerald-400 font-bold' : 'text-rose-400') : 'text-slate-400'}">
          <span>•</span>
          <span>Без пробілів</span>
          {#if newPassword.length > 0}
            {#if hasNoSpaces}<Check class="w-3.5 h-3.5 ml-auto text-emerald-400" />{:else}<X class="w-3.5 h-3.5 ml-auto text-rose-400" />{/if}
          {/if}
        </div>
        <div class="flex items-center gap-2 {confirmNewPassword.length > 0 ? (passwordsMatch ? 'text-emerald-400 font-bold' : 'text-rose-400') : 'text-slate-400'}">
          <span>•</span>
          <span>Паролі співпадають</span>
          {#if confirmNewPassword.length > 0}
            {#if passwordsMatch}<Check class="w-3.5 h-3.5 ml-auto text-emerald-400" />{:else}<X class="w-3.5 h-3.5 ml-auto text-rose-400" />{/if}
          {/if}
        </div>
      </div>

      <!-- Security advice info -->
      <div class="p-4 rounded-2xl bg-[#01141c]/80 border border-cyan-900/30 text-xs text-slate-400 space-y-2 leading-relaxed">
        <div class="flex items-center gap-2 text-cyan-400 font-bold text-xs">
          <Shield class="w-4 h-4 shrink-0" />
          <span>Захист вашого акаунту</span>
        </div>
        <p class="text-[11px] text-slate-400">
          Після успішної зміни пароля всі ваші активні сесії на інших пристроях будуть автоматично завершені задля безпеки.
        </p>
      </div>
    </div>
  </div>
</div>
