import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ReviewRequest {
  id: string;
  code: string;
  language: string;
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed';
  createdAt: string;
}

export interface ReviewIssue {
  id: string;
  category: string;
  severity: 'Critical' | 'High' | 'Medium' | 'Low';
  description: string;
  lineNumber?: number;
  suggestion?: string;
}

export interface ReviewResult {
  id: string;
  reviewRequestId: string;
  summary: string;
  qualityScore: number;
  issues: ReviewIssue[];
  completedAt: string;
}

export interface ReviewResponse {
  request: ReviewRequest;
  result: ReviewResult | null;
}

@Injectable({ providedIn: 'root' })
export class ReviewApiService {
  private baseUrl = 'http://localhost:5110/api/review';

  constructor(private http: HttpClient) {}

  submitReview(code: string, language: string): Observable<ReviewRequest> {
    return this.http.post<ReviewRequest>(this.baseUrl, { code, language });
  }

  getReview(id: string): Observable<ReviewResponse> {
    return this.http.get<ReviewResponse>(`${this.baseUrl}/${id}`);
  }

  getAllReviews(): Observable<ReviewRequest[]> {
    return this.http.get<ReviewRequest[]>(this.baseUrl);
  }
}