import React, { useEffect, useState } from 'react';
import { StyleSheet, View } from 'react-native';
import { StatusBar } from 'expo-status-bar';
import { NavigationContainer, DarkTheme } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { SafeAreaProvider, SafeAreaView } from 'react-native-safe-area-context';

import { useAuthStore } from './src/store/useAuthStore';
import { useChatStore } from './src/store/useChatStore';
import { tokenStorage } from './src/services/tokenStorage';
import { chatService } from './src/services/chatService';
import { theme } from './src/styles/theme';
import { colors } from './src/theme/colors';

import type { Game, UserGame, Dialog, TabId } from './src/types';

import { HomeScreen } from './src/screens/HomeScreen';
import { GameDetailsScreen } from './src/screens/GameDetailsScreen';
import { LibraryScreen } from './src/screens/LibraryScreen';
import { LibraryGameScreen } from './src/screens/LibraryGameScreen';
import { ChatsScreen } from './src/screens/ChatsScreen';
import { ChatDetailScreen } from './src/screens/ChatDetailScreen';
import { ProfileScreen } from './src/screens/ProfileScreen';
import { SettingsScreen } from './src/screens/SettingsScreen';
import { CartScreen } from './src/screens/CartScreen';
import { LoginScreen } from './src/screens/LoginScreen';
import { ForgotPasswordScreen } from './src/screens/ForgotPasswordScreen';
import { VerifyCodeScreen } from './src/screens/VerifyCodeScreen';

import { BottomTabBar } from './src/components/common/BottomTabBar';

export type RootStackParamList = {
  MainTabs: { initialTab?: TabId } | undefined;
  GameDetails: { game: Game };
  LibraryGame: { game: Game; userGame?: UserGame };
  ChatDetail: { dialog: Dialog };
  Settings: undefined;
  Cart: undefined;
  Login: undefined;
  ForgotPassword: undefined;
  VerifyCode: { email?: string; code?: string };
};

const Stack = createNativeStackNavigator<RootStackParamList>();

const navigationTheme = {
  ...DarkTheme,
  colors: {
    ...DarkTheme.colors,
    primary: theme.colors.primary,
    background: theme.colors.background,
    card: theme.colors.surface,
    text: theme.colors.text,
    border: theme.colors.borderSubtle,
  },
};

interface MainTabsScreenProps {
  navigation: any;
  route?: any;
}

function MainTabsScreen({ navigation, route }: MainTabsScreenProps) {
  const [activeTab, setActiveTab] = useState<TabId>(route?.params?.initialTab || 'store');
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const unreadTotal = useChatStore((s) => s.unreadTotal);
  const startRealtime = useChatStore((s) => s.startRealtime);
  const stopRealtime = useChatStore((s) => s.stopRealtime);
  const loadConversations = useChatStore((s) => s.loadConversations);

  // Запускаємо реалтайм хаби SignalR (ChatHub + FriendsHub) при авторизації
  useEffect(() => {
    if (isAuthenticated) {
      loadConversations();
      startRealtime();
    } else {
      stopRealtime();
    }
  }, [isAuthenticated]);

  return (
    <SafeAreaView style={styles.tabContainer} edges={['left', 'right', 'bottom']}>
      <View style={styles.tabContent}>
        {activeTab === 'store' && (
          <HomeScreen
            navigation={navigation}
            onSelectGame={(game) => navigation.navigate('GameDetails', { game })}
            onNavigateLibrary={() => setActiveTab('library')}
            onNavigateLogin={() => navigation.navigate('Login')}
          />
        )}

        {activeTab === 'library' && (
          <LibraryScreen
            navigation={navigation}
            onSelectLibraryGame={(game, userGame) =>
              navigation.navigate('LibraryGame', { game, userGame })
            }
            onNavigateCatalog={() => setActiveTab('store')}
            onNavigateLogin={() => navigation.navigate('Login')}
          />
        )}

        {activeTab === 'chats' && (
          <ChatsScreen
            onSelectDialog={(dialog) => navigation.navigate('ChatDetail', { dialog })}
            navigation={navigation}
            onNavigateLogin={() => navigation.navigate('Login')}
          />
        )}

        {activeTab === 'profile' && (
          <ProfileScreen
            navigation={navigation}
            onNavigateToSettings={() => navigation.navigate('Settings')}
            onNavigateToLibrary={() => setActiveTab('library')}
            onNavigateToLogin={() => navigation.navigate('Login')}
          />
        )}
      </View>

      <BottomTabBar
        activeTab={activeTab}
        onSelectTab={setActiveTab}
        unreadCount={unreadTotal}
      />
    </SafeAreaView>
  );
}

export default function App() {
  useEffect(() => {
    tokenStorage.init().catch((err) => {
      console.warn('[App] Failed to initialize token storage:', err);
    });

    useAuthStore.getState().checkAuth().catch((err) => {
      console.warn('[App] Failed to check auth session:', err);
    });
  }, []);

  return (
    <SafeAreaProvider>
      <StatusBar style="light" />
      <NavigationContainer theme={navigationTheme}>
        <Stack.Navigator
          initialRouteName="MainTabs"
          screenOptions={{
            headerShown: false,
            animation: 'slide_from_right',
            contentStyle: { backgroundColor: colors.background },
          }}
        >
          <Stack.Screen name="MainTabs" component={MainTabsScreen} />
          <Stack.Screen name="GameDetails" component={GameDetailsScreen} />
          <Stack.Screen name="LibraryGame" component={LibraryGameScreen} />
          <Stack.Screen name="ChatDetail" component={ChatDetailScreen} />
          <Stack.Screen name="Cart" component={CartScreen} />

          <Stack.Screen name="Settings">
            {(props) => (
              <SettingsScreen
                navigation={props.navigation}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>

          <Stack.Screen name="Login" component={LoginScreen} />
          <Stack.Screen name="ForgotPassword" component={ForgotPasswordScreen} />
          <Stack.Screen name="VerifyCode" component={VerifyCodeScreen} />
        </Stack.Navigator>
      </NavigationContainer>
    </SafeAreaProvider>
  );
}

const styles = StyleSheet.create({
  tabContainer: {
    flex: 1,
    backgroundColor: colors.background,
  },
  tabContent: {
    flex: 1,
  },
});
