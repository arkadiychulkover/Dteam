import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert } from 'react-native';
import { colors } from '../../theme/colors';
import { Ionicons } from '@expo/vector-icons';

export const DeleteAccountTab: React.FC = () => {
  const handleDelete = () => {
    Alert.alert(
      'Видалення акаунта',
      'Ви впевнені, що бажаєте безповоротно видалити свій обліковий запис Dteam? Усі ігри, досягнення та NFT значки буде втрачено.',
      [
        { text: 'Скасувати', style: 'cancel' },
        {
          text: 'Так, видалити',
          style: 'destructive',
          onPress: () => {
            Alert.alert(
              'Запит на видалення надіслано',
              'Ваш запит зареєстровано в системі. Протягом 14 днів акаунт буде повністю видалено з бази даних Dteam.'
            );
          },
        },
      ]
    );
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Небезпечна зона</Text>
        <Text style={styles.subtitle}>Дії, які мають незворотні наслідки для вашого профілю</Text>
      </View>

      <View style={styles.warningCard}>
        <View style={styles.warningHeader}>
          <Ionicons name="warning" size={24} color={colors.danger} />
          <Text style={styles.warningTitle}>Безповоротне видалення акаунта</Text>
        </View>

        <Text style={styles.warningBody}>
          Після підтвердження видалення вашого акаунта:
        </Text>

        <View style={styles.bulletList}>
          <Text style={styles.bulletItem}>• Доступ до всієї придбаної бібліотеки ігор буде втрачено</Text>
          <Text style={styles.bulletItem}>• Усі невнесені в блокчейн бали DTP буде анульовано</Text>
          <Text style={styles.bulletItem}>• Історію діалогів, відгуки та списки друзів буде очищено</Text>
          <Text style={styles.bulletItem}>• NFT-значки залишаться у вашому Web3-гаманці, але відв'яжуться від платформи</Text>
        </View>

        <TouchableOpacity style={styles.deleteBtn} onPress={handleDelete} activeOpacity={0.8}>
          <Ionicons name="trash-outline" size={18} color="#ffffff" />
          <Text style={styles.deleteBtnText}>Видалити мій акаунт Dteam</Text>
        </TouchableOpacity>
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
    color: colors.danger,
    marginBottom: 4,
  },
  subtitle: {
    fontSize: 12,
    color: colors.textMuted,
  },
  warningCard: {
    backgroundColor: 'rgba(239, 68, 68, 0.08)',
    borderRadius: 20,
    padding: 18,
    borderWidth: 1,
    borderColor: 'rgba(239, 68, 68, 0.3)',
  },
  warningHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    marginBottom: 12,
  },
  warningTitle: {
    fontSize: 15,
    fontWeight: '800',
    color: colors.danger,
    flex: 1,
  },
  warningBody: {
    fontSize: 13,
    color: colors.text,
    fontWeight: '600',
    marginBottom: 10,
  },
  bulletList: {
    gap: 6,
    marginBottom: 20,
  },
  bulletItem: {
    fontSize: 12,
    color: colors.textMuted,
    lineHeight: 18,
  },
  deleteBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: colors.danger,
    borderRadius: 14,
    paddingVertical: 14,
  },
  deleteBtnText: {
    color: '#ffffff',
    fontWeight: '800',
    fontSize: 13,
  },
});
