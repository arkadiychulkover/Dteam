import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../../theme/colors';

interface ProfileLevelHexagonProps {
  level: number;
  size?: number;
}

export const ProfileLevelHexagon: React.FC<ProfileLevelHexagonProps> = ({
  level,
  size = 44,
}) => {
  return (
    <View
      style={[
        styles.container,
        {
          width: size,
          height: size,
          borderRadius: size * 0.28,
        },
      ]}
    >
      <View
        style={[
          styles.inner,
          {
            borderRadius: size * 0.24,
          },
        ]}
      >
        <Text style={[styles.levelText, { fontSize: size * 0.42 }]}>{level}</Text>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: 'rgba(34, 211, 238, 0.2)',
    borderWidth: 1.5,
    borderColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.6,
    shadowRadius: 6,
    elevation: 4,
  },
  inner: {
    width: '84%',
    height: '84%',
    backgroundColor: '#021118',
    alignItems: 'center',
    justifyContent: 'center',
  },
  levelText: {
    fontWeight: '900',
    color: colors.primary,
    fontFamily: 'monospace',
  },
});
