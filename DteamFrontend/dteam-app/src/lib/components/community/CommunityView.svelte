<script lang="ts">
  import {
    Heart,
    MessageSquare,
    Share2,
    MoreHorizontal,
    Play,
    Search,
    SlidersHorizontal,
    Sparkles,
    Image,
    Video,
    MessageCircle,
    BookOpen,
    Newspaper,
    Filter
  } from 'lucide-svelte';

  // Sub-Navigation Tabs
  let activeTab = $state<'subscriptions' | 'library' | 'recommended'>('recommended');
  let activeCategory = $state<string>('Усі розділи');
  let sortBy = $state<string>('Популярні');
  let searchQuery = $state('');

  const sampleCommunityPosts = [
    {
      id: 'cp-1',
      authorName: 'NikaNii',
      authorAvatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&auto=format&fit=crop&q=80',
      timestamp: '2 години тому',
      title: 'Епічний світанок у відкритому космосі!',
      body: 'Сьогодні досліджували нову зоряну систему з командою. Удалося відшукати рідкісну планету з атмосферою неону.',
      mediaType: 'image',
      mediaUrl: 'https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=1000&auto=format&fit=crop&q=80',
      likes: 2500,
      comments: 310,
    },
    {
      id: 'cp-2',
      authorName: 'CyberViper',
      authorAvatar: 'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=150&auto=format&fit=crop&q=80',
      timestamp: '5 годин тому',
      title: 'Спідран турнір — Фінальний забіг',
      body: 'Подивіться моменти з нашого турніру по кіберпанк арені. Таймінги на 02:45 просто божевільні!',
      mediaType: 'video',
      mediaUrl: 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=1000&auto=format&fit=crop&q=80',
      likes: 4200,
      comments: 580,
    },
    {
      id: 'cp-3',
      authorName: 'AstraWalker',
      authorAvatar: 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150&auto=format&fit=crop&q=80',
      timestamp: '1 день тому',
      title: 'Гайд по оптимізації бази Neocity',
      body: 'Детальний розбір того, як правильно розподіляти енергетичні вузли у вашому поселенні.',
      mediaType: 'image',
      mediaUrl: 'https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=1000&auto=format&fit=crop&q=80',
      likes: 1800,
      comments: 140,
    },
  ];

  const categories = [
    { name: 'Усі розділи', icon: Sparkles },
    { name: 'Форум', icon: MessageCircle },
    { name: 'Скріншоти', icon: Image },
    { name: 'Відео', icon: Video },
    { name: 'Гайди', icon: BookOpen },
    { name: 'Новини', icon: Newspaper },
  ];
</script>

<div class="min-h-[90vh] bg-[#0A0D14] text-slate-200 font-sans p-4 sm:p-6 lg:p-8 space-y-6 max-w-7xl mx-auto">
  <!-- Top Sub-Navigation Bar -->
  <div class="bg-[#111C24] border border-white/10 rounded-2xl p-4 flex flex-wrap items-center justify-between gap-4">
    <div class="flex items-center gap-6 text-sm font-bold">
      <button
        onclick={() => activeTab = 'subscriptions'}
        class="transition-colors cursor-pointer pb-1 relative {activeTab === 'subscriptions' ? 'text-cyan-400 font-black' : 'text-slate-400 hover:text-white'}"
      >
        Підписки
        {#if activeTab === 'subscriptions'}
          <span class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>
        {/if}
      </button>

      <button
        onclick={() => activeTab = 'library'}
        class="transition-colors cursor-pointer pb-1 relative {activeTab === 'library' ? 'text-cyan-400 font-black' : 'text-slate-400 hover:text-white'}"
      >
        З Бібліотеки
        {#if activeTab === 'library'}
          <span class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>
        {/if}
      </button>

      <button
        onclick={() => activeTab = 'recommended'}
        class="transition-colors cursor-pointer pb-1 relative {activeTab === 'recommended' ? 'text-cyan-400 font-black' : 'text-slate-400 hover:text-white'}"
      >
        Рекомендоване
        {#if activeTab === 'recommended'}
          <span class="absolute bottom-0 left-0 right-0 h-0.5 bg-cyan-400 rounded-full"></span>
        {/if}
      </button>
    </div>

    <!-- Sorting Dropdown -->
    <div class="flex items-center gap-2">
      <span class="text-xs text-slate-400">Сортування:</span>
      <select
        bind:value={sortBy}
        class="bg-[#16222F] text-xs font-bold text-cyan-400 border border-white/10 rounded-xl px-3 py-1.5 focus:outline-none focus:border-cyan-400 cursor-pointer"
      >
        <option value="Популярні">Популярні</option>
        <option value="Нові">Нові</option>
        <option value="Обговорювані">Обговорювані</option>
      </select>
    </div>
  </div>

  <!-- Main Feed & Right Sidebar Grid -->
  <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
    <!-- Main Feed Area (Left 75%) -->
    <div class="lg:col-span-8 space-y-6">
      {#each sampleCommunityPosts as post}
        <div class="bg-[#111C24] border border-white/10 rounded-3xl p-6 space-y-4 shadow-xl hover:border-cyan-500/30 transition-all">
          <!-- User Header -->
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <img src={post.authorAvatar} alt={post.authorName} class="w-10 h-10 rounded-full object-cover ring-2 ring-cyan-500/30" />
              <div>
                <span class="block text-sm font-bold text-white">{post.authorName}</span>
                <span class="block text-[11px] text-slate-500">{post.timestamp}</span>
              </div>
            </div>
            <button class="text-slate-500 hover:text-slate-300 transition-colors cursor-pointer">
              <MoreHorizontal class="w-5 h-5" />
            </button>
          </div>

          <!-- Title & Body -->
          <div class="space-y-1.5">
            <h3 class="text-base sm:text-lg font-bold text-white">{post.title}</h3>
            <p class="text-xs sm:text-sm text-slate-300 leading-relaxed">{post.body}</p>
          </div>

          <!-- Embedded Media -->
          <div class="relative rounded-2xl overflow-hidden border border-white/10 max-h-[420px] bg-slate-900 group">
            <img src={post.mediaUrl} alt="" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
            {#if post.mediaType === 'video'}
              <div class="absolute inset-0 flex items-center justify-center bg-black/40 backdrop-blur-[2px]">
                <div class="w-16 h-16 rounded-full bg-cyan-400 text-black flex items-center justify-center shadow-xl shadow-cyan-400/50 group-hover:scale-110 transition-transform">
                  <Play class="w-7 h-7 fill-black ml-1" />
                </div>
              </div>
            {/if}
          </div>

          <!-- Footer Stats & Actions -->
          <div class="pt-4 border-t border-white/5 flex items-center justify-between text-xs text-slate-400">
            <div class="flex items-center gap-6">
              <button class="flex items-center gap-2 hover:text-cyan-400 transition-colors cursor-pointer font-semibold">
                <Heart class="w-4 h-4 text-rose-400 fill-rose-400" /> {(post.likes / 1000).toFixed(1)}k
              </button>
              <button class="flex items-center gap-2 hover:text-cyan-400 transition-colors cursor-pointer font-semibold">
                <MessageSquare class="w-4 h-4" /> {post.comments}
              </button>
            </div>
            <button class="flex items-center gap-2 hover:text-white transition-colors cursor-pointer font-semibold">
              <Share2 class="w-4 h-4" /> Поділитись
            </button>
          </div>
        </div>
      {/each}
    </div>

    <!-- Right Filter Sidebar (Right 25%) -->
    <div class="lg:col-span-4 space-y-6">
      <div class="bg-[#111C24] border border-white/10 rounded-3xl p-5 space-y-5 sticky top-36">
        <!-- Search Input -->
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
            <Search class="w-4 h-4" />
          </div>
          <input
            type="text"
            bind:value={searchQuery}
            placeholder="Пошук: Усі розділи"
            class="w-full pl-10 pr-4 py-2.5 bg-[#0a0d14] border border-white/10 rounded-xl text-xs text-white placeholder-slate-500 focus:outline-none focus:border-cyan-400 transition-all"
          />
        </div>

        <!-- Categories List -->
        <div class="space-y-1">
          <span class="text-[11px] font-extrabold uppercase tracking-wider text-slate-400 block px-2 mb-2">Категорії</span>
          {#each categories as cat}
            {@const Icon = cat.icon}
            <button
              onclick={() => activeCategory = cat.name}
              class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-semibold transition-all cursor-pointer border
                {activeCategory === cat.name
                  ? 'bg-cyan-500/20 text-cyan-400 border-cyan-500/40 font-bold'
                  : 'text-slate-400 hover:bg-slate-800/60 border-transparent hover:text-slate-200'}"
            >
              <Icon class="w-4 h-4" />
              <span>{cat.name}</span>
            </button>
          {/each}
        </div>
      </div>
    </div>
  </div>
</div>
