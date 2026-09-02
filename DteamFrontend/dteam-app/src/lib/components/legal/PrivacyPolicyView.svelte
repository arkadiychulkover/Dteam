<script lang="ts">
  import { Shield, ArrowLeft, Lock, Eye, Database } from 'lucide-svelte';
  import { uiStore } from '../../stores/uiStore';
</script>

<div class="max-w-4xl mx-auto px-4 py-8 sm:py-12 space-y-8 animate-in fade-in duration-300">
  <div class="flex items-center justify-between pb-6 border-b border-cyan-500/20">
    <div class="flex items-center gap-3">
      <div class="p-3 rounded-2xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-400">
        <Shield class="w-6 h-6" />
      </div>
      <div>
        <h1 class="text-2xl sm:text-3xl font-black text-white font-display tracking-wide">
          Політика конфіденційності (Privacy Policy)
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
        1. Збір інформації
      </h2>
      <p>
        Ми збираємо мінімально необхідну інформацію для забезпечення стабільної роботи платформи, зокрема:
      </p>
      <ul class="list-disc list-inside space-y-1 text-slate-400 pl-2">
        <li>Ім'я користувача (Username), адреса електронної пошти.</li>
        <li>Публічні адреси гаманців для підтвердження блокчейн-транзакцій.</li>
        <li>Історія завантажень, активність у спільноті та повідомлення в чаті.</li>
      </ul>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        2. Використання даних
      </h2>
      <p>
        Зібрані дані використовуються виключно для:
      </p>
      <ul class="list-disc list-inside space-y-1 text-slate-400 pl-2">
        <li>Надання доступу до придбаних ігор та оновлень.</li>
        <li>Захисту облікового запису та запобігання шахрайству.</li>
        <li>Роботи системи друзів, повідомлень та стрічки активності.</li>
      </ul>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        3. Захист та шифрування
      </h2>
      <p>
        Усі паролі зберігаються в захешованому вигляді із застосуванням криптографічних солей. Сесійні токени захищені протоколом JWT і передаються виключно через захищене з'єднання HTTPS / WSS.
      </p>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        4. Передача третім особам
      </h2>
      <p>
        Dteam не продає та не передає ваші персональні дані третім сторонам або рекламним брокерам. Дані можуть бути розкриті лише відповідно до чинного законодавства.
      </p>
    </section>
  </div>
</div>
