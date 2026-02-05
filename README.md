# SimpleSurveys

A lightweight survey application built with .NET 9 Minimal API and Angular 21.

## Features

- Create surveys with text or date options
- Single or multiple choice selection modes
- Deadline-based voting with automatic closure
- Cookie-based voter tracking (prevents duplicate votes)
- Voter name collection
- Admin dashboard for survey management
- Winner highlighting after deadline

## Tech Stack

- **Backend:** .NET 9, Minimal API, Entity Framework Core, SQLite
- **Frontend:** Angular 21, TypeScript, SCSS

## Getting Started

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- npm

### Backend

```bash
cd backend/SimpleSurveys.Api
dotnet run
```

API runs at: http://localhost:5226
Swagger UI: http://localhost:5226/swagger

### Frontend

```bash
cd frontend/simple-surveys
npm install
npm start
```

App runs at: http://localhost:4200

## Default Admin Credentials

Password: `admin123` (configurable in `appsettings.json`)

## Project Structure

```
SimpleSurveys/
├── backend/
│   └── SimpleSurveys.Api/     # .NET Minimal API
├── frontend/
│   └── simple-surveys/        # Angular app
├── SPEC.md                    # Detailed specification
└── README.md
```

## API Endpoints

### Public
- `GET /api/surveys/{id}` - Get survey with options
- `POST /api/surveys/{id}/vote` - Submit vote
- `GET /api/surveys/{id}/my-votes` - Get current user's votes

### Admin (requires authentication)
- `POST /api/admin/login` - Login
- `GET /api/admin/surveys` - List all surveys
- `POST /api/admin/surveys` - Create survey
- `PUT /api/admin/surveys/{id}` - Update survey
- `DELETE /api/admin/surveys/{id}` - Delete survey
- `GET /api/admin/surveys/{id}/voters` - Get voter details
