import React, { useEffect } from 'react';
import { StatusBar } from 'expo-status-bar';
import { NavigationContainer, DarkTheme } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { HomeScreen } from './src/screens/HomeScreen';
import { GameDetailsScreen } from './src/screens/GameDetailsScreen';
import { LoginScreen } from './src/screens/LoginScreen';
import { ForgotPasswordScreen } from './src/screens/ForgotPasswordScreen';
import { VerifyCodeScreen } from './src/screens/VerifyCodeScreen';
import { LibraryScreen } from './src/screens/LibraryScreen';
import { LibraryGameScreen } from './src/screens/LibraryGameScreen';
import { useAuthStore } from './src/store/useAuthStore';
import { theme } from './src/styles/theme';
import type { Game, UserGame } from './src/types';

export type RootStackParamList = {
  Home: undefined;
  GameDetails: { game: Game };
  Login: undefined;
  ForgotPassword: undefined;
  VerifyCode: { email?: string; code?: string };
  Library: undefined;
  LibraryGame: { game: Game; userGame?: UserGame };
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

export default function App() {
  useEffect(() => {
    useAuthStore.getState().checkAuth().catch((err) => {
      console.warn('[App] Failed to initialize auth session:', err);
    });
  }, []);

  return (
    <SafeAreaProvider>
      <StatusBar style="light" />
      <NavigationContainer theme={navigationTheme}>
        <Stack.Navigator
          initialRouteName="Home"
          screenOptions={{
            headerShown: false,
            animation: 'slide_from_right',
            contentStyle: { backgroundColor: theme.colors.background },
          }}
        >
          <Stack.Screen name="Home">
            {(props) => (
              <HomeScreen
                navigation={props.navigation}
                onSelectGame={(game) => props.navigation.navigate('GameDetails', { game })}
              />
            )}
          </Stack.Screen>
          <Stack.Screen name="GameDetails" component={GameDetailsScreen} />
          <Stack.Screen name="Login" component={LoginScreen} />
          <Stack.Screen name="ForgotPassword" component={ForgotPasswordScreen} />
          <Stack.Screen name="VerifyCode" component={VerifyCodeScreen} />
          <Stack.Screen name="Library" component={LibraryScreen} />
          <Stack.Screen name="LibraryGame" component={LibraryGameScreen} />
        </Stack.Navigator>
      </NavigationContainer>
    </SafeAreaProvider>
  );
}
