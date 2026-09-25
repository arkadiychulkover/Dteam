<script lang="ts">
  import { FileText, ArrowLeft, ShieldCheck, Scale, AlertCircle } from 'lucide-svelte';
  import { uiStore } from '../../stores/uiStore';
</script>

<div class="max-w-4xl mx-auto px-4 py-8 sm:py-12 space-y-8 animate-in fade-in duration-300">
  <div class="flex items-center justify-between pb-6 border-b border-cyan-500/20">
    <div class="flex items-center gap-3">
      <div class="p-3 rounded-2xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-400">
        <Scale class="w-6 h-6" />
      </div>
      <div>
        <h1 class="text-2xl sm:text-3xl font-black text-white font-display tracking-wide">
          Умови використання (Terms of Service)
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
        1. Загальні положення
      </h2>
      <p>
        Ласкаво просимо на платформу <strong>Dteam</strong> (надалі — «Платформа», «Сервіс»). Використовуючи наш сервіс, ви погоджуєтеся дотримуватися цих Умов використання. Якщо ви не згодні з будь-яким із пунктів, будь ласка, припиніть використання платформи.
      </p>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        2. Обліковий запис та безпека
      </h2>
      <p>
        Для доступу до цифрових покупок, чату, спільноти та бібліотеки ігор користувач зобов'язаний створити обліковий запис. Ви несете повну відповідальність за збереження конфіденційності своїх облікових даних і за будь-які дії, здійснені під вашим акаунтом.
      </p>
      <ul class="list-disc list-inside space-y-1 text-slate-400 pl-2">
        <li>Забороняється передача облікового запису третім особам.</li>
        <li>Забороняється використання автоматизованих скриптів та ботів без узгодження.</li>
      </ul>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        3. Цифровий контент та транзакції
      </h2>
      <p>
        Усі транзакції на платформі здійснюються з використанням захищених цифрових активів та технології Web3 (включаючи мережу TON). Після підтвердження транзакції в блокчейні цифровий контент негайно стає доступним у вашій бібліотеці.
      </p>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        4. Правила поведінки у спільноті та чаті
      </h2>
      <p>
        Користувачі зобов'язуються дотримуватися взаємної поваги. На платформі суворо заборонено:
      </p>
      <ul class="list-disc list-inside space-y-1 text-slate-400 pl-2">
        <li>Публікацію шкідливого програмного забезпечення, чітів та вірусів.</li>
        <li>Спам, образи, розпалювання ворожнечі та дискримінацію.</li>
        <li>Порушення авторських прав розробників цифрових ігор.</li>
      </ul>
    </section>

    <section class="space-y-2">
      <h2 class="text-lg font-black text-white flex items-center gap-2 text-cyan-300">
        5. Зміни до умов
      </h2>
      <p>
        Адміністрація Dteam залишає за собою право вносити зміни до цих Умов у будь-який час. Продовжуючи використовувати платформу після внесення змін, ви підтверджуєте свою згоду з оновленими умовами.
      </p>
    </section>
  </div>
</div>
