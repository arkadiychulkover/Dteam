import React, { useState } from 'react';
import { View, Text, StyleSheet, Switch } from 'react-native';
import { colors } from '../../theme/colors';
import { Ionicons } from '@expo/vector-icons';

export const NotificationsTab: React.FC = () => {
  const [chatMessages, setChatMessages] = useState(true);
  const [friendRequests, setFriendRequests] = useState(true);
  const [wishlistDiscounts, setWishlistDiscounts] = useState(true);
  const [gameUpdates, setGameUpdates] = useState(true);
  const [emailDigest, setEmailDigest] = useState(false);

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Налаштування сповіщень</Text>
        <Text style={styles.subtitle}>Оберіть, які сповіщення ви бажаєте отримувати</Text>
      </View>

      <View style={styles.card}>
        <View style={styles.toggleRow}>
          <View style={styles.toggleInfo}>
            <View style={styles.iconBox}>
              <Ionicons name="chatbubble-outline" size={18} color={colors.primary} />
            </View>
            <View style={styles.textContainer}>
              <Text style={styles.itemTitle}>Повідомлення в чатах</Text>
              <Text style={styles.itemSubtitle}>Сповіщати про нові приватні повідомлення</Text>
            </View>
          </View>
          <Switch
            value={chatMessages}
            onValueChange={setChatMessages}
            trackColor={{ false: colors.background, true: colors.primaryHover }}
            thumbColor={chatMessages ? colors.primary : colors.textDim}
          />
        </View>

        <View style={styles.toggleRow}>
          <View style={styles.toggleInfo}>
            <View style={styles.iconBox}>
              <Ionicons name="person-add-outline" size={18} color={colors.accentEmerald} />
            </View>
            <View style={styles.textContainer}>
              <Text style={styles.itemTitle}>Запити в друзі</Text>
              <Text style={styles.itemSubtitle}>Сповіщати про нові запити на дружбу</Text>
            </View>
          </View>
          <Switch
            value={friendRequests}
            onValueChange={setFriendRequests}
            trackColor={{ false: colors.background, true: colors.primaryHover }}
            thumbColor={friendRequests ? colors.primary : colors.textDim}
          />
        </View>

        <View style={styles.toggleRow}>
          <View style={styles.toggleInfo}>
            <View style={styles.iconBox}>
              <Ionicons name="pricetag-outline" size={18} color={colors.accentAmber} />
            </View>
            <View style={styles.textContainer}>
              <Text style={styles.itemTitle}>Знижки списку бажаного</Text>
              <Text style={styles.itemSubtitle}>Повідомляти, коли гра з вішліста отримує знижку</Text>
            </View>
          </View>
          <Switch
            value={wishlistDiscounts}
            onValueChange={setWishlistDiscounts}
            trackColor={{ false: colors.background, true: colors.primaryHover }}
            thumbColor={wishlistDiscounts ? colors.primary : colors.textDim}
          />
        </View>

        <View style={styles.toggleRow}>
          <View style={styles.toggleInfo}>
            <View style={styles.iconBox}>
              <Ionicons name="arrow-down-circle-outline" size={18} color={colors.accentPurple} />
            </View>
            <View style={styles.textContainer}>
              <Text style={styles.itemTitle}>Оновлення ігор</Text>
              <Text style={styles.itemSubtitle}>Сповіщати про патчі та нові версії ігор</Text>
            </View>
          </View>
          <Switch
            value={gameUpdates}
            onValueChange={setGameUpdates}
            trackColor={{ false: colors.background, true: colors.primaryHover }}
            thumbColor={gameUpdates ? colors.primary : colors.textDim}
          />
        </View>

        <View style={[styles.toggleRow, { borderBottomWidth: 0 }]}>
          <View style={styles.toggleInfo}>
            <View style={styles.iconBox}>
              <Ionicons name="mail-outline" size={18} color={colors.textMuted} />
            </View>
            <View style={styles.textContainer}>
              <Text style={styles.itemTitle}>Email дайджест</Text>
              <Text style={styles.itemSubtitle}>Щотижневий звіт про події та новинки</Text>
            </View>
          </View>
          <Switch
            value={emailDigest}
            onValueChange={setEmailDigest}
            trackColor={{ false: colors.background, true: colors.primaryHover }}
            thumbColor={emailDigest ? colors.primary : colors.textDim}
          />
        </View>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    padding: 16,
  },
  header: {
    marginBottom: 20,
  },
  title: {
    fontSize: 18,
    fontWeight: '800',
    color: colors.text,
    marginBottom: 4,
  },
  subtitle: {
    fontSize: 12,
    color: colors.textMuted,
  },
  card: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: colors.border,
    overflow: 'hidden',
  },
  toggleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    padding: 14,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  toggleInfo: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    flex: 1,
    marginRight: 10,
  },
  iconBox: {
    width: 38,
    height: 38,
    borderRadius: 12,
    backgroundColor: colors.background,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.borderLight,
  },
  textContainer: {
    flex: 1,
  },
  itemTitle: {
    fontSize: 14,
    fontWeight: '700',
    color: colors.text,
    marginBottom: 2,
  },
  itemSubtitle: {
    fontSize: 11,
    color: colors.textDim,
  },
});
