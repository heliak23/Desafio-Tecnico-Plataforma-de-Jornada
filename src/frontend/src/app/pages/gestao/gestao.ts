import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdjustmentService } from '../../services/adjustment.service';
import { EmployeeService } from '../../services/employee.service';
import { AuthService } from '../../services/auth.service';
import { AdjustmentResponse, AdjustmentStatus, User, TimeRecordType, MonthlyTimesheetReport, TeamMonthlySummaryReport } from '../../models/types';

@Component({
  selector: 'app-gestao',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './gestao.html',
  styleUrl: './gestao.css'
})
export class GestaoComponent implements OnInit {
  pendingAdjustments = signal<AdjustmentResponse[]>([]);
  employees = signal<User[]>([]);
  isLoading = signal(false);

  // Resumo de Fechamento Mensal da Equipe (RH / Gestor)
  teamSummary = signal<TeamMonthlySummaryReport | null>(null);
  isLoadingSummary = signal(false);
  selectedYear: number = new Date().getFullYear();
  selectedMonth: number = new Date().getMonth() + 1;

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

  // Modal / Review state
  selectedAdjustment: AdjustmentResponse | null = null;
  reviewNotes: string = '';
  isProcessing = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  // Employee timesheet view
  selectedEmployeeReport: MonthlyTimesheetReport | null = null;
  isLoadingEmployeeReport = signal(false);

  constructor(
    public authService: AuthService,
    private adjustmentService: AdjustmentService,
    private employeeService: EmployeeService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    // Carrega solicitações pendentes
    this.adjustmentService.getPending().subscribe({
      next: (list) => {
        this.pendingAdjustments.set(list);
      },
      error: () => {}
    });

    // Carrega colaboradores da equipe / empresa
    this.employeeService.getEmployees().subscribe({
      next: (list) => {
        this.employees.set(list);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });

    // Carrega o fechamento consolidado da equipe se for RH
    if (this.authService.isHR()) {
      this.loadTeamSummary();
    }
  }

  get currentMonthName(): string {
    const m = this.months.find(x => x.value === Number(this.selectedMonth));
    return m ? m.name : '';
  }

  loadTeamSummary(): void {
    this.isLoadingSummary.set(true);
    const yr = Number(this.selectedYear);
    const mo = Number(this.selectedMonth);

    this.employeeService.getTeamMonthlySummary(yr, mo).subscribe({
      next: (summary) => {
        this.teamSummary.set(summary);
        this.isLoadingSummary.set(false);
      },
      error: () => this.isLoadingSummary.set(false)
    });
  }

  onMonthChange(newMonth: number | string): void {
    this.selectedMonth = Number(newMonth);
    this.loadTeamSummary();
  }

  onYearChange(newYear: number | string): void {
    this.selectedYear = Number(newYear);
    this.loadTeamSummary();
  }

  onFilterSummaryChange(): void {
    this.loadTeamSummary();
  }

  printTeamReport(): void {
    window.print();
  }

  printEmployeeReport(): void {
    window.print();
  }

  openReviewModal(adjustment: AdjustmentResponse): void {
    this.selectedAdjustment = adjustment;
    this.reviewNotes = '';
  }

  closeReviewModal(): void {
    this.selectedAdjustment = null;
    this.reviewNotes = '';
  }

  submitReview(status: AdjustmentStatus): void {
    if (!this.selectedAdjustment) return;

    this.isProcessing.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adjustmentService.review(this.selectedAdjustment.id, {
      status: status,
      reviewNotes: this.reviewNotes.trim() || (status === AdjustmentStatus.Approved ? 'Aprovado pelo gestor.' : 'Recusado pelo gestor.')
    }).subscribe({
      next: () => {
        this.isProcessing.set(false);
        const action = status === AdjustmentStatus.Approved ? 'aprovada' : 'recusada';
        this.successMessage.set(`Solicitação ${action} com sucesso! O ponto foi atualizado.`);
        this.closeReviewModal();
        this.loadData();

        setTimeout(() => this.successMessage.set(null), 5000);
      },
      error: (err) => {
        this.isProcessing.set(false);
        this.errorMessage.set(err.error?.message || 'Erro ao processar a avaliação.');
      }
    });
  }

  viewEmployeeTimesheet(employee: User, year: number = this.selectedYear, month: number = this.selectedMonth): void {
    this.isLoadingEmployeeReport.set(true);
    const yr = Number(year);
    const mo = Number(month);

    this.employeeService.getEmployeeMonthly(employee.id, yr, mo).subscribe({
      next: (report) => {
        this.selectedEmployeeReport = report;
        this.isLoadingEmployeeReport.set(false);
      },
      error: () => {
        this.isLoadingEmployeeReport.set(false);
      }
    });
  }

  viewEmployeeById(userId: string): void {
    const yr = Number(this.selectedYear);
    const mo = Number(this.selectedMonth);
    const emp = this.employees().find(e => e.id === userId);

    if (emp) {
      this.viewEmployeeTimesheet(emp, yr, mo);
    } else {
      this.isLoadingEmployeeReport.set(true);
      this.employeeService.getEmployeeMonthly(userId, yr, mo).subscribe({
        next: (report) => {
          this.selectedEmployeeReport = report;
          this.isLoadingEmployeeReport.set(false);
        },
        error: () => this.isLoadingEmployeeReport.set(false)
      });
    }
  }

  closeEmployeeTimesheetModal(): void {
    this.selectedEmployeeReport = null;
  }

  onResetDatabase(): void {
    if (!confirm('Deseja realmente zerar o banco de dados e restaurar o estado padrão de fábrica?')) {
      return;
    }

    this.isProcessing.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.authService.resetDatabase().subscribe({
      next: (res) => {
        this.isProcessing.set(false);
        this.successMessage.set(res.message);
        this.loadData();
        setTimeout(() => this.successMessage.set(null), 5000);
      },
      error: () => {
        this.isProcessing.set(false);
        this.errorMessage.set('Erro ao resetar o banco de dados.');
      }
    });
  }

  getTypeLabel(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn: return '🟢 Entrada';
      case TimeRecordType.LunchStart: return '☕ Início Intervalo';
      case TimeRecordType.LunchEnd: return '💼 Retorno Intervalo';
      case TimeRecordType.ClockOut: return '🛑 Saída';
      default: return 'Batida';
    }
  }

  getPunchTypeName(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn: return 'Entrada';
      case TimeRecordType.LunchStart: return 'Almoço';
      case TimeRecordType.LunchEnd: return 'Retorno';
      case TimeRecordType.ClockOut: return 'Saída';
      default: return 'Ponto';
    }
  }

  get currentDateString(): string {
    return new Date().toLocaleDateString('pt-BR');
  }

  formatTimeInOrigin(record: any): string {
    try {
      const date = new Date(record.timestampUtc);
      return date.toLocaleTimeString('pt-BR', {
        timeZone: record.timezoneId || 'UTC',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
      });
    } catch {
      const date = new Date(record.timestampUtc);
      return date.toLocaleTimeString('pt-BR', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
      });
    }
  }

  getInitials(name?: string): string {
    if (!name) return 'DD';
    const parts = name.trim().split(' ').filter(Boolean);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }
}
