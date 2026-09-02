<script lang="ts">
  import { RefreshCw, ArrowLeft, CheckCircle2, AlertTriangle, HelpCircle } from 'lucide-svelte';
  import { uiStore } from '../../stores/uiStore';
</script>

<div class="max-w-4xl mx-auto px-4 py-8 sm:py-12 space-y-8 animate-in fade-in duration-300">
  <div class="flex items-center justify-between pb-6 border-b border-cyan-500/20">
    <div class="flex items-center gap-3">
      <div class="p-3 rounded-2xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-400">
        <RefreshCw class="w-6 h-6" />
      </div>
      <div>
        <h1 class="text-2xl sm:text-3xl font-black text-white font-display tracking-wide">
          Політика повернення коштів (Refund Policy)
        </h1>
        <p class="text-xs text-slate-400 mt-1">Останнє оновлення: 1 вересня 2026 року</p>
      </div>
    </div>

    <button
      onclick={() => uiStore.setTab('store')}
      class="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-[#061820] hover:bg-cyan-950/60 border border-cyan-500/30 text-xs font-bold text-cyan-300 transition-all cursor-pointer"
    >
      <ArrowLeft class="w-4 h-4" />
      <span>Назад до крамниці</span>
    </button>
  </div>

  <div class="bg-[#061820]/90 backdrop-blur-xl border border-cyan-500/20 rounded-3xl p-6 sm:p-8 shadow-2xl space-y-6 text-sm text-slate-300 leading-relaxed">
    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        1. Умови повернення цифрових ігор
      </h2>
      <p>
        Ви можете запросити повернення коштів за будь-яку цифрову гру, придбану на Dteam, за дотримання таких базових умов:
      </p>
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2">
        <div class="p-4 rounded-2xl bg-[#030d12] border border-cyan-500/20 space-y-1.5">
          <div class="flex items-center gap-2 text-emerald-400 font-bold text-xs">
            <CheckCircle2 class="w-4 h-4" />
            <span>Термін запиту: до 14 днів</span>
          </div>
          <p class="text-xs text-slate-400">Запит надіслано не пізніше ніж через 14 днів після моменту покупки.</p>
        </div>

        <div class="p-4 rounded-2xl bg-[#030d12] border border-cyan-500/20 space-y-1.5">
          <div class="flex items-center gap-2 text-emerald-400 font-bold text-xs">
            <CheckCircle2 class="w-4 h-4" />
            <span>Час у грі: менше 2 годин</span>
          </div>
          <p class="text-xs text-slate-400">Загальний час запуску гри не перевищує 2 годин відповідно до статистики бібліотеки.</p>
        </div>
      </div>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        2. Доповнення (DLC) та внутрішньоігрові предмети
      </h2>
      <p>
        Повернення за DLC можливе протягом 14 днів за умови, що базова гра запускалася менше 2 годин після покупки доповнення, і доповнення не було використано або змінено.
      </p>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        3. Спосіб повернення
      </h2>
      <p>
        Кошти повертаються на внутрішній баланс акаунту Dteam або безпосередньо у вигляді TON на криптогаманець користувача після схвалення заявки службою підтримки.
      </p>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        4. Як подати запит
      </h2>
      <p>
        Для подання запиту на повернення коштів перейдіть у налаштування гри в бібліотеці або зверніться до служби підтримки платформи через розділ спільноти.
      </p>
    </section>
  </div>
</div>
