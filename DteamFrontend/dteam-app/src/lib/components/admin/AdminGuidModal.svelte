<script lang="ts">
import { adminService, activeAdminIdStore } from '../../services/adminService';
  import { uiStore } from '../../stores/uiStore';
  import type { Duser } from '../../types';
  import { X, KeyRound, Wand2, Copy, Check, ShieldCheck, UserCheck } from 'lucide-svelte';

  interface Props {
    isOpen: boolean;
    onClose: () => void;
    users: Duser[];
    onGuidChanged: () => void;
  }

  let { isOpen, onClose, users, onGuidChanged }: Props = $props();

  let inputGuid = $state('');
  let isCopied = $state(false);
  let isAutoDetecting = $state(false);

  $effect(() => {
    if (isOpen) {
      inputGuid = $activeAdminIdStore || '';
    }
  });

  const adminUsers = $derived(users.filter((u) => u.isAdmin));

  function handleSave() {
    if (!inputGuid.trim()) return;
    adminService.setActiveAdminId(inputGuid.trim());
    uiStore.addToast({
      title: 'Admin ID Updated',
      message: `Active testing admin ID set to: ${inputGuid.trim().slice(0, 8)}...`,
      type: 'success',
    });
    onGuidChanged();
    onClose();
  }

  async function handleAutoDetect() {
    isAutoDetecting = true;
    try {
      const detectedId = await adminService.autoDiscoverAdminGuid();
      inputGuid = detectedId;
      uiStore.addToast({
        title: 'Auto-Discovery Successful',
        message: `Discovered and set admin GUID: ${detectedId.slice(0, 8)}...`,
        type: 'success',
      });
      onGuidChanged();
    } catch (e: any) {
      uiStore.addToast({
        title: 'Discovery Failed',
        message: e.message || 'Could not auto-detect admin GUID.',
        type: 'error',
      });
    } finally {
      isAutoDetecting = false;
    }
  }

  function handleCopy() {
    if (!inputGuid) return;
    navigator.clipboard.writeText(inputGuid);
    isCopied = true;
    setTimeout(() => (isCopied = false), 2000);
  }

  function handleSelectUser(user: Duser) {
    inputGuid = user.id;
  }
</script>

{#if isOpen}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in">
    <div class="relative w-full max-w-lg bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl shadow-cyan-950/50 p-6 overflow-hidden flex flex-col">
      <div class="flex items-center justify-between pb-4 border-b border-cyan-950">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-500/20 to-emerald-500/20 border border-cyan-500/40 flex items-center justify-center text-cyan-400">
            <KeyRound class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-base font-bold text-white tracking-wide flex items-center gap-2">
              Active Admin GUID (Testing Parameter)
            </h3>
            <p class="text-xs text-slate-400">Passed automatically to all `/api/admin/*?adminId=...` endpoints</p>
          </div>
        </div>
        <button
          onclick={onClose}
          class="p-2 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800/60 transition-colors cursor-pointer"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <div class="py-4 space-y-4">
        <div>
          <label for="guid-input" class="block text-xs font-semibold text-slate-300 mb-1">Current Active Admin GUID</label>
          <div class="flex items-center gap-2">
            <input
              id="guid-input"
              type="text"
              placeholder="00000000-0000-0000-0000-000000000000"
              bind:value={inputGuid}
              class="flex-1 px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/30 focus:border-cyan-400 focus:outline-none text-xs text-cyan-300 font-mono"
            />
            <button
              type="button"
              onclick={handleCopy}
              class="p-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 hover:text-white border border-slate-700 transition-colors cursor-pointer"
              title="Copy GUID"
            >
              {#if isCopied}
                <Check class="w-4 h-4 text-emerald-400" />
              {:else}
                <Copy class="w-4 h-4" />
              {/if}
            </button>
          </div>
        </div>

        <div>
          <button
            type="button"
            onclick={handleAutoDetect}
            disabled={isAutoDetecting}
            class="w-full flex items-center justify-center gap-2 py-2.5 px-4 rounded-xl bg-cyan-950/80 hover:bg-cyan-900 border border-cyan-500/40 text-cyan-300 font-semibold text-xs transition-all cursor-pointer shadow-inner"
          >
            <Wand2 class="w-4 h-4 text-cyan-400" />
            <span>{isAutoDetecting ? 'Querying database...' : 'Auto-Detect Admin GUID from Database'}</span>
          </button>
        </div>

        {#if adminUsers.length > 0}
          <div class="pt-2">
            <div class="text-xs font-semibold text-slate-400 mb-2 flex items-center gap-1.5">
              <ShieldCheck class="w-3.5 h-3.5 text-emerald-400" />
              <span>Registered Admins in Database ({adminUsers.length}):</span>
            </div>
            <div class="space-y-1.5 max-h-36 overflow-y-auto pr-1">
              {#each adminUsers as adminUser}
                <button
                  type="button"
                  onclick={() => handleSelectUser(adminUser)}
                  class="w-full text-left p-2 rounded-xl bg-[#0a232c] hover:bg-[#0d303c] border border-cyan-500/20 hover:border-cyan-400/50 flex items-center justify-between transition-all cursor-pointer group"
                >
                  <div class="flex items-center gap-2.5">
                    <img src={adminUser.avatarUrl || 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150'} alt="avatar" class="w-6 h-6 rounded-lg object-cover" />
                    <div>
                      <div class="text-xs font-bold text-slate-200 group-hover:text-cyan-300 flex items-center gap-1.5">
                        {adminUser.username}
                        <span class="text-[10px] text-cyan-400/80 font-normal">({adminUser.email})</span>
                      </div>
                      <div class="text-[10px] text-slate-500 font-mono">{adminUser.id}</div>
                    </div>
                  </div>
                  <UserCheck class="w-4 h-4 text-slate-500 group-hover:text-cyan-400 opacity-0 group-hover:opacity-100 transition-opacity" />
                </button>
              {/each}
            </div>
          </div>
        {/if}
      </div>

      <div class="flex items-center justify-end gap-3 pt-4 border-t border-cyan-950">
        <button
          type="button"
          onclick={onClose}
          class="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-xs font-semibold text-slate-300 transition-colors cursor-pointer"
        >
          Cancel
        </button>
        <button
          type="button"
          onclick={handleSave}
          class="px-5 py-2 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-bold text-xs shadow-lg shadow-cyan-500/25 transition-all cursor-pointer"
        >
          Save & Apply
        </button>
      </div>
    </div>
  </div>
{/if}

