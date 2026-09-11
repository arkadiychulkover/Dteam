import { writable } from 'svelte/store';

export type Theme = 'dark' | 'light';

function createThemeStore() {
  const isBrowser = typeof window !== 'undefined';
  const saved = isBrowser ? localStorage.getItem('dteam_theme') : null;
  const initialTheme: Theme = saved === 'light' ? 'light' : 'dark';

  const { subscribe, set, update } = writable<Theme>(initialTheme);

  function applyTheme(theme: Theme) {
    if (!isBrowser) return;
    localStorage.setItem('dteam_theme', theme);
    const root = document.documentElement;
    if (theme === 'light') {
      root.classList.remove('dark');
      root.classList.add('light', 'light-theme');
      document.body?.classList.add('light-theme');
    } else {
      root.classList.remove('light', 'light-theme');
      root.classList.add('dark');
      document.body?.classList.remove('light-theme');
    }
  }

  if (isBrowser) {
    applyTheme(initialTheme);
  }

  return {
    subscribe,
    setTheme(theme: Theme) {
      applyTheme(theme);
      set(theme);
    },
    toggleTheme() {
      update(current => {
        const next: Theme = current === 'dark' ? 'light' : 'dark';
        applyTheme(next);
        return next;
      });
    },
    init() {
      if (!isBrowser) return;
      const current = (localStorage.getItem('dteam_theme') as Theme) || 'dark';
      applyTheme(current);
      set(current);
    }
  };
}

export const themeStore = createThemeStore();
