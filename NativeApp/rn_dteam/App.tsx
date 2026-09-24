import React, { useEffect } from 'react';
import { StatusBar } from 'expo-status-bar';
import { NavigationContainer, DarkTheme } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { HomeScreen } from './src/screens/HomeScreen';
import { GameDetailsScreen } from './src/screens/GameDetailsScreen';
import { tokenStorage } from './src/services/tokenStorage';
import { theme } from './src/styles/theme';
import type { Game } from './src/types';

export type RootStackParamList = {
  Home: undefined;
  GameDetails: { game: Game };
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
    tokenStorage.init().catch((err) => {
      console.warn('[App] Failed to initialize token storage:', err);
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
                onSelectGame={(game) => props.navigation.navigate('GameDetails', { game })}
              />
            )}
          </Stack.Screen>
          <Stack.Screen
            name="GameDetails"
            component={GameDetailsScreen}
          />
        </Stack.Navigator>
      </NavigationContainer>
    </SafeAreaProvider>
  );
}
