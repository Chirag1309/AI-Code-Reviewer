import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ReviewApiService, ReviewResult as ReviewResultModel, ReviewResponse } from '../../services/review-api';

@Component({
  selector: 'app-review-result',
  standalone: false,
  templateUrl: './review-result.html',
  styleUrl: './review-result.scss'
})
export class ReviewResult implements OnInit, OnDestroy {
  status: string = 'Pending';
  result: ReviewResultModel | null = null;
  private pollInterval: any;
  private reviewId: string = '';

  constructor(
    private route: ActivatedRoute,
    private reviewApi: ReviewApiService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.reviewId = this.route.snapshot.paramMap.get('id')!;
    this.fetchResult();
    this.pollInterval = setInterval(() => this.fetchResult(), 3000);
  }

  ngOnDestroy() {
    if (this.pollInterval) clearInterval(this.pollInterval);
  }

  fetchResult() {
    this.reviewApi.getReview(this.reviewId).subscribe({
      next: (response: ReviewResponse) => {
        this.status = response.request.status;
        if (response.result) this.result = response.result;

        if (this.status === 'Completed' || this.status === 'Failed') {
          clearInterval(this.pollInterval);
        }

        this.cdr.detectChanges();
      },
      error: (err: any) => console.error('Poll failed', err)
    });
  }

  getScoreColorClass(): string {
    const score = this.result?.qualityScore ?? 0;
    if (score >= 80) return 'score-great';
    if (score >= 60) return 'score-good';
    if (score >= 40) return 'score-ok';
    return 'score-poor';
  }

  getSeverityStats() {
    const severities = ['Critical', 'High', 'Medium', 'Low'];
    return severities.map(s => ({
      severity: s,
      count: this.result?.issues.filter(i => i.severity === s).length ?? 0
    })).filter(s => s.count > 0);
  }
}