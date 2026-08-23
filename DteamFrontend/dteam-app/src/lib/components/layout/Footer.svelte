<script lang="ts">
  import { Gamepad2, Shield, Cpu, ExternalLink } from 'lucide-svelte';
</script>

<footer class="mt-20 border-t border-slate-800/80 bg-[#0a0c14] py-12 px-4 lg:px-8 text-xs text-slate-400">
  <div class="max-w-7xl mx-auto flex flex-col md:flex-row items-center justify-between gap-6">
    <div class="flex items-center gap-3">
      <div class="w-8 h-8 rounded-lg bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center text-white">
        <Gamepad2 class="w-4 h-4" />
      </div>
      <div>
        <span class="font-bold text-slate-200 tracking-wider font-['Outfit']">DTEAM PLATFORM</span>
        <p class="text-[11px] text-slate-500 mt-0.5">The Next-Gen Web3 Decentralized Steam Ecosystem on The Open Network (TON).</p>
      </div>
    </div>

    <div class="flex items-center gap-6 text-[11px]">
      <span class="flex items-center gap-1 text-slate-400">
        <Shield class="w-3.5 h-3.5 text-cyan-400" /> Decentralized Vault
      </span>
      <span class="flex items-center gap-1 text-slate-400">
        <Cpu class="w-3.5 h-3.5 text-blue-400" /> Powered by ASP.NET Core & Svelte
      </span>
    </div>
  </div>
</footer>
