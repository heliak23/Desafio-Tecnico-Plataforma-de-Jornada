import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AdjustmentService } from '../../services/adjustment.service';
import { TimeRecordService } from '../../services/time-record.service';
import { AuthService } from '../../services/auth.service';
import { AdjustmentResponse, AdjustmentStatus, TimeRecordType, CreateAdjustmentRequest } from '../../models/types';

@Component({
  selector: 'app-ajustes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ajustes.html',
  styleUrl: './ajustes.css'
})
export class AjustesComponent implements OnInit {
  adjustments = signal<AdjustmentResponse[]>([]);
  isLoading = signal(false);
  isSubmitting = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  // Form Model
  targetDate: string = '';
  requestedType: TimeRecordType = TimeRecordType.ClockIn;
  requestedTime: string = '09:00';
  reason: string = '';
  timezoneId: string = '';

  constructor(
    public authService: AuthService,
    private adjustmentService: AdjustmentService,
    private timeRecordService: TimeRecordService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.timezoneId = this.timeRecordService.getBrowserTimezone();
    
    // Check if coming from espelho with pre-selected date
    this.route.queryParams.subscribe(params => {
      if (params['date']) {
        this.targetDate = params['date'];
      } else {
        this.targetDate = new Date().toISOString().split('T')[0];
      }
    });

    this.loadMyAdjustments();
  }

  loadMyAdjustments(): void {
    this.isLoading.set(true);
    this.adjustmentService.getMyAdjustments().subscribe({
      next: (list) => {
        this.adjustments.set(list);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onSubmit(): void {
    if (!this.targetDate || !this.requestedTime || !this.reason.trim()) {
      this.errorMessage.set('Por favor, preencha todos os campos obrigatórios e descreva o motivo.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const fullLocalDateTime = `${this.targetDate}T${this.requestedTime}:00`;

    const request: CreateAdjustmentRequest = {
      targetDate: this.targetDate,
      requestedType: Number(this.requestedType),
      requestedLocalTime: fullLocalDateTime,
      timezoneId: this.timezoneId,
      offsetMinutes: this.timeRecordService.getBrowserOffsetMinutes(),
      reason: this.reason.trim()
    };

    this.adjustmentService.create(request).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.successMessage.set('Solicitação de ajuste enviada para aprovação do gestor com sucesso!');
        this.reason = '';
        this.loadMyAdjustments();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message || 'Erro ao enviar a solicitação.');
      }
    });
  }

  getStatusBadgeClass(status: AdjustmentStatus): string {
    switch (status) {
      case AdjustmentStatus.Approved: return 'badge-success';
      case AdjustmentStatus.Rejected: return 'badge-danger';
      default: return 'badge-warning';
    }
  }

  getStatusText(status: AdjustmentStatus): string {
    switch (status) {
      case AdjustmentStatus.Approved: return 'Aprovado';
      case AdjustmentStatus.Rejected: return 'Recusado';
      default: return 'Pendente';
    }
  }

  getTypeLabel(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn: return 'Entrada';
      case TimeRecordType.LunchStart: return 'Início Intervalo';
      case TimeRecordType.LunchEnd: return 'Retorno Intervalo';
      case TimeRecordType.ClockOut: return 'Saída';
      default: return 'Batida';
    }
  }
}

