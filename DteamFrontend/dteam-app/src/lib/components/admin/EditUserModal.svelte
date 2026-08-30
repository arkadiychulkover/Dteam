<script lang="ts">
  import { adminService, type UpdateUserDto } from '../../services/adminService';
  import { uiStore } from '../../stores/uiStore';
  import type { Duser } from '../../types';
  import { X, Edit, Shield, Wallet, Mail, Lock, User, Ban, CheckCircle2 } from 'lucide-svelte';

  interface Props {
    user: Duser | null;
    isOpen: boolean;
    onClose: () => void;
    onUserUpdated: () => void;
  }

  let { user, isOpen, onClose, onUserUpdated }: Props = $props();

  let email = $state('');
  let username = $state('');
  let password = $state('');
  let walletAddress = $state('');
  let isAdmin = $state(false);
  let isBanned = $state(false);
  let balanceInTon = $state('0');
  let status = $state(1);
  let avatarUrl = $state('');
  let bio = $state('');
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  $effect(() => {
    if (user) {
      email = user.email || '';
      username = user.username || '';
      password = '';
      walletAddress = user.walletAddress || '';
      isAdmin = !!user.isAdmin;
      isBanned = !!user.isBanned;
      balanceInTon = ((Number(user.balanceInNanoTons) || 0) / 1_000_000_000).toFixed(2);
      status = user.status ?? 1;
      avatarUrl = user.avatarUrl || '';
      bio = user.bio || '';
    }
  });

  async function handleSubmit(e: Event) {
    e.preventDefault();
    if (!user) return;

    isSubmitting = true;
    errorMessage = '';

    try {
      const balanceInNanoTons = Math.floor((parseFloat(balanceInTon) || 0) * 1_000_000_000);
      const payload: UpdateUserDto = {
        email: email.trim() || undefined,
        username: username.trim() || undefined,
        password: password.trim() ? password.trim() : undefined,
        walletAddress: walletAddress.trim() || undefined,
        isAdmin,
        isBanned,
        status,
        balanceInNanoTons,
        avatarUrl: avatarUrl.trim() || undefined,
        bio: bio.trim() || undefined,
      };

      await adminService.updateUser(user.id, payload);

      uiStore.addToast({
        title: 'User Updated',
        message: `User '${username}' was successfully updated!`,
        type: 'success',
      });

      onUserUpdated();
      onClose();
    } catch (err: any) {
      errorMessage = err.message || 'Failed to update user.';
    } finally {
      isSubmitting = false;
    }
  }
</script>

{#if isOpen && user}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in">
    <div class="relative w-full max-w-2xl bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl shadow-cyan-950/50 p-6 overflow-hidden max-h-[90vh] flex flex-col">
      <div class="flex items-center justify-between pb-4 border-b border-cyan-950">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-500/20 to-blue-500/20 border border-cyan-500/40 flex items-center justify-center text-cyan-400">
            <Edit class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-white tracking-wide flex items-center gap-2">
              Edit User Profile
              <span class="text-xs px-2 py-0.5 rounded-full bg-cyan-950 text-cyan-400 border border-cyan-500/30">
                ID: {user.id.slice(0, 8)}...
              </span>
            </h3>
            <p class="text-xs text-slate-400">Modify credentials, roles, balance, and account status</p>
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
            <label for="edit-email" class="block text-xs font-semibold text-slate-300 mb-1">Email Address</label>
            <div class="relative">
              <Mail class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="edit-email"
                type="email"
                required
                bind:value={email}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white"
              />
            </div>
          </div>

          <div>
            <label for="edit-username" class="block text-xs font-semibold text-slate-300 mb-1">Username</label>
            <div class="relative">
              <User class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="edit-username"
                type="text"
                required
                bind:value={username}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white"
              />
            </div>
          </div>

          <div>
            <label for="edit-password" class="block text-xs font-semibold text-slate-300 mb-1">New Password (Leave blank to keep current)</label>
            <div class="relative">
              <Lock class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
              <input
                id="edit-password"
                type="text"
                placeholder="Enter new password to reset"
                bind:value={password}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white font-mono placeholder-slate-500"
              />
            </div>
          </div>

          <div>
            <label for="edit-balance" class="block text-xs font-semibold text-slate-300 mb-1">TON Balance</label>
            <div class="relative">
              <Wallet class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-cyan-400" />
              <input
                id="edit-balance"
                type="number"
                step="0.1"
                min="0"
                bind:value={balanceInTon}
                class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-cyan-300 font-mono"
              />
            </div>
          </div>
        </div>

        <div>
          <label for="edit-wallet" class="block text-xs font-semibold text-slate-300 mb-1">TON Wallet Address</label>
          <input
            id="edit-wallet"
            type="text"
            bind:value={walletAddress}
            class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 font-mono"
          />
        </div>

        <div>
          <label for="edit-avatar" class="block text-xs font-semibold text-slate-300 mb-1">Avatar Image URL</label>
          <div class="flex items-center gap-3">
            <img src={avatarUrl || 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150'} alt="Avatar preview" class="w-10 h-10 rounded-xl object-cover border border-cyan-500/40" />
            <input
              id="edit-avatar"
              type="text"
              bind:value={avatarUrl}
              class="flex-1 px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200"
            />
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-3 pt-2">
          <div class="p-3.5 rounded-xl bg-[#081e26] border border-cyan-500/20 flex items-center justify-between">
            <div class="flex items-center gap-2.5">
              <Shield class="w-4 h-4 text-cyan-400" />
              <div>
                <div class="text-xs font-bold text-white">Administrator</div>
                <div class="text-[10px] text-slate-400">IsAdmin Model Flag</div>
              </div>
            </div>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" bind:checked={isAdmin} class="sr-only peer" />
              <div class="w-10 h-5 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-cyan-500"></div>
            </label>
          </div>

          <div class="p-3.5 rounded-xl bg-[#081e26] border border-rose-500/20 flex items-center justify-between">
            <div class="flex items-center gap-2.5">
              <Ban class="w-4 h-4 text-rose-400" />
              <div>
                <div class="text-xs font-bold text-white">Ban Account</div>
                <div class="text-[10px] text-slate-400">Restrict user access</div>
              </div>
            </div>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" bind:checked={isBanned} class="sr-only peer" />
              <div class="w-10 h-5 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-rose-500"></div>
            </label>
          </div>
        </div>

        <div>
          <label for="edit-bio" class="block text-xs font-semibold text-slate-300 mb-1">User Bio</label>
          <textarea
            id="edit-bio"
            rows="2"
            bind:value={bio}
            class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 resize-none"
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
              <span>Saving...</span>
            {:else}
              <CheckCircle2 class="w-4 h-4" />
              <span>Save Changes</span>
            {/if}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}
