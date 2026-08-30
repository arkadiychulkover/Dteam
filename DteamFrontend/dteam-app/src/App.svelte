<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import Header from './lib/components/layout/Header.svelte';
  import Footer from './lib/components/layout/Footer.svelte';
  import StoreView from './lib/components/store/StoreView.svelte';
  import CatalogView from './lib/components/store/CatalogView.svelte';
  import GameDetailsView from './lib/components/store/GameDetailsView.svelte';
  import WishlistView from './lib/components/wishlist/WishlistView.svelte';
  import CartView from './lib/components/cart/CartView.svelte';
  import AdminView from './lib/components/admin/AdminView.svelte';
  import BannedView from './lib/components/banned/BannedView.svelte';
  import ToastContainer from './lib/components/ui/ToastContainer.svelte';
  import LiveBackground from './lib/components/ui/LiveBackground.svelte';
  import LibraryView from './lib/components/library/LibraryView.svelte';
  import CommunityView from './lib/components/community/CommunityView.svelte';
  import FriendsView from './lib/components/friends/FriendsView.svelte';
  import PublicProfileView from './lib/components/profile/PublicProfileView.svelte';
  import MyProfileView from './lib/components/profile/MyProfileView.svelte';
  import DeveloperView from './lib/components/developer/DeveloperView.svelte';
  import PublishGameModal from './lib/components/developer/PublishGameModal.svelte';
  import EditDeveloperGameModal from './lib/components/developer/EditDeveloperGameModal.svelte';

  import LoginView from './lib/components/auth/LoginView.svelte';
  import RegisterView from './lib/components/auth/RegisterView.svelte';
  import ForgotPasswordView from './lib/components/auth/ForgotPasswordView.svelte';
  import ConfirmCodeView from './lib/components/auth/ConfirmCodeView.svelte';
  import ResetPasswordView from './lib/components/auth/ResetPasswordView.svelte';
  import LoginModal from './lib/components/auth/LoginModal.svelte';
  import ConfirmCodeModal from './lib/components/auth/ConfirmCodeModal.svelte';
  import DepositModal from './lib/components/payment/DepositModal.svelte';

  import { uiStore } from './lib/stores/uiStore';
  import { authStore, currentUser } from './lib/stores/authStore';
  import { wishlistStore } from './lib/stores/wishlistStore';
  import { cartStore } from './lib/stores/cartStore';
  import { friendsStore } from './lib/stores/friendsStore';
  import { friendsHubService } from './lib/services/friendsHubService';
  import { onlineHubService } from './lib/services/onlineHubService';
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

  $effect(() => {
    if ($currentUser?.id) {
      checkUserBanStatus();
      wishlistStore.loadWishlist();
      cartStore.loadCart();
      friendsStore.loadAll();
      friendsHubService.start();
    } else {
      friendsHubService.stop();
    }
  });

  onMount(() => {
   
    onlineHubService.startConnection();

    checkUserBanStatus();
    wishlistStore.loadWishlist();
    cartStore.loadCart();
    if ($currentUser?.id) {
      friendsStore.loadAll();
      friendsHubService.start();
    }
    const interval = setInterval(checkUserBanStatus, 5000);
    return () => clearInterval(interval);
  });

  onDestroy(() => {
    friendsHubService.stop();
    onlineHubService.stopConnection();
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
    {:else if $uiStore.activeTab === 'library'}
      <LibraryView />
    {:else if $uiStore.activeTab === 'community'}
      <CommunityView />
    {:else if $uiStore.activeTab === 'friends'}
      <FriendsView />
    {:else if $uiStore.activeTab === 'profile'}
      <PublicProfileView />
    {:else if $uiStore.activeTab === 'my-profile'}
      <MyProfileView />
    {:else if $uiStore.activeTab === 'catalog'}
      <CatalogView />
    {:else if $uiStore.activeTab === 'game'}
      <GameDetailsView />
    {:else if $uiStore.activeTab === 'wishlist'}
      <WishlistView />
    {:else if $uiStore.activeTab === 'cart'}
      <CartView />
    {:else if $uiStore.activeTab === 'admin'}
      <AdminView />
    {:else if $uiStore.activeTab === 'developer'}
      <DeveloperView />
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

  <LoginModal />
  <ConfirmCodeModal />
  <PublishGameModal />
  <EditDeveloperGameModal />
  {#if $uiStore.isDepositModalOpen}
    <DepositModal />
  {/if}
  <ToastContainer />

  {#if !isBanned}
    <Footer />
  {/if}
</div>