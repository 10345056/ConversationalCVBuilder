# Project Architecture

## System overview

The application has a React single-page frontend and an ASP.NET Core Web API. The frontend calls the API through Vite's `/api` development proxy. The API delegates use cases to the Application layer and infrastructure services.

```text
Developer browser
  └── React + TypeScript + Fluent UI + Vite (localhost:5173)
        └── /api requests → Vite proxy → ASP.NET Core API (localhost:5001 HTTPS)
              ├── API: controllers, middleware, composition root
              └── Application: MediatR requests/handlers, contracts, services
                    ├── Domain: workspace, CV entities, value objects
                    └── Infrastructure: AI, document parsing, exports, repositories
                          └── SQLite repositories (configured; not the active
                              persistence path for the in-memory workspace flow)
```

## Frontend

- React and TypeScript provide the interactive, responsive CV workspace.
- Fluent UI provides controls and layout components.
- Vite provides the development server and proxies `/api` calls to the local API.
- Framer Motion provides landing-page, upload, and preview animations.
- Browser `SpeechRecognition` / `webkitSpeechRecognition` provides voice input where supported.
- `frontend/src/api/client.ts` owns API calls, workspace normalization, and file downloads.
- `frontend/src/components/` contains the upload, conversation, and preview panels.

## Backend

The backend targets ASP.NET Core 8 and is organized using Clean Architecture:

- **Domain** — CV profile, user, workspace, conversation, and value objects; business invariants.
- **Application** — use-case requests, handlers, validation, contracts, service abstractions, and workspace orchestration.
- **Infrastructure** — implementations for Copilot AI, document text extraction, PDF/DOCX export, EF Core/SQLite repositories, and optional service clients.
- **API** — HTTP controllers, request contracts, dependency injection composition, CORS, Swagger, health endpoint, and exception middleware.

## Patterns

- **CQRS:** application commands and queries represent use cases.
- **MediatR:** dispatches application requests to their handlers.
- **Repository pattern:** abstracts persistence for CV, user, and conversation aggregates.
- **Dependency injection:** API composition registers application and infrastructure services.
- **Service layer:** workspace and conversation workflows coordinate domain operations.

## Main request flows

### Resume upload and generation

```text
PDF/DOCX/PPTX upload
 → API document endpoint
 → document parser extracts text
 → Copilot AI service extracts/analyzes structured profile (when available)
 → workspace and conversation state updated
 → Generate CV sends profile + extracted source + chat history for optimization
 → React preview / PDF / DOCX
```

### Chat and voice

```text
Typed answer or browser voice transcript
 → POST /api/workspace/messages
 → conversation handler combines profile, resume source, and conversation history
 → Copilot applies answer and returns updated profile and one follow-up (when available)
 → preview is refreshed
```

### Export

```text
POST /api/export/pdf  → QuestPDF branded document
POST /api/export/docx → Open XML Word document
GET  /api/export/json → JSON workspace/profile download
```

## Workspace and persistence boundary

The active `Workspace` is held by an in-memory singleton repository and is shared by the local API process. It is not scoped per user and resets when the API restarts. EF Core and SQLite repositories are configured for persisted CV-related aggregates, but migrations are not currently present and the live workspace workflow does not use those repositories.

## Docker topology

Docker Compose starts the ASP.NET Core API on port 8080 and a built React frontend served by Nginx on port 3000. Nginx forwards `/api/` requests to the `backend` service. A named SQLite volume is mounted at `/data`.

The Docker setup does not configure Copilot CLI authentication. The local Copilot integration depends on a signed-in CLI account and is not automatically available inside the backend container.
