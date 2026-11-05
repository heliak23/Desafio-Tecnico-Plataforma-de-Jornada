import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdjustmentService } from '../../services/adjustment.service';
import { EmployeeService } from '../../services/employee.service';
import { AuthService } from '../../services/auth.service';
import { AdjustmentResponse, AdjustmentStatus, User, TimeRecordType, MonthlyTimesheetReport } from '../../models/types';

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

  viewEmployeeTimesheet(employee: User): void {
    this.isLoadingEmployeeReport.set(true);
    const now = new Date();

    this.employeeService.getEmployeeMonthly(employee.id, now.getFullYear(), now.getMonth() + 1).subscribe({
      next: (report) => {
        this.selectedEmployeeReport = report;
        this.isLoadingEmployeeReport.set(false);
      },
      error: () => {
        this.isLoadingEmployeeReport.set(false);
      }
    });
  }

  closeEmployeeTimesheetModal(): void {
    this.selectedEmployeeReport = null;
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
}

