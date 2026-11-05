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

  // Simulador de Avanço de Horário (Time Travel)
  readonly simulatedOffsetMinutes = signal<number>(0);

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
    const effectiveNow = new Date(Date.now() + this.simulatedOffsetMinutes() * 60 * 1000);
    const tz = this.getEffectiveTimezone();
    this.timezone.set(tz);

    try {
      this.currentTime.set(effectiveNow.toLocaleTimeString('pt-BR', { timeZone: tz, hour12: false }));
      this.currentDate.set(
        effectiveNow.toLocaleDateString('pt-BR', {
          timeZone: tz,
          weekday: 'long',
          day: '2-digit',
          month: 'long',
          year: 'numeric'
        })
      );
    } catch {
      this.currentTime.set(effectiveNow.toLocaleTimeString('pt-BR', { hour12: false }));
    }
  }

  private calculateOffsetMinutesForTimezone(tz: string): number {
    try {
      const now = new Date(Date.now() + this.simulatedOffsetMinutes() * 60 * 1000);
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

  // --- Funções do Simulador de Horário (Time Travel) ---
  advanceMinutes(minutes: number): void {
    this.simulatedOffsetMinutes.update(current => current + minutes);
    this.updateClock();
    this.loadTodayData();
  }

  resetSimulatedTime(): void {
    this.simulatedOffsetMinutes.set(0);
    this.updateClock();
    this.loadTodayData();
  }

  jumpToHour(targetHour: number): void {
    const tz = this.getEffectiveTimezone();
    const now = new Date();
    
    // Obtém hora e minuto atual no fuso selecionado
    const parts = new Intl.DateTimeFormat('en-US', {
      timeZone: tz,
      hour: 'numeric',
      minute: 'numeric',
      hour12: false
    }).formatToParts(now);

    const curHour = Number(parts.find(p => p.type === 'hour')?.value ?? 0);
    const curMinute = Number(parts.find(p => p.type === 'minute')?.value ?? 0);

    const currentTotalMinutes = curHour * 60 + curMinute;
    const targetTotalMinutes = targetHour * 60;

    let delta = targetTotalMinutes - currentTotalMinutes;
    // Se o horário alvo for menor que a hora atual, pula para o mesmo horário amanhã (+ 24h)
    if (delta < 0) {
      delta += 24 * 60;
    }

    this.simulatedOffsetMinutes.set(delta);
    this.updateClock();
    this.loadTodayData();
  }

  getSimulatedOffsetLabel(): string {
    const offset = this.simulatedOffsetMinutes();
    if (offset === 0) return 'Horário Real';
    const sign = offset > 0 ? '+' : '-';
    const abs = Math.abs(offset);
    const hours = Math.floor(abs / 60);
    const minutes = abs % 60;

    if (hours > 0 && minutes > 0) return `${sign}${hours}h ${minutes}min`;
    if (hours > 0) return `${sign}${hours}h`;
    return `${sign}${minutes}min`;
  }

  loadTodayData(): void {
    const tz = this.getEffectiveTimezone();
    const effectiveNow = new Date(Date.now() + this.simulatedOffsetMinutes() * 60 * 1000);
    
    // Formata YYYY-MM-DD no fuso selecionado
    const dateFormatted = new Intl.DateTimeFormat('en-CA', { timeZone: tz }).format(effectiveNow);

    this.timeRecordService.getDaily(dateFormatted, tz).subscribe({
      next: (report) => this.dailyReport.set(report),
      error: () => {}
    });

    const simTimeUtc = this.simulatedOffsetMinutes() !== 0 ? effectiveNow.toISOString() : undefined;
    this.timeRecordService.getNextExpected(simTimeUtc, tz).subscribe({
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

    const simDate = this.simulatedOffsetMinutes() !== 0 
      ? new Date(Date.now() + this.simulatedOffsetMinutes() * 60 * 1000).toISOString()
      : undefined;

    this.timeRecordService.punch(this.locationNotes, typeToSend, tz, offset, simDate).subscribe({
      next: (res) => {
        this.isPunching.set(false);
        const typeName = this.getPunchName(res.type);
        const simSuffix = this.simulatedOffsetMinutes() !== 0 ? ` (Horário Simulado: ${this.currentTime()})` : '';
        this.successMessage.set(`Ponto registrado com sucesso: ${typeName}${simSuffix}!`);
        this.showCustomTypeSelect = false;
        this.loadTodayData();

        setTimeout(() => this.successMessage.set(null), 6000);
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

  getActivePunchType(): TimeRecordType {
    if (this.showCustomTypeSelect && this.selectedCustomType) {
      return this.selectedCustomType;
    }
    return this.nextPunchType();
  }

  getActivePunchName(): string {
    return this.getPunchName(this.getActivePunchType());
  }

  getActivePunchIcon(): string {
    return this.getPunchIcon(this.getActivePunchType());
  }

  toggleCustomTypeSelect(): void {
    this.showCustomTypeSelect = !this.showCustomTypeSelect;
    if (this.showCustomTypeSelect && !this.selectedCustomType) {
      this.selectedCustomType = this.nextPunchType();
    } else if (!this.showCustomTypeSelect) {
      this.selectedCustomType = undefined;
    }
  }

  selectCustomType(type: TimeRecordType): void {
    this.selectedCustomType = type;
  }

  formatRecordTime(rec: any): string {
    try {
      const date = new Date(rec.timestampUtc);
      return date.toLocaleTimeString('pt-BR', {
        timeZone: rec.timezoneId || 'UTC',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false
      });
    } catch {
      const date = new Date(rec.timestampUtc);
      return date.toLocaleTimeString('pt-BR', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false
      });
    }
  }

  isDifferentFromCurrentTimezone(recTz: string): boolean {
    const currentTz = this.getEffectiveTimezone();
    return !!recTz && !!currentTz && recTz !== currentTz;
  }

  formatConvertedTime(rec: any): string {
    try {
      const date = new Date(rec.timestampUtc);
      const currentTz = this.getEffectiveTimezone();
      return date.toLocaleTimeString('pt-BR', {
        timeZone: currentTz,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false
      });
    } catch {
      return '';
    }
  }
}
