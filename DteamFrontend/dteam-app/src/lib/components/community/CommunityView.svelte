<script lang="ts">
  import {
    Heart,
    MessageSquare,
    Share2,
    MoreHorizontal,
    Play,
    Search,
    ChevronLeft,
    Sparkles,
    Image,
    Video,
    MessageCircle,
    BookOpen,
    Newspaper
  } from 'lucide-svelte';

  let activeTab = $state<'subscriptions' | 'library' | 'recommended'>('recommended');
  let activeCategory = $state<string>('Усі розділи');
  let sortBy = $state<string>('Популярні');
  let searchQuery = $state('');

  const sampleCommunityPosts = [
    {
      id: 'cp-1',
      authorName: 'NikaNii',
      authorAvatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&auto=format&fit=crop&q=80',
      timestamp: '25.02.2024',
      title: 'Епічний світанок у відкритому космосі!',
      body: 'Сьогодні досліджували нову зоряну систему з командою у No Man\'s Sky. Удалося відшукати рідкісну планету з неоновою атмосферою.',
      mediaType: 'image',
      mediaUrl: 'https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?w=1000&auto=format&fit=crop&q=80',
      likes: '2.5k',
      comments: '2.5k',
    },
    {
      id: 'cp-2',
      authorName: 'CyberViper',
      authorAvatar: 'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=150&auto=format&fit=crop&q=80',
      timestamp: '24.02.2024',
      title: 'Спідран турнір — Фінальний забіг',
      body: 'Подивіться моменти з нашого турніру по Sekiro. Таймінги паррування просто божевільні!',
      mediaType: 'video',
      mediaUrl: 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=1000&auto=format&fit=crop&q=80',
      likes: '2.5k',
      comments: '2.5k',
    },
    {
      id: 'cp-3',
      authorName: 'AstraWalker',
      authorAvatar: 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150&auto=format&fit=crop&q=80',
      timestamp: '23.02.2024',
      title: 'Гайд по оптимізації середньовічної фортеці',
      body: 'Детальний розбір того, як правильно розподіляти видобуток ресурсів у Lords of the Manor.',
      mediaType: 'image',
      mediaUrl: 'https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=1000&auto=format&fit=crop&q=80',
      likes: '2.5k',
      comments: '2.5k',
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

<div class="min-h-[90vh] bg-[#070C12] text-[#F1F5F9] font-sans p-6 space-y-6 max-w-7xl mx-auto">
  
  <!-- 1. Top Navigation Tabs -->
  <div class="bg-[#0A1118] border border-white/5 rounded-xl p-3 flex flex-wrap items-center justify-between gap-4">
    <div class="flex items-center gap-4 text-xs font-bold">
      <!-- Back Arrow -->
      <button class="p-1.5 rounded-lg bg-[#101922] hover:bg-[#162330] text-[#94A3B8] hover:text-white transition-colors cursor-pointer border border-white/5">
        <ChevronLeft class="w-4 h-4" />
      </button>

      <button
        onclick={() => activeTab = 'subscriptions'}
        class="px-4 py-2 rounded-lg transition-all cursor-pointer {activeTab === 'subscriptions' ? 'bg-[#101922] text-cyan-400 border border-cyan-400/40 font-bold' : 'text-[#94A3B8] hover:text-white'}"
      >
        Підписки
      </button>

      <button
        onclick={() => activeTab = 'library'}
        class="px-4 py-2 rounded-lg transition-all cursor-pointer {activeTab === 'library' ? 'bg-[#101922] text-cyan-400 border border-cyan-400/40 font-bold' : 'text-[#94A3B8] hover:text-white'}"
      >
        З Бібліотеки
      </button>

      <button
        onclick={() => activeTab = 'recommended'}
        class="px-4 py-2 rounded-lg transition-all cursor-pointer {activeTab === 'recommended' ? 'bg-[#101922] text-cyan-400 border border-cyan-400/40 font-bold' : 'text-[#94A3B8] hover:text-white'}"
      >
        Рекомендоване
      </button>
    </div>
  </div>

  <!-- 2. Two-Column Layout (75% Left Feed / 25% Right Sidebar) -->
  <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
    
    <!-- Left Timeline Feed (75%) -->
    <div class="lg:col-span-8 space-y-6">
      {#each sampleCommunityPosts as post}
        <div class="bg-[#101922] border border-white/[0.06] rounded-xl p-6 space-y-4 shadow-xl hover:border-cyan-400/30 transition-all">
          
          <!-- User Header -->
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <img src={post.authorAvatar} alt={post.authorName} class="w-8 h-8 rounded-full object-cover ring-1 ring-white/10" />
              <div>
                <span class="block text-xs font-bold text-[#F1F5F9]">{post.authorName}</span>
                <span class="block text-[10px] text-[#64748B]">{post.timestamp}</span>
              </div>
            </div>
            <button class="text-slate-500 hover:text-slate-300 transition-colors cursor-pointer">
              <MoreHorizontal class="w-4 h-4" />
            </button>
          </div>

          <!-- Post Title & Content -->
          <div class="space-y-1">
            <h3 class="text-base font-bold text-[#F1F5F9]">{post.title}</h3>
            <p class="text-xs text-[#94A3B8] leading-relaxed">{post.body}</p>
          </div>

          <!-- Full-Width Embedded Media -->
          <div class="relative rounded-xl overflow-hidden border border-white/5 max-h-[400px] bg-slate-900 group">
            <img src={post.mediaUrl} alt="" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
            {#if post.mediaType === 'video'}
              <div class="absolute inset-0 flex items-center justify-center bg-black/40 backdrop-blur-[1px]">
                <div class="w-14 h-14 rounded-full bg-white text-black flex items-center justify-center shadow-xl group-hover:scale-110 transition-transform">
                  <Play class="w-6 h-6 fill-black ml-0.5" />
                </div>
              </div>
            {/if}
          </div>

          <!-- Interactive Footer -->
          <div class="pt-3 border-t border-white/5 flex items-center justify-between text-xs text-[#94A3B8]">
            <div class="flex items-center gap-6">
              <button class="flex items-center gap-2 text-rose-500 font-semibold hover:opacity-80 transition-opacity cursor-pointer">
                <Heart class="w-4 h-4 fill-rose-500 text-rose-500" /> {post.likes}
              </button>
              <button class="flex items-center gap-2 hover:text-white transition-colors cursor-pointer font-medium">
                <MessageSquare class="w-4 h-4" /> {post.comments}
              </button>
            </div>
            <button class="flex items-center gap-2 hover:text-white transition-colors cursor-pointer font-medium">
              <Share2 class="w-4 h-4" /> Поділитись
            </button>
          </div>

        </div>
      {/each}
    </div>

    <!-- Right Filter Sidebar (25% Panel: bg-[#0D151D] p-4 rounded-xl) -->
    <div class="lg:col-span-4 space-y-6">
      <div class="bg-[#0D151D] border border-white/[0.06] p-4 rounded-xl space-y-4 sticky top-24">
        
        <!-- Dropdown Selector -->
        <div class="space-y-1.5">
          <label for="community-sort" class="block text-[10px] font-bold uppercase tracking-wider text-[#64748B]">Сортування</label>
          <select
            id="community-sort"
            bind:value={sortBy}
            class="w-full bg-[#101922] text-xs font-bold text-cyan-400 border border-white/10 rounded-lg px-3 py-2 focus:outline-none focus:border-cyan-400 cursor-pointer"
          >
            <option value="Популярні">Сортування: Популярні</option>
            <option value="Нові">Сортування: Нові</option>
            <option value="Обговорювані">Сортування: Обговорювані</option>
          </select>
        </div>

        <!-- Search Box -->
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-500">
            <Search class="w-3.5 h-3.5" />
          </div>
          <input
            type="text"
            bind:value={searchQuery}
            placeholder="Пошук: Усі розділи"
            class="w-full pl-9 pr-3 py-2 bg-[#101922] border border-white/10 rounded-lg text-xs text-[#F1F5F9] placeholder-slate-500 focus:outline-none focus:border-cyan-400 transition-all"
          />
        </div>

        <!-- Category List Items -->
        <div class="space-y-1 pt-2">
          <span class="text-[10px] font-bold uppercase tracking-wider text-[#64748B] block px-1 mb-1">Розділи</span>
          {#each categories as cat}
            {@const Icon = cat.icon}
            <button
              onclick={() => activeCategory = cat.name}
              class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-medium transition-all cursor-pointer border
                {activeCategory === cat.name
                  ? 'bg-cyan-400 text-black border-cyan-400 font-bold'
                  : 'text-[#94A3B8] hover:bg-[#162330] border-transparent hover:text-[#F1F5F9]'}"
            >
              <Icon class="w-3.5 h-3.5" />
              <span>{cat.name}</span>
            </button>
          {/each}
        </div>

      </div>
    </div>

  </div>
</div>
