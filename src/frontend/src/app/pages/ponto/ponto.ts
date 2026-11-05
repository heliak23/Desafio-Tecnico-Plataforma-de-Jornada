import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TimeRecordService } from '../../services/time-record.service';
import { AuthService } from '../../services/auth.service';
import { DailyTimeReport, TimeRecordType } from '../../models/types';

export interface LocationPreset {
  id: string;
  label: string;
  timezoneId: string;
  defaultNote: string;
}

@Component({
  selector: 'app-ponto',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ponto.html',
  styleUrl: './ponto.css'
})
export class PontoComponent implements OnInit, OnDestroy {
  readonly currentTime = signal('');
  readonly currentDate = signal('');
  readonly timezone = signal('');
  readonly offsetString = signal('');

  dailyReport = signal<DailyTimeReport | null>(null);
  nextPunchType = signal<TimeRecordType>(TimeRecordType.ClockIn);
  nextPunchDescription = signal<string>('Entrada');

  locationNotes = '';
  selectedCustomType?: TimeRecordType;
  showCustomTypeSelect = false;

  // Seletor de Localidade / Viagem
  locationPresets: LocationPreset[] = [
    { id: 'AUTO', label: '🌐 Automático (Detectado pelo Navegador)', timezoneId: '', defaultNote: '' },
    { id: 'BR_SP', label: '🇧🇷 Brasil — São Paulo (UTC-3)', timezoneId: 'America/Sao_Paulo', defaultNote: 'Escritório São Paulo' },
    { id: 'PT_LIS', label: '🇵🇹 Portugal — Lisboa (UTC+0 / UTC+1)', timezoneId: 'Europe/Lisbon', defaultNote: 'Hub Lisboa (Viagem Internacional)' },
    { id: 'ES_MAD', label: '🇪🇸 Espanha — Madrid (UTC+1 / UTC+2)', timezoneId: 'Europe/Madrid', defaultNote: 'Hub Madrid (Cliente/Evento)' },
    { id: 'GB_LON', label: '🇬🇧 Reino Unido — Londres (UTC+0 / UTC+1)', timezoneId: 'Europe/London', defaultNote: 'Hub Londres (Reunião Internacional)' }
  ];
  selectedLocationPreset = signal('AUTO');

  isPunching = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  private timerInterval?: any;

  constructor(
    public authService: AuthService,
    private timeRecordService: TimeRecordService
  ) {}

  ngOnInit(): void {
    this.updateClock();
    this.timerInterval = setInterval(() => this.updateClock(), 1000);

    this.formatOffset();
    this.loadTodayData();
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  getEffectiveTimezone(): string {
    const selected = this.selectedLocationPreset();
    const preset = this.locationPresets.find(p => p.id === selected);
    if (preset && preset.timezoneId) {
      return preset.timezoneId;
    }
    return this.timeRecordService.getBrowserTimezone();
  }

  onLocationPresetChange(presetId: string): void {
    this.selectedLocationPreset.set(presetId);
    const preset = this.locationPresets.find(p => p.id === presetId);
    
    // Sugere nota padrão se o usuário ainda não tiver preenchido ou se for uma nota de outro preset
    if (preset) {
      if (preset.defaultNote) {
        this.locationNotes = preset.defaultNote;
      } else if (this.locationPresets.some(p => p.defaultNote === this.locationNotes)) {
        this.locationNotes = '';
      }
    }

    this.formatOffset();
    this.updateClock();
    this.loadTodayData();
  }

  private updateClock(): void {
    const now = new Date();
    const tz = this.getEffectiveTimezone();
    this.timezone.set(tz);

    try {
      this.currentTime.set(now.toLocaleTimeString('pt-BR', { timeZone: tz, hour12: false }));
      this.currentDate.set(
        now.toLocaleDateString('pt-BR', {
          timeZone: tz,
          weekday: 'long',
          day: '2-digit',
          month: 'long',
          year: 'numeric'
        })
      );
    } catch {
      this.currentTime.set(now.toLocaleTimeString('pt-BR', { hour12: false }));
    }
  }

  private calculateOffsetMinutesForTimezone(tz: string): number {
    try {
      const now = new Date();
      const localStr = now.toLocaleString('en-US', { timeZone: tz });
      const utcStr = now.toLocaleString('en-US', { timeZone: 'UTC' });
      return Math.round((new Date(localStr).getTime() - new Date(utcStr).getTime()) / 60000);
    } catch {
      return this.timeRecordService.getBrowserOffsetMinutes();
    }
  }

  private formatOffset(): void {
    const tz = this.getEffectiveTimezone();
    const offset = this.calculateOffsetMinutesForTimezone(tz);
    const sign = offset >= 0 ? '+' : '-';
    const hours = Math.floor(Math.abs(offset) / 60);
    const mins = Math.abs(offset) % 60;
    this.offsetString.set(`UTC${sign}${hours.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}`);
  }

  loadTodayData(): void {
    const tz = this.getEffectiveTimezone();
    this.timeRecordService.getDaily(undefined, tz).subscribe({
      next: (report) => this.dailyReport.set(report),
      error: () => {}
    });

    this.timeRecordService.getNextExpected().subscribe({
      next: (res) => {
        this.nextPunchType.set(res.type);
        this.nextPunchDescription.set(res.description);
      },
      error: () => {}
    });
  }

  onPunch(): void {
    this.isPunching.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const typeToSend = this.showCustomTypeSelect ? this.selectedCustomType : undefined;
    const tz = this.getEffectiveTimezone();
    const offset = this.calculateOffsetMinutesForTimezone(tz);

    this.timeRecordService.punch(this.locationNotes, typeToSend, tz, offset).subscribe({
      next: (res) => {
        this.isPunching.set(false);
        const typeName = this.getPunchName(res.type);
        this.successMessage.set(`Ponto registrado com sucesso: ${typeName}!`);
        this.showCustomTypeSelect = false;
        this.loadTodayData();

        setTimeout(() => this.successMessage.set(null), 5000);
      },
      error: (err) => {
        this.isPunching.set(false);
        this.errorMessage.set(err.error?.message || 'Erro ao registrar o ponto. Tente novamente.');
      }
    });
  }

  getPunchName(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn: return 'Entrada';
      case TimeRecordType.LunchStart: return 'Início de Intervalo';
      case TimeRecordType.LunchEnd: return 'Retorno de Intervalo';
      case TimeRecordType.ClockOut: return 'Saída';
      default: return 'Registro';
    }
  }

  getPunchIcon(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn: return '🟢';
      case TimeRecordType.LunchStart: return '☕';
      case TimeRecordType.LunchEnd: return '💼';
      case TimeRecordType.ClockOut: return '🛑';
      default: return '📍';
    }
  }
}
