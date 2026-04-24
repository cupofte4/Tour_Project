# AGENTS.md

```text
You are working inside a mono-repo for a tourism guide app.

Current focus:
- Ignore `src/mobile/` and all .NET MAUI work unless the user explicitly asks for it.
- Active work areas are `backend/`, `frontend/`, and `database/`.

Current stack in active scope:
- Backend: ASP.NET Core Web API
- Frontend: React + Vite
- ORM: EF Core
- Database used by current repo wiring: MySQL
- Container/dev infra: Docker Compose

Project status for GPT:
- Backend API is already implemented with controllers for auth, locations, analytics, manager flows, tours, sessions, uploads, and users.
- EF Core setup and migrations already exist under `backend/Data` and `backend/Migrations`.
- The backend currently uses Pomelo MySQL provider and a MySQL connection in code/docker files.
- Frontend is an active React app with pages for home, login, register, favorites, admin dashboard, manager dashboard, tours, and location management.
- Admin flow exists for managing users, locations, assignments, analytics, tours, and uploads.
- Manager flow exists for managing assigned locations and viewing statistics.
- Database artifacts currently live in `database/`, with `foodguide.sql` as the main SQL file.
- There are build artifacts checked into the repo (`frontend/dist`, `frontend/node_modules`, `backend/bin_build`, `backend/build-check`), but they are not primary source folders.

Working assumptions:
- Prefer editing only source files in `backend/`, `frontend/src/`, `frontend/public/`, `database/`, and root infra files such as `docker-compose.yml`.
- Do not treat `src/mobile/` as part of the requested scope.
- Do not refactor unrelated files.
- Keep changes minimal and compilable.
- Prefer stubs/fakes over inventing full external integrations.
- If a dependency or interface is missing, add the minimum required version.

Workdir structure to understand before making changes:
- `AGENTS.md`: repo-specific instructions for coding agents
- `plans.md`: product/implementation notes; may be outdated compared with the live repo
- `docker-compose.yml`: local database container setup
- `docker-compose.ec2.yml`: deployment-oriented multi-service compose file
- `backend/`: ASP.NET Core API project
  - `Program.cs`: app startup, DI, auth, CORS, DB wiring
  - `Controllers/`: API endpoints
  - `Data/`: `AppDbContext`
  - `Models/`: EF entities and request models
  - `Services/`: application services
  - `Application/`: request/DTO/service contracts
  - `Migrations/`: EF Core migrations
  - `appsettings*.json`: runtime config
  - `Dockerfile`: backend container build
- `frontend/`: React + Vite web app
  - `src/pages/`: routed pages
  - `src/components/`: shared UI parts
  - `src/services/`: API/data access helpers
  - `src/hooks/`: custom hooks
  - `src/styles/`: CSS files
  - `public/`: static assets
  - `Dockerfile`: frontend container build
  - `nginx.conf`: frontend container web server config
- `database/`: SQL/bootstrap database assets
- `docs/`: supporting documentation
- `src/mobile/`: ignore for current tasks unless explicitly requested

Rules:
- Implement only the requested slice.
- Stay within backend, frontend, and database unless the user expands scope.
- Explain every modified file.
- Include exact run/test commands in the final response.
- If something in `plans.md` conflicts with the live codebase, trust the live codebase first.
```
