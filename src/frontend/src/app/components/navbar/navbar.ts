import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { TimeRecordService } from '../../services/time-record.service';
import { CommonModule } from '@angular/common';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, ThemeToggleComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class NavbarComponent implements OnInit, OnDestroy {
  readonly currentTime = signal('');
  readonly timezoneInfo = signal('');
  private timerInterval?: any;

  constructor(
    public authService: AuthService,
    private timeRecordService: TimeRecordService
  ) {}

  ngOnInit(): void {
    this.updateClock();
    this.timerInterval = setInterval(() => this.updateClock(), 1000);
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  private updateClock(): void {
    const now = new Date();
    const timeStr = now.toLocaleTimeString('pt-BR', { hour12: false });
    this.currentTime.set(timeStr);

    const tz = this.timeRecordService.getBrowserTimezone();
    const offset = this.timeRecordService.getBrowserOffsetMinutes();
    const sign = offset >= 0 ? '+' : '-';
    const hours = Math.floor(Math.abs(offset) / 60);
    const mins = Math.abs(offset) % 60;
    const offsetFormatted = `UTC${sign}${hours.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}`;

    this.timezoneInfo.set(`${tz} (${offsetFormatted})`);
  }

  getRoleLabel(role?: number): string {
    switch (role) {
      case 3: return 'Recursos Humanos';
      case 2: return 'Gestor';
      default: return 'Colaborador';
    }
  }

  getUserInitials(): string {
    const name = this.authService.currentUser()?.name;
    if (!name) return 'DD';
    const cleaned = name.replace(/\(.*?\)/g, '').replace(/\[.*?\]/g, '').trim();
    const parts = cleaned.split(' ').filter(Boolean);
    if (parts.length === 0) return 'DD';
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }
}

