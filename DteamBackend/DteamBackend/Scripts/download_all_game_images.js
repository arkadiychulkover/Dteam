const fs = require('fs');
const path = require('path');

const targetDirs = [
  path.join(__dirname, 'wwwroot', 'game_images'),
  path.join(__dirname, 'bin', 'Debug', 'net8.0', 'wwwroot', 'game_images')
];

for (const dir of targetDirs) {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

async function fetchBufferWithFallback(urls) {
  for (const url of urls) {
    if (!url) continue;
    try {
      const res = await fetch(url, { headers: { 'User-Agent': 'Mozilla/5.0' } });
      if (res.ok) {
        const buf = Buffer.from(await res.arrayBuffer());
        if (buf.length > 1000) {
          return { buffer: buf, url };
        }
      }
    } catch (e) {
    }
  }
  return null;
}

function saveFile(filename, buffer) {
  for (const dir of targetDirs) {
    const filePath = path.join(dir, filename);
    fs.writeFileSync(filePath, buffer);
  }
}

const mainGames = [
  {
    title: 'Cyberpunk 2077',
    slug: 'cyberpunk-2077',
    appId: 1091500
  },
  {
    title: 'The Witcher 3: Wild Hunt',
    slug: 'witcher-3',
    appId: 292030
  },
  {
    title: 'Elden Ring',
    slug: 'elden-ring',
    appId: 1245620
  },
  {
    title: 'DOOM Eternal',
    slug: 'doom-eternal',
    appId: 782330
  },
  {
    title: "Sid Meier's Civilization VI",
    slug: 'civ-6',
    appId: 289070
  },
  {
    title: 'Forza Horizon 5',
    slug: 'forza-horizon-5',
    appId: 1551360
  },
  {
    title: 'Resident Evil 4 Remake',
    slug: 'resident-evil-4',
    appId: 2050650
  },
  {
    title: "Baldur's Gate 3",
    slug: 'baldurs-gate-3',
    appId: 1086940
  },
  {
    title: 'Dead Cells',
    slug: 'dead-cells',
    appId: 588650
  },
  {
    title: 'Subnautica',
    slug: 'subnautica',
    appId: 264710
  }
];

const dlcs = [
  {
    title: 'Cyberpunk 2077: Phantom Liberty',
    slug: 'cyberpunk-phantom-liberty',
    appId: 2138330,
    parentSlug: 'cyberpunk-2077'
  },
  {
    title: 'Cyberpunk 2077: REDmod',
    slug: 'cyberpunk-redmod',
    appId: 2060310,
    parentSlug: 'cyberpunk-2077'
  },
  {
    title: 'The Witcher 3: Hearts of Stone',
    slug: 'witcher-3-hos',
    appId: 378648,
    parentSlug: 'witcher-3'
  },
  {
    title: 'The Witcher 3: Blood and Wine',
    slug: 'witcher-3-baw',
    appId: 378649,
    parentSlug: 'witcher-3'
  },
  {
    title: 'Elden Ring: Shadow of the Erdtree',
    slug: 'elden-ring-erdtree',
    appId: 2778580,
    parentSlug: 'elden-ring'
  },
  {
    title: 'Elden Ring: Colosseum & Soundtrack',
    slug: 'elden-ring-colosseum',
    appId: 1245620,
    parentSlug: 'elden-ring'
  },
  {
    title: 'DOOM Eternal: The Ancient Gods - Part One',
    slug: 'doom-ancient-gods-1',
    appId: 1385200,
    parentSlug: 'doom-eternal'
  },
  {
    title: 'DOOM Eternal: The Ancient Gods - Part Two',
    slug: 'doom-ancient-gods-2',
    appId: 1477590,
    parentSlug: 'doom-eternal'
  },
  {
    title: 'Civilization VI: Gathering Storm',
    slug: 'civ-6-gathering-storm',
    appId: 947510,
    parentSlug: 'civ-6'
  },
  {
    title: 'Civilization VI: Rise and Fall',
    slug: 'civ-6-rise-and-fall',
    appId: 645402,
    parentSlug: 'civ-6'
  },
  {
    title: 'Forza Horizon 5: Hot Wheels',
    slug: 'forza-horizon-5-hot-wheels',
    appId: 1613280,
    parentSlug: 'forza-horizon-5'
  },
  {
    title: 'Forza Horizon 5: Rally Adventure',
    slug: 'forza-horizon-5-rally',
    appId: 1613281,
    parentSlug: 'forza-horizon-5'
  },
  {
    title: 'Resident Evil 4: Separate Ways',
    slug: 're4-separate-ways',
    appId: 2109315,
    parentSlug: 'resident-evil-4'
  },
  {
    title: 'Resident Evil 4: Extra DLC Pack',
    slug: 're4-extra-dlc',
    appId: 2109300,
    parentSlug: 'resident-evil-4'
  },
  {
    title: "Baldur's Gate 3: Digital Deluxe Upgrade",
    slug: 'bg3-deluxe',
    appId: 2378500,
    parentSlug: 'baldurs-gate-3'
  },
  {
    title: "Baldur's Gate 3: Adventure Companion Pack",
    slug: 'bg3-companion',
    appId: 2378500,
    parentSlug: 'baldurs-gate-3'
  },
  {
    title: 'Dead Cells: Return to Castlevania',
    slug: 'dead-cells-castlevania',
    appId: 1909750,
    parentSlug: 'dead-cells'
  },
  {
    title: 'Dead Cells: The Bad Seed',
    slug: 'dead-cells-bad-seed',
    appId: 1204130,
    parentSlug: 'dead-cells'
  },
  {
    title: 'Subnautica: Below Zero',
    slug: 'subnautica-below-zero',
    appId: 848450,
    parentSlug: 'subnautica'
  },
  {
    title: 'Subnautica: Deep Sea Pack',
    slug: 'subnautica-deep-sea',
    appId: 264710,
    parentSlug: 'subnautica'
  }
];

async function processMainGame(g) {
  console.log(`\nProcessing Main Game: ${g.title} (${g.slug}, appId ${g.appId})...`);
  
  let screenshots = [];
  let storeHeader = null;
  try {
    const detailsRes = await fetch(`https://store.steampowered.com/api/appdetails?appids=${g.appId}`);
    if (detailsRes.ok) {
      const json = await detailsRes.json();
      if (json[g.appId] && json[g.appId].data) {
        const data = json[g.appId].data;
        storeHeader = data.header_image;
        if (data.screenshots && data.screenshots.length > 0) {
          screenshots = data.screenshots.map(s => s.path_full);
        }
      }
    }
  } catch (e) {
    console.warn(`  Failed to get store details for ${g.appId}:`, e.message);
  }

  const coverUrls = [
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${g.appId}/library_600x900.jpg`,
    `https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/${g.appId}/library_600x900.jpg`,
    `https://steamcdn-a.akamaihd.net/steam/apps/${g.appId}/library_600x900.jpg`,
    storeHeader
  ];
  const coverRes = await fetchBufferWithFallback(coverUrls);
  if (coverRes) {
    saveFile(`${g.slug}_cover.jpg`, coverRes.buffer);
    console.log(`  ✓ Cover saved (${coverRes.buffer.length} bytes)`);
  } else {
    console.error(`  ✗ FAILED to get cover for ${g.title}`);
  }

  const headerUrls = [
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${g.appId}/library_hero.jpg`,
    `https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/${g.appId}/library_hero.jpg`,
    storeHeader,
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${g.appId}/header.jpg`
  ];
  const headerRes = await fetchBufferWithFallback(headerUrls);
  if (headerRes) {
    saveFile(`${g.slug}_header.jpg`, headerRes.buffer);
    console.log(`  ✓ Header saved (${headerRes.buffer.length} bytes)`);
  } else {
    console.error(`  ✗ FAILED to get header for ${g.title}`);
  }

  for (let i = 0; i < 3; i++) {
    const ssUrl = screenshots[i] || (screenshots.length > 0 ? screenshots[0] : null);
    if (ssUrl) {
      const ssRes = await fetchBufferWithFallback([ssUrl]);
      if (ssRes) {
        saveFile(`${g.slug}_ss${i + 1}.jpg`, ssRes.buffer);
        console.log(`  ✓ Screenshot ${i + 1} saved (${ssRes.buffer.length} bytes)`);
        continue;
      }
    }
    if (headerRes) {
      saveFile(`${g.slug}_ss${i + 1}.jpg`, headerRes.buffer);
      console.log(`  ✓ Screenshot ${i + 1} copied from header`);
    }
  }
}

async function processDlc(d) {
  console.log(`\nProcessing DLC: ${d.title} (${d.slug}, appId ${d.appId})...`);

  let dlcScreenshots = [];
  let dlcStoreHeader = null;
  try {
    const detailsRes = await fetch(`https://store.steampowered.com/api/appdetails?appids=${d.appId}`);
    if (detailsRes.ok) {
      const json = await detailsRes.json();
      if (json[d.appId] && json[d.appId].data) {
        const data = json[d.appId].data;
        dlcStoreHeader = data.header_image;
        if (data.screenshots && data.screenshots.length > 0) {
          dlcScreenshots = data.screenshots.map(s => s.path_full);
        }
      }
    }
  } catch (e) {}

  const headerUrls = [
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${d.appId}/header.jpg`,
    `https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/${d.appId}/header.jpg`,
    dlcStoreHeader,
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${d.appId}/capsule_616x353.jpg`,
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${d.appId}/library_hero.jpg`
  ];
  let headerRes = await fetchBufferWithFallback(headerUrls);
  if (!headerRes) {
    const parentHeaderPath = path.join(targetDirs[0], `${d.parentSlug}_header.jpg`);
    if (fs.existsSync(parentHeaderPath)) {
      headerRes = { buffer: fs.readFileSync(parentHeaderPath) };
    }
  }
  if (headerRes) {
    saveFile(`${d.slug}_header.jpg`, headerRes.buffer);
    console.log(`  ✓ DLC Header saved (${headerRes.buffer.length} bytes)`);
  }

  const coverUrls = [
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${d.appId}/library_600x900.jpg`,
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${d.appId}/capsule_616x353.jpg`,
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${d.appId}/header.jpg`,
    dlcStoreHeader
  ];
  let coverRes = await fetchBufferWithFallback(coverUrls);
  if (!coverRes) {
    if (headerRes) {
      coverRes = headerRes;
    } else {
      const parentCoverPath = path.join(targetDirs[0], `${d.parentSlug}_cover.jpg`);
      if (fs.existsSync(parentCoverPath)) {
        coverRes = { buffer: fs.readFileSync(parentCoverPath) };
      }
    }
  }
  if (coverRes) {
    saveFile(`${d.slug}_cover.jpg`, coverRes.buffer);
    console.log(`  ✓ DLC Cover saved (${coverRes.buffer.length} bytes)`);
  }

  const ssUrls = [
    ...(dlcScreenshots.length > 0 ? [dlcScreenshots[0]] : []),
    `https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/${d.appId}/ss_1.jpg`
  ];
  let ssRes = await fetchBufferWithFallback(ssUrls);
  if (!ssRes && headerRes) {
    ssRes = headerRes;
  }
  if (ssRes) {
    saveFile(`${d.slug}_ss1.jpg`, ssRes.buffer);
    console.log(`  ✓ DLC Screenshot saved (${ssRes.buffer.length} bytes)`);
  }
}

async function main() {
  console.log('=== STARTING DOWNLOAD OF OFFICIAL GAME IMAGES ===');
  for (const g of mainGames) {
    await processMainGame(g);
  }
  for (const d of dlcs) {
    await processDlc(d);
  }
  console.log('\n=== ALL GAME ASSETS SUCCESSFULLY DOWNLOADED ===');
}

main().catch(console.error);
