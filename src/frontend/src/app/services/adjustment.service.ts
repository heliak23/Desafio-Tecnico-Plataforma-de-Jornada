import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AdjustmentResponse,
  CreateAdjustmentRequest,
  ReviewAdjustmentRequest
} from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class AdjustmentService {
  private readonly apiUrl = 'http://localhost:5000/api/adjustments';

  constructor(private http: HttpClient) {}

  create(request: CreateAdjustmentRequest): Observable<AdjustmentResponse> {
    return this.http.post<AdjustmentResponse>(this.apiUrl, request);
  }

  getMyAdjustments(): Observable<AdjustmentResponse[]> {
    return this.http.get<AdjustmentResponse[]>(`${this.apiUrl}/my`);
  }

  getPending(): Observable<AdjustmentResponse[]> {
    return this.http.get<AdjustmentResponse[]>(`${this.apiUrl}/pending`);
  }

  review(id: string, request: ReviewAdjustmentRequest): Observable<AdjustmentResponse> {
    return this.http.put<AdjustmentResponse>(`${this.apiUrl}/${id}/review`, request);
  }
}

