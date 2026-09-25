
class SoundService {
  private ctx: AudioContext | null = null;
  private isMuted: boolean = false;
  private initialized: boolean = false;

  private initContext() {
    if (typeof window === 'undefined') return null;
    if (!this.ctx) {
      const AudioCtx = window.AudioContext || (window as any).webkitAudioContext;
      if (!AudioCtx) return null;
      this.ctx = new AudioCtx();
    }
    if (this.ctx.state === 'suspended') {
      this.ctx.resume().catch(() => {});
    }
    return this.ctx;
  }

  public registerUserGestureUnlock() {
    if (this.initialized || typeof window === 'undefined') return;
    this.initialized = true;

    const unlock = () => {
      if (this.ctx && this.ctx.state === 'suspended') {
        this.ctx.resume().catch(() => {});
      }
      window.removeEventListener('pointerdown', unlock);
      window.removeEventListener('keydown', unlock);
    };

    window.addEventListener('pointerdown', unlock, { passive: true, once: true });
    window.addEventListener('keydown', unlock, { passive: true, once: true });
  }

  public setMuted(muted: boolean) {
    this.isMuted = muted;
  }

  public getIsMuted(): boolean {
    return this.isMuted;
  }

  public playNotification(type: 'success' | 'info' | 'warning' | 'error' = 'info') {
    if (this.isMuted || typeof window === 'undefined') return;

    try {
      const ctx = this.initContext();
      if (!ctx) return;

      const now = ctx.currentTime;

      const masterGain = ctx.createGain();
      masterGain.gain.setValueAtTime(0.09, now);
      masterGain.connect(ctx.destination);

      if (type === 'success') {

        this.playTone(ctx, masterGain, 1046.5, now, 0.22, 'sine', 0.6);
        this.playTone(ctx, masterGain, 1318.5, now + 0.05, 0.24, 'sine', 0.5);
        this.playTone(ctx, masterGain, 1567.98, now + 0.10, 0.38, 'sine', 0.7);
      } else if (type === 'warning') {

        this.playTone(ctx, masterGain, 587.33, now, 0.18, 'triangle', 0.5);
        this.playTone(ctx, masterGain, 493.88, now + 0.08, 0.26, 'triangle', 0.45);
      } else if (type === 'error') {

        this.playTone(ctx, masterGain, 440.0, now, 0.16, 'triangle', 0.5);
        this.playTone(ctx, masterGain, 392.0, now + 0.07, 0.24, 'triangle', 0.4);
      } else {

        this.playTone(ctx, masterGain, 880.0, now, 0.18, 'sine', 0.55);
        this.playTone(ctx, masterGain, 1318.5, now + 0.06, 0.32, 'sine', 0.65);
      }
    } catch (e) {

    }
  }

  private playTone(
    ctx: AudioContext,
    destination: AudioNode,
    freq: number,
    startTime: number,
    duration: number,
    type: OscillatorType = 'sine',
    peakGain: number = 0.6
  ) {
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();

    osc.type = type;
    osc.frequency.setValueAtTime(freq, startTime);

    gain.gain.setValueAtTime(0.0001, startTime);
    gain.gain.exponentialRampToValueAtTime(peakGain, startTime + 0.012);
    gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

    osc.connect(gain);
    gain.connect(destination);

    osc.start(startTime);
    osc.stop(startTime + duration + 0.03);
  }
}

export const soundService = new SoundService();
