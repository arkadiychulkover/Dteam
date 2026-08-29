<script lang="ts">
import { adminService, type CreateUserDto } from '../../services/adminService';
  import { uiStore } from '../../stores/uiStore';
  import { MOCK_AVATARS } from '../../utils/constants';
  import { X, UserPlus, Shield, Wallet, Mail, Lock, User, Sparkles } from 'lucide-svelte';

  interface Props {
    isOpen: boolean;
    onClose: () => void;
    onUserCreated: () => void;
  }

  let { isOpen, onClose, onUserCreated }: Props = $props();

  let email = $state('');
  let username = $state('');
  let password = $state('admin123321');
  let walletAddress = $state('');
  let isAdmin = $state(false);
  let balanceInTon = $state('10');
  let status = $state(1);
  let avatarUrl = $state(MOCK_AVATARS[0]);
  let bio = $state('New Dteam community member & gamer');
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  async function handleSubmit(e: Event) {
    e.preventDefault();
    if (!email || !username || !password) {
      errorMessage = 'Email, username, and password are required.';
      return;
    }

    isSubmitting = true;
    errorMessage = '';

    try {
      const balanceInNanoTons = Math.floor((parseFloat(balanceInTon) || 0) * 1_000_000_000);
      const payload: CreateUserDto = {
        email: email.trim(),
        username: username.trim(),
        password: password.trim(),
        walletAddress: walletAddress.trim() || undefined,
        isAdmin,
        balanceInNanoTons,
        status,
        avatarUrl: avatarUrl.trim() || undefined,
        bio: bio.trim() || undefined,
      };

      await adminService.createUser(payload);

      uiStore.addToast({
        title: 'User Created',
        message: `User '${username}' was successfully added!`,
        type: 'success',
      });

      onUserCreated();
      onClose();
    } catch (err: any) {
      errorMessage = err.message || 'Failed to create user.';
    } finally {
      isSubmitting = false;
    }
  }
</script>

{#if isOpen}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in">
    <div class="relative w-full max-w-2xl bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl shadow-cyan-950/50 p-6 overflow-hidden max-h-[90vh] flex flex-col">
      <div class="absolute -top-24 -right-24 w-60 h-60 bg-cyan-500/10 rounded-full blur-3xl pointer-events-none"></div>

      <div class="flex items-center justify-between pb-4 border-b border-cyan-950">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-500/20 to-emerald-500/20 border border-cyan-500/40 flex items-center justify-center text-cyan-400">
            <UserPlus class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-white tracking-wide flex items-center gap-2">
              Create New User
              <span class="text-xs px-2 py-0.5 rounded-full bg-cyan-950 text-cyan-400 border border-cyan-500/30">
                Admin Mode
              </span>
            </h3>
            <p class="text-xs text-slate-400">Add a gamer or administrator account to Dteam database</p>
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

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label for="create-email" class="block text-xs font-semibold text-slate-300 mb-1">Email Address *</label>
            <div class="relative">
              <Mail class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="create-email"
                type="email"
                required
                placeholder="user@dteam.io"
                bind:value={email}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-500"
              />
            </div>
          </div>

          <div>
            <label for="create-username" class="block text-xs font-semibold text-slate-300 mb-1">Username *</label>
            <div class="relative">
              <User class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="create-username"
                type="text"
                required
                placeholder="NeoPlayer"
                bind:value={username}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-500"
              />
            </div>
          </div>

          <div>
            <label for="create-password" class="block text-xs font-semibold text-slate-300 mb-1">Password *</label>
            <div class="relative">
              <Lock class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="create-password"
                type="text"
                required
                placeholder="Min. 6 chars"
                bind:value={password}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-500 font-mono"
              />
            </div>
          </div>

          <div>
            <label for="create-balance" class="block text-xs font-semibold text-slate-300 mb-1">Initial Balance (TON)</label>
            <div class="relative">
              <Wallet class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-cyan-400" />
              <input
                id="create-balance"
                type="number"
                step="0.1"
                min="0"
                placeholder="10.0"
                bind:value={balanceInTon}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-cyan-300 font-mono"
              />
            </div>
          </div>
        </div>

        <div>
          <label for="create-wallet" class="block text-xs font-semibold text-slate-300 mb-1">TON Wallet Address (Optional)</label>
          <input
            id="create-wallet"
            type="text"
            placeholder="EQBvW8Z5huBkMJYdn3PBRnVDLyTO2_OTHTuP4asMb_Fton"
            bind:value={walletAddress}
            class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 font-mono placeholder-slate-500"
          />
        </div>

        <div>
          <label for="create-avatar" class="block text-xs font-semibold text-slate-300 mb-1">Avatar Image URL</label>
          <div class="flex items-center gap-3">
            <img src={avatarUrl || MOCK_AVATARS[0]} alt="Avatar preview" class="w-10 h-10 rounded-xl object-cover border border-cyan-500/40" />
            <input
              id="create-avatar"
              type="text"
              placeholder="https://images.unsplash.com/..."
              bind:value={avatarUrl}
              class="flex-1 px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 placeholder-slate-500"
            />
          </div>
          <div class="flex items-center gap-2 mt-2">
            <span class="text-[11px] text-slate-500">Presets:</span>
            {#each MOCK_AVATARS as preset}
              <button
                type="button"
                onclick={() => avatarUrl = preset}
                class="w-6 h-6 rounded-lg overflow-hidden border border-cyan-500/30 hover:border-cyan-400 cursor-pointer"
              >
                <img src={preset} alt="preset" class="w-full h-full object-cover" />
              </button>
            {/each}
          </div>
        </div>

        <div class="p-3.5 rounded-xl bg-[#081e26] border border-cyan-500/20 flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-cyan-500/10 flex items-center justify-center text-cyan-400">
              <Shield class="w-4 h-4" />
            </div>
            <div>
              <div class="text-xs font-bold text-white">Administrator Privileges (IsAdmin)</div>
              <div class="text-[11px] text-slate-400">Grants full access to admin operations & database management</div>
            </div>
          </div>
          <label class="relative inline-flex items-center cursor-pointer">
            <input type="checkbox" bind:checked={isAdmin} class="sr-only peer" />
            <div class="w-11 h-6 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-gradient-to-r peer-checked:from-cyan-500 peer-checked:to-emerald-500"></div>
          </label>
        </div>

        <div>
          <label for="create-bio" class="block text-xs font-semibold text-slate-300 mb-1">User Bio</label>
          <textarea
            id="create-bio"
            rows="2"
            bind:value={bio}
            placeholder="Short description..."
            class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 placeholder-slate-500 resize-none"
          ></textarea>
        </div>

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-cyan-950">
          <button
            type="button"
            onclick={onClose}
            class="px-4 py-2 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-xs font-semibold text-slate-300 transition-colors cursor-pointer"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            class="flex items-center gap-2 px-5 py-2 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-bold text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer disabled:opacity-50"
          >
            {#if isSubmitting}
              <span class="animate-spin">⟳</span>
              <span>Creating...</span>
            {:else}
              <Sparkles class="w-4 h-4" />
              <span>Create User</span>
            {/if}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}

