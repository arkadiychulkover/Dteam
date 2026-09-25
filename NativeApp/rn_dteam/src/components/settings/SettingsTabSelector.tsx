import React from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity } from 'react-native';
import { colors } from '../../theme/colors';
import { Ionicons } from '@expo/vector-icons';

export type SettingsTabType = 'general' | 'password' | 'notifications' | 'wallet' | 'delete';

interface SettingsTabSelectorProps {
  activeTab: SettingsTabType;
  onSelectTab: (tab: SettingsTabType) => void;
}

const TABS: { id: SettingsTabType; label: string; icon: keyof typeof Ionicons.glyphMap }[] = [
  { id: 'general', label: 'Загальні', icon: 'person-outline' },
  { id: 'password', label: 'Безпека', icon: 'lock-closed-outline' },
  { id: 'notifications', label: 'Сповіщення', icon: 'notifications-outline' },
  { id: 'wallet', label: 'Гаманець', icon: 'wallet-outline' },
  { id: 'delete', label: 'Акаунт', icon: 'trash-outline' },
];

export const SettingsTabSelector: React.FC<SettingsTabSelectorProps> = ({
  activeTab,
  onSelectTab,
}) => {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={styles.container}
    >
      {TABS.map((tab) => {
        const isActive = activeTab === tab.id;
        const isDelete = tab.id === 'delete';

        return (
          <TouchableOpacity
            key={tab.id}
            style={[
              styles.tab,
              isActive && styles.tabActive,
              isDelete && isActive && styles.tabDeleteActive,
            ]}
            onPress={() => onSelectTab(tab.id)}
            activeOpacity={0.7}
          >
            <Ionicons
              name={tab.icon}
              size={15}
              color={
                isActive
                  ? (isDelete ? colors.danger : colors.primary)
                  : colors.textDim
              }
            />
            <Text
              style={[
                styles.tabLabel,
                isActive && styles.tabLabelActive,
                isDelete && isActive && styles.tabLabelDelete,
              ]}
            >
              {tab.label}
            </Text>
          </TouchableOpacity>
        );
      })}
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    gap: 8,
  },
  tab: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: 12,
    backgroundColor: colors.surfaceCard,
    borderWidth: 1,
    borderColor: colors.border,
  },
  tabActive: {
    backgroundColor: 'rgba(34, 211, 238, 0.12)',
    borderColor: colors.primary,
  },
  tabDeleteActive: {
    backgroundColor: 'rgba(239, 68, 68, 0.12)',
    borderColor: colors.danger,
  },
  tabLabel: {
    fontSize: 12,
    fontWeight: '700',
    color: colors.textMuted,
  },
  tabLabelActive: {
    color: colors.primary,
  },
  tabLabelDelete: {
    color: colors.danger,
  },
});
