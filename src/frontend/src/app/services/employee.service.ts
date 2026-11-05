import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User, MonthlyTimesheetReport, TeamMonthlySummaryReport } from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly apiUrl = 'http://localhost:5000/api/employees';

  constructor(private http: HttpClient) {}

  getEmployees(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  getEmployeeMonthly(id: string, year?: number, month?: number, timezoneId?: string): Observable<MonthlyTimesheetReport> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    if (month) params = params.set('month', month.toString());
    if (timezoneId) params = params.set('timezoneId', timezoneId);

    return this.http.get<MonthlyTimesheetReport>(`${this.apiUrl}/${id}/monthly`, { params });
  }

  getTeamMonthlySummary(year?: number, month?: number): Observable<TeamMonthlySummaryReport> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    if (month) params = params.set('month', month.toString());

    return this.http.get<TeamMonthlySummaryReport>(`${this.apiUrl}/monthly-summary`, { params });
  }
}

