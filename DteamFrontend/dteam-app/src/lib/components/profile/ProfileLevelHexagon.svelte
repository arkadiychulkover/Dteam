<script lang="ts">
  let {
    level = 0,
    size = 'md',
    class: className = '',
    title = ''
  }: {
    level?: number;
    size?: 'sm' | 'md' | 'lg';
    class?: string;
    title?: string;
  } = $props();

  const sizeClasses = {
    sm: 'w-7 h-7',
    md: 'w-9 h-9',
    lg: 'w-11 h-11'
  };

  const dynamicFontSize = $derived.by(() => {
    const len = String(level).length;
    if (size === 'sm') {
      if (len >= 5) return 'text-[7px]';
      if (len >= 4) return 'text-[8px]';
      if (len >= 3) return 'text-[9px]';
      return 'text-[10px]';
    }
    if (size === 'lg') {
      if (len >= 6) return 'text-[10px]';
      if (len >= 5) return 'text-xs';
      if (len >= 4) return 'text-sm';
      if (len >= 3) return 'text-base';
      return 'text-lg';
    }

    if (len >= 6) return 'text-[8px]';
    if (len >= 5) return 'text-[9px]';
    if (len >= 4) return 'text-[11px]';
    if (len >= 3) return 'text-xs';
    return 'text-sm';
  });
</script>

<div
  class="relative flex items-center justify-center shrink-0 {sizeClasses[size] || sizeClasses.md} {className}"
  title={title || `Рівень ${level}`}
>
  <svg
    class="w-full h-full text-cyan-400 drop-shadow-[0_0_8px_rgba(34,211,238,0.45)] transition-transform duration-200 group-hover:scale-105"
    viewBox="0 0 100 100"
    fill="none"
  >
    <polygon
      points="50,4 94,26 94,74 50,96 6,74 6,26"
      stroke="currentColor"
      stroke-width="7"
      stroke-linejoin="round"
      fill="#021c25"
    />
  </svg>
  <span class="absolute font-black text-white font-mono leading-none select-none {dynamicFontSize}">
    {level}
  </span>
</div>
