<script lang="ts">
  import { settingsStore } from '../../../stores/settingsStore';
  import SettingsPageHeader from '../primitives/SettingsPageHeader.svelte';
  import SettingsSection from '../primitives/SettingsSection.svelte';
  import SettingsRow from '../primitives/SettingsRow.svelte';
  import SettingsToggle from '../primitives/SettingsToggle.svelte';
  import type { NotificationPreferences } from '../../../types/settings';

  const { serverState } = settingsStore;

  let prefs = $state<NotificationPreferences>({
    notifyBigSales: true,
    notifyWishlistDiscounts: true,
    notifyProfileComments: true,
    notifyFriendRequests: true,
    notifyFriendRequestAccepted: true,
    notifyFriendRequestDeclined: true,
    chatNotificationsEnabled: true,
    chatSoundEnabled: true,
  });

  $effect(() => {
    if ($serverState.preferences) {
      prefs = { ...$serverState.preferences };
    }
  });

  function updatePreference(key: keyof NotificationPreferences, val: boolean) {
    prefs[key] = val;
    settingsStore.saveNotificationPreferences(prefs);
  }
</script>

<div class="w-full">
  <SettingsPageHeader
    title="Сповіщення"
    subtitle="Оберіть, які події платформи надсилатимуть вам повідомлення та звукові сигнали."
  />

  <SettingsSection title="Беззвучні сповіщення">
    <SettingsRow title="Великий розпродаж" description="Повідомлення про сезонні та святкові знижки в крамниці Dteam.">
      <SettingsToggle
        checked={prefs.notifyBigSales}
        onchange={(val) => updatePreference('notifyBigSales', val)}
        ariaLabel="Великий розпродаж"
      />
    </SettingsRow>

    <SettingsRow title="Знижка на ігри з мого Бажаного" description="Сповіщення, коли на гру зі списку бажаного з'являється знижка.">
      <SettingsToggle
        checked={prefs.notifyWishlistDiscounts}
        onchange={(val) => updatePreference('notifyWishlistDiscounts', val)}
        ariaLabel="Знижка на ігри з мого Бажаного"
      />
    </SettingsRow>

    <SettingsRow title="Новий коментар під моїм профілем" description="Хтось залишив запис або відгук на вашій сторінці.">
      <SettingsToggle
        checked={prefs.notifyProfileComments}
        onchange={(val) => updatePreference('notifyProfileComments', val)}
        ariaLabel="Новий коментар під моїм профілем"
      />
    </SettingsRow>

    <SettingsRow title="Новий запит на дружбу" description="Вам надійшов новий запит на додавання в друзі.">
      <SettingsToggle
        checked={prefs.notifyFriendRequests}
        onchange={(val) => updatePreference('notifyFriendRequests', val)}
        ariaLabel="Новий запит на дружбу"
      />
    </SettingsRow>

    <SettingsRow title="Мій запит на дружбу прийнято" description="Користувач підтвердив вашу заявку в друзі.">
      <SettingsToggle
        checked={prefs.notifyFriendRequestAccepted}
        onchange={(val) => updatePreference('notifyFriendRequestAccepted', val)}
        ariaLabel="Мій запит на дружбу прийнято"
      />
    </SettingsRow>

    <SettingsRow title="Мій запит на дружбу відхилено" description="Користувач відхилив вашу заявку в друзі.">
      <SettingsToggle
        checked={prefs.notifyFriendRequestDeclined}
        onchange={(val) => updatePreference('notifyFriendRequestDeclined', val)}
        ariaLabel="Мій запит на дружбу відхилено"
      />
    </SettingsRow>
  </SettingsSection>

  <SettingsSection title="Чат">
    <div class="flex items-center justify-between px-3 pb-1 text-[11px] font-mono uppercase tracking-wider text-slate-400">
      <span>Подія</span>
      <div class="flex items-center gap-6 pr-1">
        <span class="w-11 text-center">Сповіщення</span>
        <span class="w-11 text-center">Звук</span>
      </div>
    </div>

    <div class="flex items-center justify-between gap-4 py-2.5 px-3 rounded-xl hover:bg-cyan-950/30 transition-colors">
      <div class="flex-1 min-w-0 pr-4">
        <div class="text-sm font-semibold text-slate-200">Нове повідомлення у чаті</div>
        <div class="text-xs text-slate-400 mt-0.5">Спливаюче повідомлення та звук при отриманні вхідного листа</div>
      </div>
      <div class="flex items-center gap-6 shrink-0">
        <SettingsToggle
          checked={prefs.chatNotificationsEnabled}
          onchange={(val) => updatePreference('chatNotificationsEnabled', val)}
          ariaLabel="Сповіщення чату"
        />
        <SettingsToggle
          checked={prefs.chatSoundEnabled}
          onchange={(val) => updatePreference('chatSoundEnabled', val)}
          ariaLabel="Звук чату"
        />
      </div>
    </div>
  </SettingsSection>
</div>
