
export interface ProfileLevelInfo {
  level: number;
  nextLevel: number;
  currentXp: number;
  xpForCurrentLevel: number;
  xpForNextLevel: number;
  xpRemaining: number;
  progressPercent: number;
}

export function getTotalXpForLevel(level: number): number {
  if (level <= 0) return 0;
  return 500 * level * level + 9500 * level;
}

export function getStepXpForLevel(level: number): number {
  if (level <= 0) return 0;
  return 10000 + (level - 1) * 1000;
}

export function calculateProfileLevel(rawTokens: number | null | undefined): ProfileLevelInfo {
  const tokens = Math.max(0, Math.floor(Number(rawTokens) || 0));

  if (tokens < 10000) {
    const xpRemaining = 10000 - tokens;
    const progressPercent = Math.min(100, Math.max(0, (tokens / 10000) * 100));
    return {
      level: 0,
      nextLevel: 1,
      currentXp: tokens,
      xpForCurrentLevel: 0,
      xpForNextLevel: 10000,
      xpRemaining,
      progressPercent
    };
  }

  const level = Math.floor(-9.5 + Math.sqrt(90.25 + 0.002 * tokens));
  const nextLevel = level + 1;
  const xpForCurrentLevel = getTotalXpForLevel(level);
  const xpForNextLevel = getTotalXpForLevel(nextLevel);
  const xpRequiredForStep = xpForNextLevel - xpForCurrentLevel;
  const xpRemaining = Math.max(0, xpForNextLevel - tokens);
  const progressPercent = xpRequiredForStep > 0
    ? Math.min(100, Math.max(0, ((tokens - xpForCurrentLevel) / xpRequiredForStep) * 100))
    : 0;

  return {
    level,
    nextLevel,
    currentXp: tokens,
    xpForCurrentLevel,
    xpForNextLevel,
    xpRemaining,
    progressPercent
  };
}
