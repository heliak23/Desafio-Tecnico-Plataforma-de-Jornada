import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService, AppTheme } from '../../services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './theme-toggle.html',
  styleUrl: './theme-toggle.css'
})
export class ThemeToggleComponent {
  readonly themeService = inject(ThemeService);

  @Input() compact = false;

  setTheme(theme: AppTheme): void {
    if (this.themeService.currentTheme() !== theme) {
      this.themeService.setTheme(theme);
    }
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  onKeyDown(event: KeyboardEvent, theme: AppTheme): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.setTheme(theme);
    }
  }
}

