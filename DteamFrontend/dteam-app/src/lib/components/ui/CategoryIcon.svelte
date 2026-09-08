<script lang="ts">
  let {
    category,
    class: className = "w-7 h-7",
  }: {
    category: string;
    class?: string;
  } = $props();
</script>

{#if category === 'Action'}
  <!-- Action: Crossed Energy Swords -->
  <svg class={className} viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs>
      <linearGradient id="actionGrad1" x1="4" y1="4" x2="24" y2="24" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#fbbf24" />
        <stop offset="50%" stop-color="#f97316" />
        <stop offset="100%" stop-color="#ef4444" />
      </linearGradient>
      <linearGradient id="actionGrad2" x1="24" y1="4" x2="4" y2="24" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#fde047" />
        <stop offset="50%" stop-color="#fb923c" />
        <stop offset="100%" stop-color="#dc2626" />
      </linearGradient>
      <filter id="actionGlow" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur stdDeviation="1" result="glow" />
        <feComposite in="SourceGraphic" in2="glow" operator="over" />
      </filter>
    </defs>
    <!-- Left-to-Right Sword -->
    <path
      d="M21.5 6.5L14 14M14 14L8.5 19.5M14 14L11.5 11.5M14 14L16.5 16.5"
      stroke="url(#actionGrad1)"
      stroke-width="2.2"
      stroke-linecap="round"
      stroke-linejoin="round"
      filter="url(#actionGlow)"
    />
    <path d="M23 5L20.5 4L20 6.5L21.5 8L24 7.5L23 5Z" fill="#fde047" />
    <path d="M8.5 19.5L6.5 17.5M6.5 21.5L5 23M7 21L5 19" stroke="#fbbf24" stroke-width="2" stroke-linecap="round" />
    <!-- Right-to-Left Sword -->
    <path
      d="M6.5 6.5L14 14M14 14L19.5 19.5"
      stroke="url(#actionGrad2)"
      stroke-width="2.2"
      stroke-linecap="round"
      stroke-linejoin="round"
      filter="url(#actionGlow)"
    />
    <path d="M5 5L7.5 4L8 6.5L6.5 8L4 7.5L5 5Z" fill="#fde047" />
    <path d="M19.5 19.5L21.5 17.5M21.5 21.5L23 23M21 21L23 19" stroke="#fb923c" stroke-width="2" stroke-linecap="round" />
    <!-- Center Clash Spark -->
    <circle cx="14" cy="14" r="1.5" fill="#ffffff" />
    <path d="M14 10.5V17.5M10.5 14H17.5" stroke="#ffffff" stroke-width="1" stroke-linecap="round" opacity="0.8" />
  </svg>

{:else if category === 'RPG'}
  <!-- RPG: Cyber Aegis Shield -->
  <svg class={className} viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs>
      <linearGradient id="rpgGrad" x1="14" y1="3" x2="14" y2="25" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#c084fc" />
        <stop offset="50%" stop-color="#818cf8" />
        <stop offset="100%" stop-color="#4f46e5" />
      </linearGradient>
      <linearGradient id="rpgCore" x1="14" y1="8" x2="14" y2="20" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#e879f9" />
        <stop offset="100%" stop-color="#6366f1" />
      </linearGradient>
      <filter id="rpgGlow" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur stdDeviation="1.2" result="glow" />
        <feComposite in="SourceGraphic" in2="glow" operator="over" />
      </filter>
    </defs>
    <!-- Outer Shield Frame -->
    <path
      d="M14 3.5L22 6.5C22 14.5 18.5 21.5 14 24.5C9.5 21.5 6 14.5 6 6.5L14 3.5Z"
      stroke="url(#rpgGrad)"
      stroke-width="2"
      stroke-linejoin="round"
      fill="#2e1065"
      fill-opacity="0.4"
      filter="url(#rpgGlow)"
    />
    <!-- Inner Facet -->
    <path
      d="M14 6.5L19.5 8.5C19.5 14 17 19 14 21.5C11 19 8.5 14 8.5 8.5L14 6.5Z"
      stroke="#a855f7"
      stroke-width="1.2"
      stroke-linejoin="round"
      opacity="0.7"
    />
    <!-- Central Crest Diamond -->
    <path d="M14 9L17 13.5L14 18L11 13.5L14 9Z" fill="url(#rpgCore)" />
    <!-- Inner Cross Glint -->
    <path d="M14 7V20M8.5 13.5H19.5" stroke="#f5d0fe" stroke-width="1" stroke-linecap="round" opacity="0.6" />
    <circle cx="14" cy="13.5" r="1.2" fill="#ffffff" />
  </svg>

{:else if category === 'Strategy'}
  <!-- Strategy: Holographic Chess Piece / Tactical Grid -->
  <svg class={className} viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs>
      <linearGradient id="stratGrad" x1="14" y1="3" x2="14" y2="25" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#38bdf8" />
        <stop offset="50%" stop-color="#0284c7" />
        <stop offset="100%" stop-color="#0369a1" />
      </linearGradient>
      <linearGradient id="stratHighlight" x1="6" y1="14" x2="22" y2="14" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#67e8f9" />
        <stop offset="100%" stop-color="#38bdf8" />
      </linearGradient>
      <filter id="stratGlow" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur stdDeviation="1" result="glow" />
        <feComposite in="SourceGraphic" in2="glow" operator="over" />
      </filter>
    </defs>
    <!-- Chess Knight / Pawn Crown -->
    <circle cx="14" cy="6" r="3" stroke="url(#stratGrad)" stroke-width="2" fill="#0c4a6e" fill-opacity="0.5" filter="url(#stratGlow)" />
    <circle cx="14" cy="6" r="1" fill="#bae6fd" />
    <!-- Neck / Collar -->
    <path d="M11 11H17L18.5 14H9.5L11 11Z" stroke="url(#stratGrad)" stroke-width="1.6" fill="#075985" fill-opacity="0.4" />
    <!-- Body Pyramid Pillars -->
    <path d="M10 14L8 21H20L18 14H10Z" stroke="url(#stratGrad)" stroke-width="1.8" stroke-linejoin="round" fill="#082f49" fill-opacity="0.6" filter="url(#stratGlow)" />
    <!-- Base Platform -->
    <path d="M6 21H22V24.5C22 24.8 21.8 25 21.5 25H6.5C6.2 25 6 24.8 6 24.5V21Z" stroke="url(#stratHighlight)" stroke-width="1.8" fill="#0369a1" />
    <!-- Digital Tactical Grid Overlay lines -->
    <path d="M14 11V21" stroke="#38bdf8" stroke-width="1.2" stroke-dasharray="1 1.5" />
    <path d="M9 17.5H19" stroke="#7dd3fc" stroke-width="1" opacity="0.8" />
  </svg>

{:else if category === 'Adventure'}
  <!-- Adventure: Cyber Astrolabe / Navigational Compass -->
  <svg class={className} viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs>
      <linearGradient id="advGrad" x1="4" y1="4" x2="24" y2="24" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#34d399" />
        <stop offset="50%" stop-color="#10b981" />
        <stop offset="100%" stop-color="#059669" />
      </linearGradient>
      <linearGradient id="advNeedleN" x1="14" y1="7" x2="14" y2="14" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#f87171" />
        <stop offset="100%" stop-color="#ef4444" />
      </linearGradient>
      <linearGradient id="advNeedleS" x1="14" y1="14" x2="14" y2="21" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#94a3b8" />
        <stop offset="100%" stop-color="#64748b" />
      </linearGradient>
      <filter id="advGlow" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur stdDeviation="1.1" result="glow" />
        <feComposite in="SourceGraphic" in2="glow" operator="over" />
      </filter>
    </defs>
    <!-- Outer Ring -->
    <circle cx="14" cy="14" r="10.5" stroke="url(#advGrad)" stroke-width="2" fill="#064e3b" fill-opacity="0.3" filter="url(#advGlow)" />
    <!-- Inner Tick Circle -->
    <circle cx="14" cy="14" r="8" stroke="#6ee7b7" stroke-width="1" stroke-dasharray="2 3" opacity="0.6" />
    <!-- Cardinal Notches -->
    <path d="M14 4V6.5M14 21.5V24M4 14H6.5M21.5 14H24" stroke="#34d399" stroke-width="1.8" stroke-linecap="round" />
    <!-- 45-degree sub-ticks -->
    <path d="M7 7L8.5 8.5M19.5 19.5L21 21M21 7L19.5 8.5M8.5 19.5L7 21" stroke="#10b981" stroke-width="1.2" stroke-linecap="round" opacity="0.6" />
    <!-- Compass Needle: North (Red) -->
    <polygon points="14,6.5 16.5,14 14,12.5 11.5,14" fill="url(#advNeedleN)" />
    <!-- Compass Needle: South (Silver) -->
    <polygon points="14,21.5 16.5,14 14,15.5 11.5,14" fill="url(#advNeedleS)" />
    <!-- Center Pivot Pin -->
    <circle cx="14" cy="14" r="2" fill="#fbbf24" stroke="#ffffff" stroke-width="0.8" />
  </svg>

{:else if category === 'Shooter'}
  <!-- Shooter: Tactical Sniper Crosshair / Reticle -->
  <svg class={className} viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs>
      <linearGradient id="shootGrad" x1="4" y1="4" x2="24" y2="24" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#f87171" />
        <stop offset="50%" stop-color="#ef4444" />
        <stop offset="100%" stop-color="#dc2626" />
      </linearGradient>
      <filter id="shootGlow" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur stdDeviation="1.1" result="glow" />
        <feComposite in="SourceGraphic" in2="glow" operator="over" />
      </filter>
    </defs>
    <!-- Outer Segmented Ring -->
    <circle cx="14" cy="14" r="10" stroke="url(#shootGrad)" stroke-width="1.8" stroke-dasharray="12 4" filter="url(#shootGlow)" />
    <!-- Inner Concentric Ring -->
    <circle cx="14" cy="14" r="5" stroke="#fca5a5" stroke-width="1.2" opacity="0.8" />
    <!-- Crosshair Lines with central gap -->
    <path d="M14 2.5V8.5M14 19.5V25.5M2.5 14H8.5M19.5 14H25.5" stroke="url(#shootGrad)" stroke-width="2" stroke-linecap="round" />
    <!-- Target Ranging Sub-ticks -->
    <path d="M12.5 6H15.5M12.5 22H15.5M6 12.5V15.5M22 12.5V15.5" stroke="#f87171" stroke-width="1.2" stroke-linecap="round" />
    <!-- Center Laser Dot -->
    <circle cx="14" cy="14" r="1.5" fill="#ffffff" />
    <circle cx="14" cy="14" r="0.6" fill="#ef4444" />
  </svg>

{:else if category === 'Indie'}
  <!-- Indie: Radiant Prismatic Crystal / Diamond -->
  <svg class={className} viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs>
      <linearGradient id="indieGrad" x1="14" y1="3" x2="14" y2="25" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#f472b6" />
        <stop offset="50%" stop-color="#ec4899" />
        <stop offset="100%" stop-color="#a855f7" />
      </linearGradient>
      <linearGradient id="indieFacetTop" x1="8" y1="5" x2="20" y2="12" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#fbcfe8" />
        <stop offset="100%" stop-color="#f472b6" />
      </linearGradient>
      <filter id="indieGlow" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur stdDeviation="1.2" result="glow" />
        <feComposite in="SourceGraphic" in2="glow" operator="over" />
      </filter>
    </defs>
    <!-- Main Diamond Polygon -->
    <path
      d="M7 10L14 3.5L21 10L14 24.5L7 10Z"
      stroke="url(#indieGrad)"
      stroke-width="1.8"
      stroke-linejoin="round"
      fill="#4a044e"
      fill-opacity="0.3"
      filter="url(#indieGlow)"
    />
    <!-- Upper Facets Horizon -->
    <path d="M7 10H21" stroke="#f472b6" stroke-width="1.4" />
    <!-- Upper Triangles -->
    <path d="M11 10L14 3.5L17 10" stroke="#fbcfe8" stroke-width="1.2" stroke-linejoin="round" fill="url(#indieFacetTop)" fill-opacity="0.4" />
    <!-- Lower Facets to Point -->
    <path d="M11 10L14 24.5L17 10" stroke="#c084fc" stroke-width="1.2" stroke-linejoin="round" />
    <!-- Sparkle Stars at Vertex -->
    <path d="M21 5L22 7.5L24.5 8.5L22 9.5L21 12L20 9.5L17.5 8.5L20 7.5L21 5Z" fill="#ffffff" opacity="0.9" />
  </svg>

{:else if category === 'Cyberpunk'}
  <!-- Cyberpunk: Neon Lightning Bolt & Circuitry -->
  <svg class={className} viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs>
      <linearGradient id="cyberGrad" x1="8" y1="2" x2="20" y2="26" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#22d3ee" />
        <stop offset="50%" stop-color="#06b6d4" />
        <stop offset="100%" stop-color="#3b82f6" />
      </linearGradient>
      <filter id="cyberGlow" x="-25%" y="-25%" width="150%" height="150%">
        <feGaussianBlur stdDeviation="1.5" result="glow" />
        <feComposite in="SourceGraphic" in2="glow" operator="over" />
      </filter>
    </defs>
    <!-- Circuit Trace Lines behind bolt -->
    <path d="M5 8H9L11 11" stroke="#0891b2" stroke-width="1.4" stroke-linecap="round" opacity="0.6" />
    <circle cx="5" cy="8" r="1.2" fill="#22d3ee" />
    <path d="M23 20H19L17 17" stroke="#0891b2" stroke-width="1.4" stroke-linecap="round" opacity="0.6" />
    <circle cx="23" cy="20" r="1.2" fill="#22d3ee" />
    <!-- Glowing Lightning Bolt Core -->
    <path
      d="M16 2.5L7.5 14H13.5L11.5 25.5L21 12H15L16 2.5Z"
      stroke="url(#cyberGrad)"
      stroke-width="1.6"
      stroke-linejoin="round"
      fill="url(#cyberGrad)"
      filter="url(#cyberGlow)"
    />
    <!-- Inner Hot White Energy Spine -->
    <path
      d="M15 5L9.5 13H14L12.8 21.5L18.5 12.5H14.5L15 5Z"
      fill="#ffffff"
      opacity="0.8"
    />
  </svg>

{:else if category === 'Horror'}
  <!-- Horror: Menacing Cyber Eye / Biological Glitch -->
  <svg class={className} viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs>
      <linearGradient id="horrorGrad" x1="4" y1="14" x2="24" y2="14" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#fb7185" />
        <stop offset="50%" stop-color="#e11d48" />
        <stop offset="100%" stop-color="#9f1239" />
      </linearGradient>
      <linearGradient id="horrorIris" x1="14" y1="9" x2="14" y2="19" gradientUnits="userSpaceOnUse">
        <stop offset="0%" stop-color="#f43f5e" />
        <stop offset="100%" stop-color="#4c0519" />
      </linearGradient>
      <filter id="horrorGlow" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur stdDeviation="1.2" result="glow" />
        <feComposite in="SourceGraphic" in2="glow" operator="over" />
      </filter>
    </defs>
    <!-- Eyeball Oval Profile -->
    <path
      d="M3 14C5.5 8 10 5.5 14 5.5C18 5.5 22.5 8 25 14C22.5 20 18 22.5 14 22.5C10 22.5 5.5 20 3 14Z"
      stroke="url(#horrorGrad)"
      stroke-width="1.8"
      stroke-linejoin="round"
      fill="#1c1917"
      fill-opacity="0.7"
      filter="url(#horrorGlow)"
    />
    <!-- Iris -->
    <circle cx="14" cy="14" r="5" stroke="#f43f5e" stroke-width="1.4" fill="url(#horrorIris)" />
    <!-- Slit Pupil -->
    <path
      d="M14 9.5C13 11 12.2 12.5 12.2 14C12.2 15.5 13 17 14 18.5C15 17 15.8 15.5 15.8 14C15.8 12.5 15 11 14 9.5Z"
      fill="#000000"
    />
    <!-- Bloodshot / Circuit Veins -->
    <path d="M5.5 11L8.5 12.5M22.5 11L19.5 12.5M6 16.5L9 15.5M22 16.5L19 15.5" stroke="#f43f5e" stroke-width="1" stroke-linecap="round" opacity="0.6" />
    <!-- Glint Reflection -->
    <circle cx="16" cy="11.5" r="1.2" fill="#ffffff" />
  </svg>

{:else}
  <!-- Default Fallback Category Icon -->
  <svg class={className} viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="5" y="5" width="18" height="18" rx="5" stroke="#38bdf8" stroke-width="1.8" fill="#082f49" fill-opacity="0.4" />
    <path d="M14 9V19M9 14H19" stroke="#38bdf8" stroke-width="2" stroke-linecap="round" />
  </svg>
{/if}
