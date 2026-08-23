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
    {/if}
  </main>

  <!-- Sidebars & Global Modals -->
  <FriendsSidebar />
  <WalletModal />
  <PublishGameModal />
  <ToastContainer />

  <!-- Footer -->
  <Footer />
</div>
