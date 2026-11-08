import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  let service: ThemeService;

  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
    TestBed.configureTestingModule({});
    service = TestBed.inject(ThemeService);
  });

  afterEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  });

  it('should initialize with default light theme when no storage preference exists', () => {
    expect(service.currentTheme()).toBe('light');
    expect(service.isDark()).toBe(false);
    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
  });

  it('should toggle theme from light to dark and back to light', () => {
    expect(service.currentTheme()).toBe('light');

    service.toggleTheme();
    expect(service.currentTheme()).toBe('dark');
    expect(service.isDark()).toBe(true);
    expect(localStorage.getItem('dnd_theme')).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');

    service.toggleTheme();
    expect(service.currentTheme()).toBe('light');
    expect(service.isDark()).toBe(false);
    expect(localStorage.getItem('dnd_theme')).toBe('light');
    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
  });

  it('should set theme explicitly and persist', () => {
    service.setTheme('dark');
    expect(service.currentTheme()).toBe('dark');
    expect(localStorage.getItem('dnd_theme')).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');

    service.setTheme('light');
    expect(service.currentTheme()).toBe('light');
    expect(localStorage.getItem('dnd_theme')).toBe('light');
    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
  });
});

