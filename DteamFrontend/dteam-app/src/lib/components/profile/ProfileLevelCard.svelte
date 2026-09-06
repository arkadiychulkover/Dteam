<script lang="ts">
  import { calculateProfileLevel } from '../../utils/levelUtils';
  import ProfileLevelHexagon from './ProfileLevelHexagon.svelte';

  let {
    tokens = 0,
    compact = false,
    class: className = ''
  }: {
    tokens?: number | null;
    compact?: boolean;
    class?: string;
  } = $props();

  const levelInfo = $derived(calculateProfileLevel(tokens));

</script>

<div class="space-y-2.5 {className}">
  <div class="flex items-center justify-between">
    <div class="flex items-center gap-2.5">
      <span class="text-base sm:text-lg font-bold text-white tracking-wide">Рівень</span>
      <ProfileLevelHexagon level={levelInfo.level} size={compact ? 'sm' : 'md'} />
    </div>
    <div class="text-right">
      <span class="text-xs text-cyan-300 font-mono font-semibold">
        {levelInfo.currentXp.toLocaleString('uk-UA')} <span class="text-[10px] text-cyan-500 font-normal">XP</span>
      </span>
    </div>
  </div>

  <div class="space-y-1">
    <div class="w-full h-1.5 bg-[#02171d] rounded-full overflow-hidden border border-cyan-900/50">
      <div
        class="h-full bg-gradient-to-r from-cyan-500 to-emerald-400 rounded-full transition-all duration-500"
        style="width: {levelInfo.progressPercent}%"
      ></div>
    </div>
    <div class="flex items-center justify-between text-[11px]">
      <span class="text-slate-400">До {levelInfo.nextLevel} рівня:</span>
      <span class="text-cyan-300 font-medium font-mono">
        залишилось {levelInfo.xpRemaining.toLocaleString('uk-UA')} <span class="text-[10px] text-slate-500">DTP</span>
      </span>
    </div>
  </div>
</div>

