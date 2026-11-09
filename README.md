# Conversational CV Builder

A local MVP for creating a CV through a guided conversation. The workspace uses React, TypeScript, Fluent UI, and Vite on the frontend, with an ASP.NET Core 8 API organized using Clean Architecture and the repository pattern.

## Project structure

```text
.
├── frontend/
│   ├── src/
│   │   ├── api/                 # API and export client
│   │   ├── components/          # Upload, conversation, and preview panels
│   │   ├── App.tsx
│   │   ├── main.tsx
│   │   ├── styles.css
│   │   └── types.ts
│   ├── package.json
│   ├── tsconfig.json
│   └── vite.config.ts
└── backend/
    ├── ConversationalCVBuilder.sln
    └── src/
        ├── ConversationalCVBuilder.Api/            # HTTP controllers and composition root
        ├── ConversationalCVBuilder.Application/    # Use cases, contracts, and interfaces
        ├── ConversationalCVBuilder.Domain/         # Workspace and CV entities
        └── ConversationalCVBuilder.Infrastructure/ # EF Core/SQLite repositories, exports, and document services
```

## Requirements

- Node.js 20 or newer (includes npm)
- .NET 8 SDK

## Run locally

Install the frontend dependencies once:

```powershell
cd frontend
npm install
```

Start the backend from the API project directory:

```powershell
cd backend/src/ConversationalCVBuilder.Api
dotnet run
```

In another terminal, start the frontend:

```powershell
cd frontend
npm run dev
```

Open the frontend at `http://localhost:5173`. The API listens at `https://localhost:5001` (and `http://localhost:5000`), Swagger is at `https://localhost:5001/swagger`, and the health check is at `https://localhost:5001/health`.

The Vite development server proxies `/api` requests to the HTTPS API, so the browser does not need to access the local API certificate directly. To call a different API origin directly, set `VITE_API_BASE_URL` in `frontend/.env.local`; `frontend/.env.example` shows the default same-origin configuration. If the development certificate is not trusted, run `dotnet dev-certs https --trust` and accept the operating system prompt.

## API endpoints

- `POST /api/documents/parse` — multipart form upload (`file`) for PDF, DOCX, PPTX, or TXT; returns extracted text, extraction status, and diagnostics. Unreadable or image-only documents return a partial result and a reviewable local draft rather than an extraction-related HTTP 400.
- `GET /api/workspace` — current CV profile, conversation, uploaded text, and AI analysis.
- `POST /api/workspace/new-session` — resets the in-memory workspace and starts a clean CV session. The frontend calls this on page load and provides a **Start new session** button.
- `POST /api/workspace/messages` — JSON `{ "message": "..." }`; continues the context-aware CV conversation.
- `PUT /api/workspace/profile` — validates and enhances the supplied CV profile with Copilot when configured.
- `PUT /api/workspace/profile/manual` — saves edits made directly in the CV preview without running profile enhancement.
- `POST /api/export/pdf` and `POST /api/export/docx` — optional JSON `{ "templateKey": "classic" }`; returns a branded file.
- `GET /api/workspace/templates` — list available templates.

The `.http` file in the API project contains request examples.

The SQLite database defaults to `conversational-cv-builder.db` in the API working directory. Generate and apply EF Core migrations before using the persisted repositories; database creation is not automatically forced at application startup.

AI enhancement runs when you select **Generate NTT CV** and uses the official GitHub Copilot .NET SDK and the locally signed-in Copilot CLI account. Uploading alone creates a local draft without spending Copilot credits. If Copilot is unavailable or out of quota, the app keeps the draft; inferred role content and sample project ideas are explicitly marked for review and should be replaced or confirmed before sharing. The local fallback does not invent employers, dates, certifications, or measured achievements.

This integration is intended for the existing local, single-user workspace. Do not expose this backend as a shared service: it uses the host's signed-in Copilot account and local CLI credentials. The API's development CORS policy permits `http://localhost:5173`.

## Run with Docker Compose

With Docker Desktop (or another Docker Compose v2 installation) running, execute this command from the repository root:

```powershell
docker compose up --build
```

Open the frontend at `http://localhost:3000`. The API is available at `http://localhost:8080`, with Swagger at `http://localhost:8080/swagger` and the health check at `http://localhost:8080/health`. The frontend's Nginx server forwards `/api` requests to the backend container.

The backend stores its SQLite database under `/data`; Compose mounts the named `sqlite-data` volume there so the data survives container restarts and rebuilds. The current workspace flow is in-memory and does not automatically create the database; the volume is ready for the configured EF Core repositories when database migrations are applied.

## MVP features

- Responsive three-panel workspace: document upload, guided chat, and live CV preview.
- PDF, DOCX, PPTX, and TXT text extraction (10 MB upload limit), with a local reviewable draft and optional Copilot-powered CV refinement.
- NTT DATA CV preview and ATS-friendly PDF/DOCX exports with consistent sections, bullet formatting, profile completeness, and a generated-content review notice.
- Context-aware CV conversation, one-at-a-time follow-up questions, manual editing, and AI-assisted CV enhancement.
- Classic, Modern, and Creative preview templates.
- NTT DATA-branded DOCX, PDF, and JSON exports from the API, including logo, generated date, and page numbers.
- CQRS application requests use MediatR with a FluentValidation pipeline for CV profile, conversation, document, template, and export operations.
- SQLite persistence is configured through EF Core with repositories for CV profiles, users, and conversations.
- Live voice recording and speech-to-text with the browser Web Speech API where supported (typically Chrome or Edge, with microphone permission). Audio is processed by the browser and is not uploaded or stored by this MVP.

## MVP boundaries

The existing conversational workspace remains an in-memory, single-user MVP; the new aggregate repositories use SQLite but have not yet replaced the workspace flow. No uploaded files or audio are persisted. Production deployment needs authenticated user-scoped application flows, database migrations, privacy controls, and configured AI/speech providers. QuestPDF is configured for its Community license; verify license eligibility for the deploying organization or configure a commercial license as required.

The existing CQRS `GenerateCv` command validates the draft and returns a structured CV snapshot. The workspace generation action uses the locally authenticated Copilot CLI when available, while the existing API PDF and DOCX exporters render the resulting profile in the NTT DATA template.
