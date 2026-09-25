import React, { useState } from 'react';
import {
  StyleSheet,
  View,
  SafeAreaView,
  Platform,
  Alert,
} from 'react-native';
import { StatusBar } from 'expo-status-bar';
import { colors } from './src/theme/colors';
import { TabId, Dialog, UserProfile, ChatMessage } from './src/types';
import {
  initialProfile,
  initialDialogs,
  initialTransactions,
} from './src/data/mockData';
import { Header } from './src/components/common/Header';
import { BottomTabBar } from './src/components/common/BottomTabBar';
import { ProfileScreen } from './src/screens/ProfileScreen';
import { SettingsScreen } from './src/screens/SettingsScreen';
import { ChatsScreen } from './src/screens/ChatsScreen';
import { ChatDetailScreen } from './src/screens/ChatDetailScreen';
import { LibraryPlaceholderScreen } from './src/screens/LibraryPlaceholderScreen';
import { StorePlaceholderScreen } from './src/screens/StorePlaceholderScreen';

export default function App() {
  const [activeTab, setActiveTab] = useState<TabId>('profile');
  const [activeDialog, setActiveDialog] = useState<Dialog | null>(null);

  const [profile, setProfile] = useState<UserProfile>(initialProfile);
  const [dialogs, setDialogs] = useState<Dialog[]>(initialDialogs);
  const [transactions, setTransactions] = useState(initialTransactions);

  const [isDemoMode, setIsDemoMode] = useState<boolean>(true);

  // Total unread messages count for the tab bar badge
  const unreadCount = dialogs.reduce((sum, d) => sum + d.unreadCount, 0);

  const handleSelectTab = (tab: TabId) => {
    setActiveTab(tab);
    setActiveDialog(null);
  };

  const handleSelectDialog = (dialog: Dialog) => {
    // Reset unread count for this dialog
    setDialogs((prev) =>
      prev.map((d) => (d.id === dialog.id ? { ...d, unreadCount: 0 } : d))
    );
    setActiveDialog({ ...dialog, unreadCount: 0 });
  };

  const handleBackFromChat = () => {
    setActiveDialog(null);
  };

  const getCurrentTimeFormatted = () => {
    const now = new Date();
    const hours = now.getHours().toString().padStart(2, '0');
    const minutes = now.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  };

  const handleSendMessage = (dialogId: string, text: string) => {
    const nowTime = getCurrentTimeFormatted();
    const newMsg: ChatMessage = {
      id: `m-${Date.now()}`,
      dialogId,
      senderId: profile.id,
      text,
      type: 'text',
      timestamp: nowTime,
      isMine: true,
      status: 'sent',
    };

    setDialogs((prev) =>
      prev.map((d) => {
        if (d.id === dialogId) {
          return {
            ...d,
            lastMessage: text,
            lastMessageTime: nowTime,
            messages: [...d.messages, newMsg],
          };
        }
        return d;
      })
    );

    if (activeDialog && activeDialog.id === dialogId) {
      setActiveDialog((prev) =>
        prev
          ? {
              ...prev,
              lastMessage: text,
              lastMessageTime: nowTime,
              messages: [...prev.messages, newMsg],
            }
          : null
      );
    }
  };

  const handleSendVoice = (dialogId: string) => {
    const nowTime = getCurrentTimeFormatted();
    const duration = Math.floor(Math.random() * 15) + 6;
    const newMsg: ChatMessage = {
      id: `m-voice-${Date.now()}`,
      dialogId,
      senderId: profile.id,
      text: 'Голосове повідомлення',
      type: 'voice',
      voiceDuration: duration,
      timestamp: nowTime,
      isMine: true,
      status: 'sent',
    };

    setDialogs((prev) =>
      prev.map((d) => {
        if (d.id === dialogId) {
          return {
            ...d,
            lastMessage: `🎙️ Голосове повідомлення (0:${duration < 10 ? '0' : ''}${duration})`,
            lastMessageTime: nowTime,
            messages: [...d.messages, newMsg],
          };
        }
        return d;
      })
    );

    if (activeDialog && activeDialog.id === dialogId) {
      setActiveDialog((prev) =>
        prev
          ? {
              ...prev,
              lastMessage: `🎙️ Голосове повідомлення (0:${duration < 10 ? '0' : ''}${duration})`,
              lastMessageTime: nowTime,
              messages: [...prev.messages, newMsg],
            }
          : null
      );
    }
  };

  const handleUpdateProfile = (updated: Partial<UserProfile>) => {
    setProfile((prev) => ({ ...prev, ...updated }));
  };

  const handleToggleDemoMode = () => {
    const nextMode = !isDemoMode;
    setIsDemoMode(nextMode);
    Alert.alert(
      nextMode ? '⚡ Демо-режим увімкнено' : '🌐 Режим підключення до API',
      nextMode
        ? 'Використовуються локальні тестові дані. Ідеально для автономної презентації!'
        : 'Спроба підключення до локального ASP.NET Core API (http://localhost:5117).'
    );
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <StatusBar style="light" />

      {/* Top Header */}
      <Header isDemoMode={isDemoMode} onToggleDemoMode={handleToggleDemoMode} />

      {/* Main Screen Content */}
      <View style={styles.content}>
        {activeTab === 'chats' && (
          activeDialog ? (
            <ChatDetailScreen
              dialog={activeDialog}
              onBack={handleBackFromChat}
              onSendMessage={handleSendMessage}
              onSendVoice={handleSendVoice}
            />
          ) : (
            <ChatsScreen
              dialogs={dialogs}
              onSelectDialog={handleSelectDialog}
            />
          )
        )}

        {activeTab === 'profile' && (
          <ProfileScreen
            profile={profile}
            onNavigateToSettings={() => setActiveTab('settings')}
          />
        )}

        {activeTab === 'settings' && (
          <SettingsScreen
            profile={profile}
            transactions={transactions}
            onUpdateProfile={handleUpdateProfile}
          />
        )}

        {activeTab === 'library' && <LibraryPlaceholderScreen />}

        {activeTab === 'store' && <StorePlaceholderScreen />}
      </View>

      {/* Bottom Navigation Bar */}
      <BottomTabBar
        activeTab={activeTab}
        onSelectTab={handleSelectTab}
        unreadCount={unreadCount}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.background,
    paddingTop: Platform.OS === 'android' ? 24 : 0,
  },
  content: {
    flex: 1,
  },
});
