import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { SubmitReview } from './components/submit-review/submit-review';
import { ReviewResult } from './components/review-result/review-result';
import { History } from './components/history/history';

@NgModule({
  declarations: [App, SubmitReview, ReviewResult, History],
  imports: [
    BrowserModule,
    CommonModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule
  ],
  bootstrap: [App]
})
export class AppModule { }