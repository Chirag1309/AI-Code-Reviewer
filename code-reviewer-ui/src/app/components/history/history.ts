import { Component, OnInit } from '@angular/core';
import { ReviewApiService, ReviewRequest } from '../../services/review-api';

@Component({
  selector: 'app-history',
  standalone: false,
  templateUrl: './history.html',
  styleUrl: './history.scss'
})
export class History implements OnInit {
  reviews: ReviewRequest[] = [];
  isLoading = true;

  constructor(private reviewApi: ReviewApiService) {}

  ngOnInit() {
    this.reviewApi.getAllReviews().subscribe({
      next: (data) => {
        this.reviews = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load history', err);
        this.isLoading = false;
      }
    });
  }
}