# SimpleSurveys - Application Specification

## Overview

SimpleSurveys is a lightweight web application for creating and managing surveys. It allows administrators to create surveys with multiple options (dates or text), set deadlines, and configure selection modes. Regular users can vote on active surveys, with cookie-based duplicate prevention.

---

## Technologies

| Layer | Technology |
|-------|------------|
| Backend | .NET 9 with Minimal API |
| Frontend | Angular 21 |
| Storage | SQLite with Entity Framework Core |
| Authentication | Cookie-based (admin password, voter tracking) |

### Why SQLite?
- Zero configuration, file-based database
- Cloud-agnostic (works anywhere)
- No external dependencies
- Easy to backup (single file)
- Sufficient for small-to-medium survey loads

---

## Data Model

### Survey
| Field | Type | Description |
|-------|------|-------------|
| Id | GUID | Unique identifier |
| Title | string | Survey question/title |
| Description | string? | Optional description |
| SelectionMode | enum | `Single` or `Multiple` |
| Deadline | DateTime | UTC deadline for voting |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last update timestamp |

### SurveyOption
| Field | Type | Description |
|-------|------|-------------|
| Id | GUID | Unique identifier |
| SurveyId | GUID | Parent survey reference |
| OptionType | enum | `Text` or `Date` |
| TextValue | string? | Text option value |
| DateValue | DateTime? | Date option value |
| DisplayOrder | int | Order in the list |

### Vote
| Field | Type | Description |
|-------|------|-------------|
| Id | GUID | Unique identifier |
| SurveyId | GUID | Survey reference |
| OptionId | GUID | Selected option reference |
| VoterToken | string | Cookie-based voter identifier |
| VoterName | string? | Name entered by the voter |
| CreatedAt | DateTime | Vote timestamp |

---

## API Endpoints

### Public Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/surveys/{id}` | Get survey details with options and vote counts |
| POST | `/api/surveys/{id}/vote` | Submit vote(s) for a survey |
| GET | `/api/surveys/{id}/my-votes` | Get current user's votes (by cookie) |

### Admin Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/surveys` | List all surveys |
| GET | `/api/admin/surveys/{id}` | Get survey with full details |
| GET | `/api/admin/surveys/{id}/voters` | Get list of voters with their choices |
| POST | `/api/admin/surveys` | Create new survey |
| PUT | `/api/admin/surveys/{id}` | Update survey |
| DELETE | `/api/admin/surveys/{id}` | Delete survey |
| POST | `/api/admin/login` | Admin login |
| POST | `/api/admin/logout` | Admin logout |
| GET | `/api/admin/check` | Check admin session |

### Request/Response Examples

#### Create Survey Request
```json
{
  "title": "When should we have the team meeting?",
  "description": "Please select your preferred dates",
  "selectionMode": "Multiple",
  "deadline": "2024-12-31T23:59:59Z",
  "options": [
    { "optionType": "Date", "dateValue": "2024-12-15T10:00:00Z" },
    { "optionType": "Date", "dateValue": "2024-12-16T10:00:00Z" },
    { "optionType": "Text", "textValue": "Any day works for me" }
  ]
}
```

#### Survey Response (Public)
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "When should we have the team meeting?",
  "description": "Please select your preferred dates",
  "selectionMode": "Multiple",
  "deadline": "2024-12-31T23:59:59Z",
  "isActive": true,
  "options": [
    {
      "id": "...",
      "optionType": "Date",
      "dateValue": "2024-12-15T10:00:00Z",
      "displayText": "December 15, 2024 at 10:00 AM",
      "voteCount": 5,
      "isWinner": false
    }
  ],
  "totalVotes": 12,
  "userHasVoted": true,
  "currentVoterName": "John Doe"
}
```

#### Vote Request
```json
{
  "optionIds": ["option-guid-1", "option-guid-2"],
  "voterName": "John Doe"
}
```

#### Voters Response (Admin)
```json
{
  "surveyId": "550e8400-e29b-41d4-a716-446655440000",
  "voters": [
    {
      "voterName": "John Doe",
      "selectedOptions": ["December 15, 2024 at 10:00 AM", "Any day works for me"],
      "votedAt": "2024-12-10T14:30:00Z"
    }
  ]
}
```

---

## Screens

### 1. Public Survey View (`/survey/{id}`)

**Purpose:** Allow users to view and vote on a survey.

**Components:**
- Survey title and description
- Deadline indicator (countdown or "Closed")
- List of options with:
  - Radio buttons (single select) or checkboxes (multiple select)
  - Current vote counts (always visible)
  - Visual indicator for winning option(s) if deadline passed
- Voter name input field (required before submitting)
- Submit/Update vote button (disabled if deadline passed or name empty)
- "You have already voted" indicator if applicable

**Privacy:** Vote counts are visible to all users, but voter names are only visible to admins.

**States:**
- **Active + Not Voted:** Show voting form with name input
- **Active + Voted:** Show current selection and name with option to change
- **Closed:** Show results only, highlight winner(s)

### 2. Admin Login (`/admin/login`)

**Purpose:** Simple password-based admin authentication.

**Components:**
- Password input field
- Login button
- Error message display

**Behavior:**
- On successful login, redirect to admin dashboard
- Store admin session in HTTP-only cookie

### 3. Admin Dashboard (`/admin`)

**Purpose:** List and manage all surveys.

**Components:**
- Header with "Create New Survey" button and logout
- Survey list table with columns:
  - Title
  - Status (Active/Closed)
  - Deadline
  - Total votes
  - Actions (Edit, Delete, Copy Link)
- Search/filter input
- Empty state message

**Actions:**
- Click row: Navigate to edit
- Delete: Confirmation dialog
- Copy Link: Copy public survey URL to clipboard

### 4. Admin Survey Editor (`/admin/survey/new` | `/admin/survey/{id}`)

**Purpose:** Create or edit a survey.

**Components:**
- Title input (required)
- Description textarea (optional)
- Selection mode toggle (Single/Multiple)
- Deadline date-time picker
- Options list:
  - Option type selector (Text/Date)
  - Value input (text field or date picker)
  - Delete option button
  - Reorder buttons (up/down)
- "Add Option" button
- Voters list (edit mode only):
  - Voter name
  - Selected options
  - Vote timestamp
- Save/Cancel buttons

**Validation:**
- Title required
- At least 2 options required
- Deadline must be in the future (for new surveys)
- Options must have values

---

## User Flows

### Flow 1: User Votes on Survey
```
1. User receives survey link
2. Opens /survey/{id}
3. Views survey question and options
4. Selects option(s)
5. Enters their name
6. Clicks "Submit Vote"
7. System stores vote with name + sets cookie
8. User sees confirmation with current results
```

### Flow 2: User Changes Vote
```
1. User returns to survey link
2. System recognizes cookie
3. Shows current selection
4. User modifies selection
5. Clicks "Update Vote"
6. System updates vote record
```

### Flow 3: Admin Creates Survey
```
1. Admin logs in at /admin/login
2. Clicks "Create New Survey"
3. Fills in survey details
4. Adds options
5. Sets deadline
6. Clicks "Save"
7. System creates survey
8. Admin copies link to share
```

---

## Security Considerations

### Voter Identity
- Generate unique token per browser (stored in cookie)
- Cookie: `voter_token`, HttpOnly, SameSite=Strict, 1 year expiry
- One vote set per token per survey

### Admin Authentication
- Single admin password (configured in appsettings)
- Password hashed with bcrypt
- Session cookie: `admin_session`, HttpOnly, SameSite=Strict

### API Protection
- Admin endpoints require valid admin session
- Rate limiting on vote endpoints (prevent abuse)
- CORS configured for frontend origin only

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=surveys.db"
  },
  "AdminPassword": "change-me-in-production",
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200"]
  }
}
```

---

## Project Structure

```
SimpleSurveys/
├── backend/
│   ├── SimpleSurveys.Api/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Migrations/
│   │   ├── Models/
│   │   │   ├── Survey.cs
│   │   │   ├── SurveyOption.cs
│   │   │   └── Vote.cs
│   │   ├── Endpoints/
│   │   │   ├── PublicEndpoints.cs
│   │   │   └── AdminEndpoints.cs
│   │   └── Services/
│   │       └── SurveyService.cs
│   └── SimpleSurveys.Api.sln
├── frontend/
│   └── simple-surveys/
│       ├── src/
│       │   ├── app/
│       │   │   ├── components/
│       │   │   │   ├── survey-view/
│       │   │   │   ├── admin-login/
│       │   │   │   ├── admin-dashboard/
│       │   │   │   └── survey-editor/
│       │   │   ├── services/
│       │   │   │   ├── survey.service.ts
│       │   │   │   └── admin.service.ts
│       │   │   ├── models/
│       │   │   │   └── survey.models.ts
│       │   │   └── guards/
│       │   │       └── admin.guard.ts
│       │   └── environments/
│       └── angular.json
├── SPEC.md
└── README.md
```

---

## Development Phases

### Phase 1: Backend Core
- Set up .NET project with Minimal API
- Configure SQLite + EF Core
- Implement data models and migrations
- Create public survey endpoints
- Create admin endpoints

### Phase 2: Frontend Core
- Set up Angular project
- Implement survey view component
- Implement voting functionality
- Implement admin login

### Phase 3: Admin Features
- Admin dashboard with survey list
- Survey editor (create/edit)
- Delete confirmation

### Phase 4: Polish
- Responsive design
- Error handling
- Loading states
- Form validation
- Winner highlighting
