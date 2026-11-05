import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TimeRecordService } from '../../services/time-record.service';
import { AuthService } from '../../services/auth.service';
import { MonthlyTimesheetReport, DailyTimeReport, TimeRecordType } from '../../models/types';

@Component({
  selector: 'app-espelho',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './espelho.html',
  styleUrl: './espelho.css'
})
export class EspelhoComponent implements OnInit {
  report = signal<MonthlyTimesheetReport | null>(null);
  isLoading = signal(false);

  selectedYear: number = new Date().getFullYear();
  selectedMonth: number = new Date().getMonth() + 1;
  selectedTimezone: string = '';

  months = [
    { value: 1, name: 'Janeiro' },
    { value: 2, name: 'Fevereiro' },
    { value: 3, name: 'Março' },
    { value: 4, name: 'Abril' },
    { value: 5, name: 'Maio' },
    { value: 6, name: 'Junho' },
    { value: 7, name: 'Julho' },
    { value: 8, name: 'Agosto' },
    { value: 9, name: 'Setembro' },
    { value: 10, name: 'Outubro' },
    { value: 11, name: 'Novembro' },
    { value: 12, name: 'Dezembro' }
  ];

  years: number[] = [2025, 2026, 2027];

  availableTimezones = [
    { id: 'America/Sao_Paulo', label: '🇧🇷 Brasil (São Paulo - UTC-3)' },
    { id: 'Europe/Lisbon', label: '🇵🇹 Portugal (Lisboa - UTC+0/UTC+1)' },
    { id: 'Europe/Madrid', label: '🇪🇸 Espanha (Madrid - UTC+1/UTC+2)' },
    { id: 'Europe/Berlin', label: '🇩🇪 Alemanha (Berlim - UTC+1/UTC+2)' },
    { id: 'Europe/Paris', label: '🇫🇷 França (Paris - UTC+1/UTC+2)' },
    { id: 'Europe/London', label: '🇬🇧 Reino Unido (Londres - UTC+0/UTC+1)' },
    { id: 'UTC', label: '🌐 UTC Universal' }
  ];

  constructor(
    public authService: AuthService,
    private timeRecordService: TimeRecordService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.selectedTimezone = this.timeRecordService.getBrowserTimezone();
    this.loadTimesheet();
  }

  loadTimesheet(): void {
    this.isLoading.set(true);

    this.timeRecordService.getMonthly(this.selectedYear, this.selectedMonth, this.selectedTimezone).subscribe({
      next: (res) => {
        this.report.set(res);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  onFilterChange(): void {
    this.loadTimesheet();
  }

  goToAdjustment(day: DailyTimeReport): void {
    this.router.navigate(['/ajustes'], {
      queryParams: { date: day.date }
    });
  }

  getPunchIcon(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn: return '🟢 Ent:';
      case TimeRecordType.LunchStart: return '☕ Int:';
      case TimeRecordType.LunchEnd: return '💼 Ret:';
      case TimeRecordType.ClockOut: return '🛑 Saí:';
      default: return '📍';
    }
  }

  formatRecordTime(rec: any): string {
    try {
      const date = new Date(rec.timestampUtc);
      // Se o usuário selecionou uma perspectiva específica no filtro do espelho, converte para ela;
      // senão, exibe no fuso de origem do registro
      const targetTz = this.selectedTimezone || rec.timezoneId || 'UTC';
      return date.toLocaleTimeString('pt-BR', {
        timeZone: targetTz,
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
      });
    } catch {
      const date = new Date(rec.timestampUtc);
      return date.toLocaleTimeString('pt-BR', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
      });
    }
  }
}

