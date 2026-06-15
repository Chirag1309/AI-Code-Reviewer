# 🤖 AI Code Reviewer

An async, AI-powered code review platform built with .NET 8, Angular, and Google Gemini. Submit code and get instant, structured feedback on security, performance, bugs, and best practices — categorized by severity with actionable suggestions.

![Status](https://img.shields.io/badge/status-active-success)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Angular](https://img.shields.io/badge/Angular-19-DD0031)

---

## 🎯 Overview

Paste a code snippet, select a language, and get back:
- An overall quality score (0-100)
- A summary of the code's health
- A list of issues categorized by severity (Critical / High / Medium / Low) and type (Security, Performance, Bug, Maintainability, Best Practice)
- Actionable suggestions for each issue

The review happens asynchronously — submission returns instantly, while a background worker processes the AI request, so the UI never blocks.

---

## 🏗️ Architecture

This project follows Clean Architecture, with dependencies pointing strictly inward toward the domain core.
┌──────────────────────────────────────────────────────┐

│              API  +  Worker   (Presentation)          │

│      Controllers · BackgroundService · Program.cs     │

└──────────────────────┬─────────────────────────────────┘

│ depends on

┌──────────────────────▼─────────────────────────────────┐

│                Infrastructure                          │

│   DbContext · ReviewService · GeminiReviewService      │

└──────────────────────┬─────────────────────────────────┘

│ depends on

┌──────────────────────▼─────────────────────────────────┐

│                    Core                                │

│        Models · Interfaces — zero dependencies         │

└──────────────────────────────────────────────────────┘
### Async Request Flow
Client          API              DB           Worker        Gemini AI

│──POST /review──▶│              │               │             │

│◀──202 + ID──────│              │               │             │

│                 │──save Pending──▶             │             │

│                 │              │◀──poll every 3s─            │

│                 │              │               │──call API───▶│

│                 │              │               │◀─structured──│

│                 │              │◀──save Result──│   review    │

│──GET /review/id─▶│             │               │             │

│◀──200 + result──│              │               │             │

---

## ✨ Features

- Async job processing — 202 Accepted pattern decouples request from AI latency (2–10s)
- Database-backed FIFO queue — BackgroundService polls for pending jobs
- Fault-tolerant AI integration — retry logic with exponential backoff on 503 errors, defensive JSON parsing for truncated/malformed responses
- Structured AI output — prompt-engineered to return consistent, categorized JSON
- Real-time polling UI — Angular dashboard updates live as reviews complete
- Review history — all past submissions with status tracking
- EF Core migrations — version-controlled database schema

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8 Web API, BackgroundService |
| Database | SQL Server + Entity Framework Core |
| AI | Google Gemini 2.5 Flash |
| Frontend | Angular 19, SCSS |
| Architecture | Clean Architecture (Core / Infrastructure / API / Worker) |

---

## 📁 Project Structure
ai-code-reviewer/

├── CodeReviewer.Core/            # Domain models & interfaces (zero dependencies)

├── CodeReviewer.Infrastructure/  # EF Core, DbContext, Gemini integration

├── CodeReviewer.API/             # REST API — Controllers, DI setup

├── CodeReviewer.Worker/          # Background job processor

└── code-reviewer-ui/             # Angular frontend
---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB works for development)
- Node.js 18+ and Angular CLI
- A free Gemini API key from https://aistudio.google.com/apikey

### 1. Clone the repo
```bash
git clone https://github.com/Chirag1309/AI-Code-Reviewer.git
cd AI-Code-Reviewer
```

### 2. Configure your API key

Add your Gemini API key to both:
- CodeReviewer.API/appsettings.json
- CodeReviewer.Worker/appsettings.json

```json
"Gemini": {
    "ApiKey": "your-gemini-api-key-here"
}
```

### 3. Set up the database
```bash
dotnet ef database update --project CodeReviewer.Infrastructure --startup-project CodeReviewer.API
```

### 4. Run the backend
```bash
# Terminal 1
cd CodeReviewer.API
dotnet run

# Terminal 2
cd CodeReviewer.Worker
dotnet run
```

### 5. Run the frontend
```bash
cd code-reviewer-ui
npm install
ng serve
```

Open http://localhost:4200

---

## 🔌 API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | /api/review | Submit code for review → returns 202 Accepted with job ID |
| GET | /api/review | List all review requests |
| GET | /api/review/{id} | Get status + result for a specific review |

---

## 🧠 Design Decisions

**Why async (202 Accepted) instead of synchronous?**

AI review calls take 2–10 seconds. Returning 202 immediately keeps the API responsive and decouples client wait-time from AI latency — the client polls for the result.

**Why DB polling instead of a message queue?**

For this scale, the database acts as a simple FIFO queue (ORDER BY CreatedAt). In production, this would be replaced with Azure Service Bus for push-based delivery and horizontal worker scaling.

**Why IServiceScopeFactory in the Worker?**

BackgroundService is a singleton, but DbContext is scoped. Creating a new DI scope per polling iteration avoids captive dependencies and ensures a fresh DbContext each cycle.

---

## 🔮 Future Improvements

- [ ] Replace DB polling with Azure Service Bus
- [ ] Add Redis caching for GET /api/review/{id}
- [ ] JWT authentication + per-user review history
- [ ] GitHub PR webhook integration for real diff reviews
- [ ] Polly-based retry policies for all external calls

---

