import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ClockPunchRequest,
  TimeRecordResponse,
  DailyTimeReport,
  MonthlyTimesheetReport,
  TimeRecordType
} from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class TimeRecordService {
  private readonly apiUrl = 'http://localhost:5000/api/time-records';

  constructor(private http: HttpClient) {}

  /**
   * Obtém o identificador IANA do fuso do navegador (ex: "America/Sao_Paulo", "Europe/Lisbon")
   */
  getBrowserTimezone(): string {
    try {
      return Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC';
    } catch {
      return 'UTC';
    }
  }

  /**
   * Obtém o deslocamento atual em minutos em relação ao UTC
   */
  getBrowserOffsetMinutes(): number {
    return -new Date().getTimezoneOffset();
  }

  /**
   * Bate ponto enviando os metadados de fuso e horário local (automático ou selecionado),
   * além do horário simulado opcional para modo de testes/avaliação.
   */
  punch(
    locationNotes?: string,
    type?: TimeRecordType,
    timezoneId?: string,
    offsetMinutes?: number,
    simulatedTimestampUtc?: string
  ): Observable<TimeRecordResponse> {
    const request: ClockPunchRequest = {
      type: type,
      timezoneId: timezoneId || this.getBrowserTimezone(),
      offsetMinutes: offsetMinutes !== undefined ? offsetMinutes : this.getBrowserOffsetMinutes(),
      locationNotes: locationNotes,
      simulatedTimestampUtc: simulatedTimestampUtc
    };

    return this.http.post<TimeRecordResponse>(`${this.apiUrl}/punch`, request);
  }

  getDaily(date?: string, timezoneId?: string): Observable<DailyTimeReport> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);
    params = params.set('timezoneId', timezoneId || this.getBrowserTimezone());

    return this.http.get<DailyTimeReport>(`${this.apiUrl}/daily`, { params });
  }

  getMonthly(year?: number, month?: number, timezoneId?: string): Observable<MonthlyTimesheetReport> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    if (month) params = params.set('month', month.toString());
    params = params.set('timezoneId', timezoneId || this.getBrowserTimezone());

    return this.http.get<MonthlyTimesheetReport>(`${this.apiUrl}/monthly`, { params });
  }

  getNextExpected(simulatedTimeUtc?: string, timezoneId?: string): Observable<{ type: TimeRecordType; description: string }> {
    let params = new HttpParams();
    if (simulatedTimeUtc) params = params.set('simulatedTimeUtc', simulatedTimeUtc);
    if (timezoneId) params = params.set('timezoneId', timezoneId);

    return this.http.get<{ type: TimeRecordType; description: string }>(`${this.apiUrl}/next-expected`, { params });
  }

  getLastPunch(): Observable<TimeRecordResponse | null> {
    return this.http.get<TimeRecordResponse | null>(`${this.apiUrl}/last`);
  }
}

