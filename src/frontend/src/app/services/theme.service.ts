import { Injectable, signal, computed, effect } from '@angular/core';

export type AppTheme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly STORAGE_KEY = 'dnd_theme';
  readonly currentTheme = signal<AppTheme>('light');
  readonly isDark = computed(() => this.currentTheme() === 'dark');

  constructor() {
    this.initTheme();
  }

  private initTheme(): void {
    if (typeof window === 'undefined') return;

    try {
      const savedTheme = localStorage.getItem(this.STORAGE_KEY) as AppTheme | null;
      if (savedTheme === 'dark' || savedTheme === 'light') {
        this.applyTheme(savedTheme, false);
      } else {
        // Padrão obrigatório do projeto: Light Mode quando não há preferência salva
        this.applyTheme('light', false);
      }
    } catch {
      this.applyTheme('light', false);
    }
  }

  toggleTheme(): void {
    const nextTheme: AppTheme = this.currentTheme() === 'light' ? 'dark' : 'light';
    this.setTheme(nextTheme);
  }

  setTheme(theme: AppTheme): void {
    this.applyTheme(theme, true);
  }

  private applyTheme(theme: AppTheme, persist: boolean): void {
    this.currentTheme.set(theme);

    if (typeof document !== 'undefined') {
      document.documentElement.setAttribute('data-theme', theme);
    }

    if (persist && typeof window !== 'undefined') {
      try {
        localStorage.setItem(this.STORAGE_KEY, theme);
      } catch {
        // Silently catch quota or privacy errors
      }
    }
  }
}

