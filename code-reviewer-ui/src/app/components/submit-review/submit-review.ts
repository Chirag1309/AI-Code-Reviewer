import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewApiService } from '../../services/review-api';

@Component({
  selector: 'app-submit-review',
  standalone: false,
  templateUrl: './submit-review.html',
  styleUrl: './submit-review.scss'
})
export class SubmitReview {
  code = '';
  language = 'csharp';
  isLoading = false;
  submitted = false;

  constructor(
    private reviewApi: ReviewApiService,
    private router: Router
  ) {}

  submitReview() {
    this.submitted = true;

    if (!this.code.trim()) return;

    this.isLoading = true;

    this.reviewApi.submitReview(this.code, this.language).subscribe({
      next: (request) => {
        this.router.navigate(['/review', request.id]);
      },
      error: (err) => {
        console.error('Submit failed', err);
        this.isLoading = false;
        alert('Failed to submit review. Is the API running on port 5110?');
      }
    });
  }
}