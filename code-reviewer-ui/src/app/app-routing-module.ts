import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SubmitReview } from './components/submit-review/submit-review';
import { ReviewResult } from './components/review-result/review-result';
import { History } from './components/history/history';

const routes: Routes = [
  { path: '', redirectTo: 'submit', pathMatch: 'full' },
  { path: 'submit', component: SubmitReview },
  { path: 'review/:id', component: ReviewResult },
  { path: 'history', component: History }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }