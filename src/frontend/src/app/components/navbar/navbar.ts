import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { TimeRecordService } from '../../services/time-record.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
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
}

