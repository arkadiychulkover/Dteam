<script lang="ts">
  import Header from './lib/components/layout/Header.svelte';
  import Footer from './lib/components/layout/Footer.svelte';
  import FriendsSidebar from './lib/components/layout/FriendsSidebar.svelte';
  import StoreView from './lib/components/store/StoreView.svelte';
  import LibraryView from './lib/components/library/LibraryView.svelte';
  import CommunityView from './lib/components/community/CommunityView.svelte';
  import ProfileView from './lib/components/profile/ProfileView.svelte';
  import WalletModal from './lib/components/wallet/WalletModal.svelte';
  import PublishGameModal from './lib/components/developer/PublishGameModal.svelte';
  import ToastContainer from './lib/components/ui/ToastContainer.svelte';

  // Auth Components
  import LoginView from './lib/components/auth/LoginView.svelte';
  import RegisterView from './lib/components/auth/RegisterView.svelte';
  import ForgotPasswordView from './lib/components/auth/ForgotPasswordView.svelte';
  import ConfirmCodeView from './lib/components/auth/ConfirmCodeView.svelte';
  import ResetPasswordView from './lib/components/auth/ResetPasswordView.svelte';
  import LoginModal from './lib/components/auth/LoginModal.svelte';
  import ConfirmCodeModal from './lib/components/auth/ConfirmCodeModal.svelte';

  import { uiStore } from './lib/stores/uiStore';
</script>

<div class="min-h-screen flex flex-col bg-[#0a0c14] text-slate-100 font-sans selection:bg-cyan-500 selection:text-black">
  <!-- Top Navigation -->
  <Header />

  <!-- Main Content Body -->
  <main class="flex-1">
    {#if $uiStore.activeTab === 'store'}
      <StoreView />
    {:else if $uiStore.activeTab === 'library'}
      <LibraryView />
    {:else if $uiStore.activeTab === 'community'}
      <CommunityView />
    {:else if $uiStore.activeTab === 'profile'}
      <ProfileView />
    {:else if $uiStore.activeTab === 'login'}
      <LoginView />
    {:else if $uiStore.activeTab === 'register'}
      <RegisterView />
    {:else if $uiStore.activeTab === 'forgot-password'}
      <ForgotPasswordView />
    {:else if $uiStore.activeTab === 'confirm-code'}
      <ConfirmCodeView />
    {:else if $uiStore.activeTab === 'reset-password'}
      <ResetPasswordView />
    {/if}
  </main>

  <!-- Sidebars & Global Modals -->
  <FriendsSidebar />
  <WalletModal />
  <PublishGameModal />
  <LoginModal />
  <ConfirmCodeModal />
  <ToastContainer />

  <!-- Footer -->
  <Footer />
</div>
