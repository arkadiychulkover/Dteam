<script lang="ts">
import { Gamepad2, Heart } from 'lucide-svelte';
  import { uiStore } from '../../stores/uiStore';
</script>

<footer class="mt-24 border-t border-[#0b2834] bg-[#030d12] py-10 px-4 lg:px-8 relative z-20 shadow-2xl">
  <div class="max-w-7xl mx-auto flex flex-col items-center justify-between gap-6 text-center">
    <div class="flex items-center gap-2.5">
      <div class="w-8 h-8 rounded-xl bg-gradient-to-br from-cyan-400 to-emerald-400 flex items-center justify-center text-black font-black shadow-lg shadow-cyan-500/25">
        <Gamepad2 class="w-4 h-4" />
      </div>
      <span class="font-black text-xl text-white font-display tracking-tight">
        DTEAM<span class="text-cyan-400">.</span>
      </span>
    </div>

    <p class="text-xs text-slate-300 max-w-2xl leading-relaxed font-medium">
      © 2026, Dteam Inc. Всі права захищено. Усі торговельні марки та логотипи є власністю відповідних правовласників.
    </p>

    <div class="flex flex-wrap items-center justify-center gap-6 text-xs font-semibold text-slate-300">
      <button
        onclick={() => uiStore.addToast({ title: 'Інформація', message: 'Умови використання сервісу Dteam.', type: 'info' })}
        class="hover:text-cyan-400 transition-colors cursor-pointer"
      >
        Умови використання
      </button>
      <span class="text-slate-600">•</span>
      <button
        onclick={() => uiStore.addToast({ title: 'Інформація', message: 'Політика конфіденційності та захисту даних.', type: 'info' })}
        class="hover:text-cyan-400 transition-colors cursor-pointer"
      >
        Політика конфіденційності
      </button>
      <span class="text-slate-600">•</span>
      <button
        onclick={() => uiStore.addToast({ title: 'Інформація', message: 'Правила повернення коштів платформи Dteam.', type: 'info' })}
        class="hover:text-cyan-400 transition-colors cursor-pointer"
      >
        Політика повернення коштів магазину
      </button>
    </div>
  </div>
</footer>

