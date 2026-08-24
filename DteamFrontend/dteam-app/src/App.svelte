<script lang="ts">
  import { onMount } from 'svelte';
  import Header from './lib/components/layout/Header.svelte';
  import Footer from './lib/components/layout/Footer.svelte';
  import StoreView from './lib/components/store/StoreView.svelte';
  import CatalogView from './lib/components/store/CatalogView.svelte';
  import WishlistView from './lib/components/wishlist/WishlistView.svelte';
  import AdminView from './lib/components/admin/AdminView.svelte';
  import BannedView from './lib/components/banned/BannedView.svelte';
  import ToastContainer from './lib/components/ui/ToastContainer.svelte';
  import LiveBackground from './lib/components/ui/LiveBackground.svelte';
  import { uiStore } from './lib/stores/uiStore';
  import { authStore, currentUser } from './lib/stores/authStore';
  import { wishlistStore } from './lib/stores/wishlistStore';
  import { userService } from './lib/services/userService';

  let isBanned = $state(false);

  async function checkUserBanStatus() {
    if (!$currentUser?.id) return;
    try {
      const res = await userService.checkIsBanned($currentUser.id);
      isBanned = res.isBanned;
      if (res.isBanned && $currentUser) {
        authStore.setUser({ ...$currentUser, isBanned: true });
      }
    } catch (e) {
      console.warn('[App] Could not check ban status:', e);
    }
  }

  onMount(() => {
    checkUserBanStatus();
    wishlistStore.loadWishlist();
    const interval = setInterval(checkUserBanStatus, 5000);
    return () => clearInterval(interval);
  });
</script>

<div class="min-h-screen flex flex-col text-slate-100 font-sans selection:bg-cyan-400 selection:text-black relative">
  <LiveBackground />

  {#if !isBanned}
    <Header />
  {/if}

  <main class="flex-1 relative z-10">
    {#if isBanned}
      <BannedView onRetry={checkUserBanStatus} />
    {:else if $uiStore.activeTab === 'store'}
      <StoreView />
    {:else if $uiStore.activeTab === 'catalog'}
      <CatalogView />
    {:else if $uiStore.activeTab === 'wishlist'}
      <WishlistView />
    {:else if $uiStore.activeTab === 'admin'}
      <AdminView />
    {/if}
  </main>

  <ToastContainer />

  {#if !isBanned}
    <Footer />
  {/if}
</div>
