import React, { useState } from 'react';
import {
  View,
  Image,
  ImageStyle,
  StyleProp,
  StyleSheet,
  ActivityIndicator,
  Text,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { resolveMediaUrl } from '../utils/constants';
import { theme } from '../styles/theme';

interface BackendImageProps {
  src?: string | null;
  style?: StyleProp<ImageStyle>;
  resizeMode?: 'cover' | 'contain' | 'stretch' | 'center';
  fallbackText?: string;
  fallbackIconSize?: number;
}

export const BackendImage: React.FC<BackendImageProps> = ({
  src,
  style,
  resizeMode = 'cover',
  fallbackText = 'Зображення недоступне',
  fallbackIconSize = 28,
}) => {
  const [isLoaded, setIsLoaded] = useState(false);
  const [isError, setIsError] = useState(false);

  const resolvedSrc = resolveMediaUrl(src);

  if (!resolvedSrc || isError) {
    return (
      <View style={[styles.fallbackContainer, style]}>
        <Ionicons name="image-outline" size={fallbackIconSize} color={theme.colors.textDim} />
        {fallbackText ? (
          <Text style={styles.fallbackText} numberOfLines={1}>
            {fallbackText}
          </Text>
        ) : null}
      </View>
    );
  }

  return (
    <View style={[styles.container, style]}>
      {!isLoaded && !isError && (
        <View style={styles.loaderContainer}>
          <ActivityIndicator size="small" color={theme.colors.primary} />
        </View>
      )}

      <Image
        source={{ uri: resolvedSrc }}
        style={[
          StyleSheet.absoluteFill,
          { opacity: isLoaded ? 1 : 0 },
        ]}
        resizeMode={resizeMode}
        onLoad={() => setIsLoaded(true)}
        onError={() => {
          setIsError(true);
          setIsLoaded(true);
        }}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: theme.colors.surface,
    overflow: 'hidden',
    position: 'relative',
    justifyContent: 'center',
    alignItems: 'center',
  },
  loaderContainer: {
    ...StyleSheet.absoluteFill,
    backgroundColor: 'rgba(6, 24, 32, 0.7)',
    justifyContent: 'center',
    alignItems: 'center',
    zIndex: 1,
  },
  fallbackContainer: {
    backgroundColor: theme.colors.surface,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 8,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    overflow: 'hidden',
  },
  fallbackText: {
    fontSize: 10,
    color: theme.colors.textDim,
    marginTop: 4,
    textAlign: 'center',
    fontWeight: '500',
  },
});
