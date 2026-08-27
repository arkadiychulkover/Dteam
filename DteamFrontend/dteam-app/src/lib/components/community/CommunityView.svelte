<script lang="ts">
  import { authStore, currentUser } from '../../stores/authStore';
  import { friendsStore } from '../../stores/friendsStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { Users, Sparkles, Heart, MessageSquare, Shield, Trophy } from 'lucide-svelte';

  const familyMembers = $derived($friendsStore.friends.filter(f => f.friend.isInFamily));
</script>

<div class="max-w-7xl mx-auto px-4 lg:px-8 py-6 space-y-8">
  
  <div class="p-6 md:p-8 rounded-2xl bg-gradient-to-r from-blue-950/40 via-indigo-950/40 to-purple-950/40 border border-blue-500/30 shadow-2xl relative overflow-hidden">
    <div class="relative z-10 flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
      <div>
        <div class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-cyan-500/20 text-cyan-300 text-xs font-bold border border-cyan-500/30 uppercase mb-3">
          <Shield class="w-3.5 h-3.5" /> Dteam Family Sharing
        </div>
        <h2 class="text-2xl md:text-3xl font-black text-white font-display">
          Decentralized Family Library
        </h2>
        <p class="text-xs md:text-sm text-slate-300 max-w-xl mt-1.5 leading-relaxed">
          Share your Web3 game collection with trusted family members. Play simultaneously without restrictions.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <div class="flex -space-x-2">
          {#each $friendsStore.friends.slice(0, 3) as f}
            <img
              src={f.friend.avatarUrl}
              alt={f.friend.username}
              class="w-10 h-10 rounded-full border-2 border-[#0a0c14] object-cover"
            />
          {/each}
        </div>
        <div class="text-left">
          <span class="block text-xs font-bold text-white">3 Family Slots Active</span>
          <span class="block text-[10px] text-emerald-400 font-semibold">● Family Sharing Online</span>
        </div>
      </div>
    </div>
  </div>

  <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
    
    <div class="lg:col-span-8 space-y-4">
      <h3 class="text-lg font-bold text-white flex items-center gap-2">
        <Sparkles class="w-4 h-4 text-cyan-400" /> Recent Community Activity
      </h3>

      <div class="p-4 rounded-xl bg-[#111420] border border-slate-800 space-y-4">
        {#each $friendsStore.friends as f}
          <div class="flex items-start gap-3 p-3 rounded-lg bg-slate-900/60 border border-slate-800/80">
            <img
              src={f.friend.avatarUrl}
              alt={f.friend.username}
              class="w-10 h-10 rounded-xl object-cover ring-1 ring-slate-700"
            />
            <div class="flex-1 text-xs">
              <div class="flex items-center justify-between">
                <span class="font-bold text-slate-200">{f.friend.username}</span>
                <span class="text-slate-500 text-[10px]">2 hours ago</span>
              </div>
              <p class="text-slate-400 mt-1">
                Started playing <span class="text-cyan-400 font-semibold">Neon Odyssey 2088</span> and unlocked an on-chain achievement!
              </p>
            </div>
          </div>
        {/each}
      </div>
    </div>

    <div class="lg:col-span-4 space-y-4">
      <h3 class="text-lg font-bold text-white flex items-center gap-2">
        <Trophy class="w-4 h-4 text-amber-400" /> Top Game Creators
      </h3>

      <div class="p-4 rounded-xl bg-[#111420] border border-slate-800 space-y-3">
        <div class="flex items-center justify-between p-3 rounded-lg bg-slate-900/80 border border-slate-800">
          <div class="flex items-center gap-2.5">
            <span class="w-5 font-bold text-amber-400 text-sm">#1</span>
            <div class="w-8 h-8 rounded-lg bg-cyan-500/20 text-cyan-400 flex items-center justify-center font-bold text-xs">
              NX
            </div>
            <div>
              <span class="block text-xs font-bold text-white">NexusStudios</span>
              <span class="block text-[10px] text-slate-400">2 Games Published</span>
            </div>
          </div>
          <span class="text-xs font-mono font-bold text-cyan-400">124.5 TON</span>
        </div>

        <div class="flex items-center justify-between p-3 rounded-lg bg-slate-900/80 border border-slate-800">
          <div class="flex items-center gap-2.5">
            <span class="w-5 font-bold text-slate-300 text-sm">#2</span>
            <div class="w-8 h-8 rounded-lg bg-purple-500/20 text-purple-400 flex items-center justify-center font-bold text-xs">
              CY
            </div>
            <div>
              <span class="block text-xs font-bold text-white">CyberForge</span>
              <span class="block text-[10px] text-slate-400">1 Game Published</span>
            </div>
          </div>
          <span class="text-xs font-mono font-bold text-cyan-400">89.2 TON</span>
        </div>
      </div>
    </div>
  </div>
</div>
