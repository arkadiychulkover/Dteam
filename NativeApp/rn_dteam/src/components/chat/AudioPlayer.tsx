import React, { useState, useEffect, useRef } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Platform } from 'react-native';
import { colors } from '../../theme/colors';
import { Ionicons } from '@expo/vector-icons';
import { Audio } from 'expo-av';
import { resolveMediaUrl } from '../../utils/constants';

interface AudioPlayerProps {
  duration?: number;
  voiceUri?: string;
  isMine?: boolean;
}

export const AudioPlayer: React.FC<AudioPlayerProps> = ({
  duration = 14,
  voiceUri,
  isMine = false,
}) => {
  const [isPlaying, setIsPlaying] = useState(false);
  const [currentSeconds, setCurrentSeconds] = useState(0);
  const soundRef = useRef<Audio.Sound | null>(null);
  const webAudioRef = useRef<HTMLAudioElement | null>(null);

  useEffect(() => {
    if (webAudioRef.current) {
      webAudioRef.current.pause();
      webAudioRef.current = null;
    }
    if (soundRef.current) {
      soundRef.current.unloadAsync().catch(() => {});
      soundRef.current = null;
    }
    setIsPlaying(false);
    setCurrentSeconds(0);
  }, [voiceUri]);

  useEffect(() => {
    return () => {
      if (soundRef.current) {
        soundRef.current.unloadAsync().catch(() => {});
      }
      if (webAudioRef.current) {
        webAudioRef.current.pause();
        webAudioRef.current = null;
      }
    };
  }, []);

  const togglePlay = async () => {
    if (isPlaying) {
      if (Platform.OS === 'web' && webAudioRef.current) {
        webAudioRef.current.pause();
      } else if (soundRef.current) {
        await soundRef.current.pauseAsync().catch(() => {});
      }
      setIsPlaying(false);
      return;
    }

    const targetUri = resolveMediaUrl(voiceUri) || voiceUri;

    if (targetUri) {
      try {
        if (Platform.OS === 'web') {
          if (!webAudioRef.current) {
            const audio = new window.Audio(targetUri);
            audio.onended = () => {
              setIsPlaying(false);
              setCurrentSeconds(0);
            };
            audio.ontimeupdate = () => {
              setCurrentSeconds(Math.floor(audio.currentTime));
            };
            audio.onerror = (e) => {
              console.warn('[AudioPlayer] Audio load error:', e);
              setIsPlaying(false);
            };
            webAudioRef.current = audio;
          }
          await webAudioRef.current.play();
          setIsPlaying(true);
          return;
        } else {
          if (!soundRef.current) {
            await Audio.setAudioModeAsync({
              playsInSilentModeIOS: true,
              allowsRecordingIOS: false,
            });
            const { sound } = await Audio.Sound.createAsync(
              { uri: targetUri },
              { shouldPlay: true },
              (status) => {
                if (status.isLoaded) {
                  if (status.didJustFinish) {
                    setIsPlaying(false);
                    setCurrentSeconds(0);
                  } else if (status.positionMillis != null) {
                    setCurrentSeconds(Math.floor(status.positionMillis / 1000));
                  }
                }
              }
            );
            soundRef.current = sound;
          } else {
            await soundRef.current.playAsync();
          }
          setIsPlaying(true);
          return;
        }
      } catch (err) {
        console.warn('Real audio playback fallback to simulated:', err);
      }
    }

    setIsPlaying(true);
  };

  useEffect(() => {
    let interval: any = null;
    if (isPlaying && (!voiceUri || (!webAudioRef.current && !soundRef.current))) {
      interval = setInterval(() => {
        setCurrentSeconds((prev) => {
          if (prev >= duration) {
            setIsPlaying(false);
            return 0;
          }
          return prev + 1;
        });
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [isPlaying, duration, voiceUri]);

  const formatTime = (secs: number) => {
    const mins = Math.floor(secs / 60);
    const remaining = secs % 60;
    return `${mins}:${remaining < 10 ? '0' : ''}${remaining}`;
  };

  const bars = [10, 18, 24, 14, 28, 22, 16, 30, 24, 18, 12, 26, 20, 14, 22, 16, 12];

  return (
    <View style={styles.container}>
      <TouchableOpacity
        style={[styles.playButton, isMine ? styles.playButtonMine : styles.playButtonOther]}
        onPress={togglePlay}
        activeOpacity={0.8}
      >
        <Ionicons
          name={isPlaying ? 'pause' : 'play'}
          size={16}
          color={isMine ? '#000000' : colors.primary}
          style={{ marginLeft: isPlaying ? 0 : 2 }}
        />
      </TouchableOpacity>

      <View style={styles.waveformContainer}>
        <View style={styles.waveformRow}>
          {bars.map((height, index) => {
            const playedPercent = (currentSeconds / Math.max(1, duration)) * bars.length;
            const isBarActive = isPlaying && index <= playedPercent;

            return (
              <View
                key={index}
                style={[
                  styles.waveBar,
                  {
                    height,
                    backgroundColor: isBarActive
                      ? (isMine ? '#03212c' : colors.primary)
                      : (isMine ? 'rgba(0,0,0,0.35)' : 'rgba(34, 211, 238, 0.3)'),
                  },
                ]}
              />
            );
          })}
        </View>

        <View style={styles.metaRow}>
          <Text style={[styles.durationText, { color: isMine ? 'rgba(0,0,0,0.7)' : colors.textMuted }]}>
            {formatTime(isPlaying ? currentSeconds : duration)}
          </Text>
          {voiceUri && (
            <View style={styles.micRealBadge}>
              <Ionicons name="mic" size={10} color={isMine ? '#000' : colors.primary} />
              <Text style={[styles.realLabel, { color: isMine ? '#000' : colors.primary }]}>Live Audio</Text>
            </View>
          )}
        </View>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    minWidth: 200,
  },
  playButton: {
    width: 38,
    height: 38,
    borderRadius: 19,
    alignItems: 'center',
    justifyContent: 'center',
  },
  playButtonMine: {
    backgroundColor: colors.primary,
  },
  playButtonOther: {
    backgroundColor: 'rgba(34, 211, 238, 0.15)',
    borderWidth: 1,
    borderColor: colors.primary,
  },
  waveformContainer: {
    flex: 1,
    gap: 4,
  },
  waveformRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 3,
    height: 32,
  },
  waveBar: {
    width: 3,
    borderRadius: 2,
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  durationText: {
    fontSize: 10,
    fontFamily: 'monospace',
    fontWeight: '600',
  },
  micRealBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 3,
    opacity: 0.8,
  },
  realLabel: {
    fontSize: 9,
    fontWeight: '700',
    fontFamily: 'monospace',
  },
});
