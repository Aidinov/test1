# Design Docs MVP

Monorepository for a minimal design document service.

## Structure
- `backend/` - ASP.NET Core minimal API
- `frontend/` - React 18 app (Vite)
- `ops/` - deployment and environment files

## Setup
1. Copy `ops/.env.example` to `.env` and adjust values. In GitLab, open **User Settings → Access Tokens**, create a token with scopes `api`, `read_repository`, and `write_repository`, and set the value in `GITLAB_PERSONAL_TOKEN`.
2. In the GitLab project, go to **Settings → Webhooks**, add a new webhook pointing to `POST /api/gitlab/webhook`, enable **Push events**, and set the secret token to `GITLAB_WEBHOOK_SECRET`.
3. Ensure a Git repository with markdown docs exists at `GITLAB_PROJECT_HTTP_URL`.

## Local Run with Docker
```
cd ops
cp .env.example .env
docker compose up --build
```

The API will be available at `http://localhost:8090`, the frontend at `http://localhost:5173`.

## Backend Development
Requires .NET 8 SDK.
```
cd backend/DesignDocs.Api
dotnet ef migrations add Init
.dotnet ef database update
```

## Frontend Development
Requires Node.js 18+
```
cd frontend
npm install
npm run dev
```

## Testing
Backend: `dotnet test`
Frontend: `npm test`
