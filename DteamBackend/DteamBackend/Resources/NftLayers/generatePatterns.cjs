const sharp = require('d:/Dteam/DteamBackend/DteamBackend/Services/jsServices/node_modules/sharp');
const fs = require('fs');
const path = require('path');

const layersDir = 'd:/Dteam/DteamBackend/DteamBackend/Resources/NftLayers/patterns';

if (!fs.existsSync(layersDir)) {
  fs.mkdirSync(layersDir, { recursive: true });
}

// 1. Neon Floating Dollar Signs
const svg1 = `
<svg width="1024" height="1024" viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <filter id="glow1" x="-50%" y="-50%" width="200%" height="200%">
      <feGaussianBlur stdDeviation="8" result="blur" />
      <feMerge>
        <feMergeNode in="blur" />
        <feMergeNode in="SourceGraphic" />
      </feMerge>
    </filter>
  </defs>
  <g filter="url(#glow1)" fill="#22d3ee" opacity="0.8" font-family="Impact, Arial Black, sans-serif" font-weight="bold">
    <text x="120" y="180" font-size="90" transform="rotate(-15 120 180)">$</text>
    <text x="820" y="220" font-size="110" transform="rotate(20 820 220)">$</text>
    <text x="80" y="460" font-size="75" transform="rotate(10 80 460)">$</text>
    <text x="890" y="510" font-size="85" transform="rotate(-25 890 510)">$</text>
    <text x="240" y="320" font-size="50" opacity="0.5">$</text>
    <text x="760" y="380" font-size="60" opacity="0.5">$</text>
    <text x="160" y="720" font-size="70" transform="rotate(15 160 720)">$</text>
    <text x="840" y="740" font-size="95" transform="rotate(-12 840 740)">$</text>
    <text x="480" y="130" font-size="80" transform="rotate(5 480 130)" fill="#4ade80">$</text>
  </g>
  <g fill="#ffffff" opacity="0.85">
    <polygon points="180,240 185,255 200,260 185,265 180,280 175,265 160,260 175,255" />
    <polygon points="840,320 843,332 855,335 843,338 840,350 837,338 825,335 837,332" />
    <polygon points="120,600 123,612 135,615 123,618 120,630 117,618 105,615 117,612" />
    <polygon points="890,640 894,655 910,660 894,665 890,680 886,665 870,660 886,655" />
  </g>
</svg>
`;

// 2. Golden Coins & Star Confetti
const svg2 = `
<svg width="1024" height="1024" viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <radialGradient id="goldGrad" cx="40%" cy="35%" r="60%">
      <stop offset="0%" stop-color="#fffbeb" />
      <stop offset="40%" stop-color="#fde047" />
      <stop offset="80%" stop-color="#eab308" />
      <stop offset="100%" stop-color="#ca8a04" />
    </radialGradient>
    <filter id="goldGlow" x="-30%" y="-30%" width="160%" height="160%">
      <feGaussianBlur stdDeviation="5" result="blur" />
      <feMerge><feMergeNode in="blur" /><feMergeNode in="SourceGraphic" /></feMerge>
    </filter>
  </defs>
  <g filter="url(#goldGlow)">
    <g transform="translate(140, 160) rotate(-20)">
      <ellipse cx="0" cy="0" rx="45" ry="35" fill="url(#goldGrad)" stroke="#a16207" stroke-width="4" />
      <text x="0" y="10" text-anchor="middle" font-family="Arial, sans-serif" font-weight="bold" font-size="28" fill="#78350f">D</text>
    </g>
    <g transform="translate(850, 200) rotate(25)">
      <ellipse cx="0" cy="0" rx="50" ry="38" fill="url(#goldGrad)" stroke="#a16207" stroke-width="4" />
      <text x="0" y="11" text-anchor="middle" font-family="Arial, sans-serif" font-weight="bold" font-size="32" fill="#78350f">$</text>
    </g>
    <g transform="translate(100, 480) rotate(15)">
      <ellipse cx="0" cy="0" rx="38" ry="28" fill="url(#goldGrad)" stroke="#a16207" stroke-width="3" />
      <text x="0" y="8" text-anchor="middle" font-family="Arial, sans-serif" font-weight="bold" font-size="22" fill="#78350f">D</text>
    </g>
    <g transform="translate(900, 480) rotate(-35)">
      <ellipse cx="0" cy="0" rx="42" ry="32" fill="url(#goldGrad)" stroke="#a16207" stroke-width="4" />
      <text x="0" y="9" text-anchor="middle" font-family="Arial, sans-serif" font-weight="bold" font-size="25" fill="#78350f">$</text>
    </g>
    <g transform="translate(150, 750) rotate(-10)">
      <ellipse cx="0" cy="0" rx="46" ry="34" fill="url(#goldGrad)" stroke="#a16207" stroke-width="4" />
      <text x="0" y="10" text-anchor="middle" font-family="Arial, sans-serif" font-weight="bold" font-size="26" fill="#78350f">D</text>
    </g>
    <g transform="translate(860, 760) rotate(18)">
      <ellipse cx="0" cy="0" rx="48" ry="36" fill="url(#goldGrad)" stroke="#a16207" stroke-width="4" />
      <text x="0" y="10" text-anchor="middle" font-family="Arial, sans-serif" font-weight="bold" font-size="28" fill="#78350f">$</text>
    </g>
  </g>
  <g fill="#fef08a" opacity="0.75">
    <circle cx="220" cy="280" r="6" />
    <circle cx="800" cy="340" r="8" />
    <circle cx="260" cy="640" r="5" />
    <circle cx="790" cy="620" r="7" />
    <circle cx="512" cy="100" r="9" />
  </g>
</svg>
`;

// 3. Cyberpunk Laser Grid & Tech HUD
const svg3 = `
<svg width="1024" height="1024" viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <filter id="cyanGlow">
      <feGaussianBlur stdDeviation="4" result="blur" />
      <feMerge><feMergeNode in="blur" /><feMergeNode in="SourceGraphic" /></feMerge>
    </filter>
  </defs>
  <g filter="url(#cyanGlow)" stroke="#06b6d4" stroke-width="2" opacity="0.75" fill="none">
    <path d="M 60 140 L 60 60 L 140 60" stroke-width="4" />
    <path d="M 964 140 L 964 60 L 884 60" stroke-width="4" />
    <path d="M 60 884 L 60 964 L 140 964" stroke-width="4" />
    <path d="M 964 884 L 964 964 L 884 964" stroke-width="4" />

    <circle cx="140" cy="140" r="25" stroke="#38bdf8" />
    <line x1="140" y1="100" x2="140" y2="180" stroke="#38bdf8" />
    <line x1="100" y1="140" x2="180" y2="140" stroke="#38bdf8" />

    <circle cx="884" cy="140" r="25" stroke="#38bdf8" />
    <line x1="884" y1="100" x2="884" y2="180" stroke="#38bdf8" />
    <line x1="844" y1="140" x2="924" y2="140" stroke="#38bdf8" />

    <line x1="40" y1="300" x2="240" y2="300" stroke="#f43f5e" stroke-width="3" stroke-dasharray="15 10" />
    <line x1="784" y1="300" x2="984" y2="300" stroke="#f43f5e" stroke-width="3" stroke-dasharray="15 10" />
    <line x1="40" y1="700" x2="240" y2="700" stroke="#22d3ee" stroke-width="3" stroke-dasharray="10 15" />
    <line x1="784" y1="700" x2="984" y2="700" stroke="#22d3ee" stroke-width="3" stroke-dasharray="10 15" />
  </g>
  <text x="70" y="95" fill="#06b6d4" font-family="Courier New, monospace" font-weight="bold" font-size="18" opacity="0.85">NFT://DTEAM.DNFT.001</text>
  <text x="780" y="95" fill="#06b6d4" font-family="Courier New, monospace" font-weight="bold" font-size="18" opacity="0.85">LOCK: ACQUIRED</text>
</svg>
`;

// 4. Trading Bull Market Chart & Green Arrows
const svg4 = `
<svg width="1024" height="1024" viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <filter id="greenGlow">
      <feGaussianBlur stdDeviation="6" result="blur" />
      <feMerge><feMergeNode in="blur" /><feMergeNode in="SourceGraphic" /></feMerge>
    </filter>
  </defs>
  <g filter="url(#greenGlow)">
    <path d="M 40 850 Q 200 800, 300 650 T 500 500 T 750 350 T 960 150" fill="none" stroke="#22c55e" stroke-width="6" />
    <polygon points="980,130 940,150 965,175" fill="#22c55e" />
  </g>
  <g fill="#22c55e" stroke="#22c55e" stroke-width="2" opacity="0.8">
    <line x1="70" y1="680" x2="70" y2="800" />
    <rect x="60" y="710" width="20" height="60" rx="2" />
    <line x1="120" y1="600" x2="120" y2="740" />
    <rect x="110" y="630" width="20" height="80" rx="2" />
    <line x1="170" y1="550" x2="170" y2="670" />
    <rect x="160" y="580" width="20" height="70" rx="2" />

    <line x1="840" y1="300" x2="840" y2="450" />
    <rect x="830" y="330" width="20" height="90" rx="2" />
    <line x1="900" y1="200" x2="900" y2="380" />
    <rect x="890" y="230" width="20" height="110" rx="2" />
    <line x1="960" y1="120" x2="960" y2="300" />
    <rect x="950" y="150" width="20" height="120" rx="2" />
  </g>
  <g font-family="Impact, Arial Black, sans-serif" font-size="42" fill="#22c55e" opacity="0.9" filter="url(#greenGlow)">
    <text x="80" y="180">BULLISH ▲</text>
    <text x="730" y="880">+1000% 🚀</text>
  </g>
</svg>
`;

// 5. Classic Banknote Guilloche Security Stamp & Ornaments
const svg5 = `
<svg width="1024" height="1024" viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <filter id="goldStampGlow">
      <feGaussianBlur stdDeviation="3" result="blur" />
      <feMerge><feMergeNode in="blur" /><feMergeNode in="SourceGraphic" /></feMerge>
    </filter>
  </defs>
  <g stroke="#15803d" stroke-width="1.5" fill="none" opacity="0.65">
    <rect x="40" y="40" width="944" height="944" rx="20" stroke="#16a34a" stroke-width="3" stroke-dasharray="8 6" />
    <rect x="55" y="55" width="914" height="914" rx="16" stroke="#22c55e" stroke-width="2" />
    
    <circle cx="100" cy="100" r="35" stroke="#15803d" stroke-width="2" />
    <circle cx="924" cy="100" r="35" stroke="#15803d" stroke-width="2" />
    <circle cx="100" cy="924" r="35" stroke="#15803d" stroke-width="2" />
    <circle cx="924" cy="924" r="35" stroke="#15803d" stroke-width="2" />
  </g>
  <g transform="translate(150, 240) rotate(-15)" filter="url(#goldStampGlow)" opacity="0.85">
    <circle cx="0" cy="0" r="70" stroke="#eab308" stroke-width="4" stroke-dasharray="6 4" fill="none" />
    <circle cx="0" cy="0" r="60" stroke="#eab308" stroke-width="2" fill="none" />
    <text x="0" y="-15" text-anchor="middle" fill="#eab308" font-family="Arial Black, sans-serif" font-size="16" font-weight="bold">DTEAM OFFICIAL</text>
    <text x="0" y="15" text-anchor="middle" fill="#fef08a" font-family="Impact, sans-serif" font-size="28">VERIFIED</text>
    <text x="0" y="38" text-anchor="middle" fill="#eab308" font-family="Arial Black, sans-serif" font-size="14">★ 100% LEGIT ★</text>
  </g>
  <g transform="translate(880, 280) rotate(15)" filter="url(#goldStampGlow)" opacity="0.85">
    <rect x="-70" y="-30" width="140" height="60" rx="10" stroke="#22c55e" stroke-width="4" fill="none" />
    <text x="0" y="10" text-anchor="middle" fill="#22c55e" font-family="Impact, sans-serif" font-size="32">MINTED</text>
  </g>
</svg>
`;

async function renderPatterns() {
  const list = [
    { svg: svg1, out: 'pat_1.png' },
    { svg: svg2, out: 'pat_2.png' },
    { svg: svg3, out: 'pat_3.png' },
    { svg: svg4, out: 'pat_4.png' },
    { svg: svg5, out: 'pat_5.png' },
  ];

  for (const item of list) {
    const outPath = path.join(layersDir, item.out);
    await sharp(Buffer.from(item.svg))
      .png()
      .toFile(outPath);
    console.log('Rendered pattern:', item.out);
  }
}

renderPatterns().then(() => console.log('All 5 patterns rendered successfully!'));
