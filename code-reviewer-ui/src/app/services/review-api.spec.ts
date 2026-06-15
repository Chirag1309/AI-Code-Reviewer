import { TestBed } from '@angular/core/testing';

import { ReviewApi } from './review-api';

describe('ReviewApi', () => {
  let service: ReviewApi;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ReviewApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
