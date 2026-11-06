# Conversational CV Builder — Start Here

## Project name

Conversational CV Builder

## Purpose

Create NTT DATA-branded CVs from uploaded PDF, DOCX, and PPTX resumes, voice input, and guided chat interaction.

## Outputs

- Branded PDF
- Branded DOCX (Word)
- JSON profile data through the API

## Current status

This is a working local, single-user MVP. The React workspace, document text extraction, browser voice transcription, guided CV conversation, NTT DATA preview, and PDF/DOCX export are implemented. Copilot-powered extraction and CV optimization require the GitHub Copilot CLI to be installed and signed in on the same machine as the API. Without Copilot, extracted and manually entered details remain available for editing.

The current workspace is held in memory and resets when the API restarts. SQLite repositories are configured for CV-related data, but the live workspace flow is not persisted.

## Features implemented

- Upload PDF, DOCX, and PPTX resumes (10 MB maximum).
- Extract readable document text and analyze CV fields.
- Record voice input and transcribe it in supported browsers.
- Build and edit profile details through chat.
- Generate an NTT DATA-themed CV preview and completion indicator.
- Export branded PDF and Word documents; retrieve profile JSON through the API.
- Run locally or with Docker Compose.
- Inspect API endpoints in Swagger during development.

## Project folder structure

```text
ConversationalCVBuilder/
├── backend/
│   ├── ConversationalCVBuilder.sln
│   └── src/
│       ├── ConversationalCVBuilder.Domain/
│       ├── ConversationalCVBuilder.Application/
│       ├── ConversationalCVBuilder.Infrastructure/
│       └── ConversationalCVBuilder.Api/
├── frontend/
│   ├── src/
│   ├── public/assets/
│   ├── package.json
│   └── package-lock.json
├── PROJECT_EXPORT/
├── docker-compose.yml
├── backend/Dockerfile
├── frontend/Dockerfile
└── README.md
```

## Quick start

Install Visual Studio Code, Git, the .NET 8 SDK, and Node.js LTS (Node 20 or newer). Then open the extracted project root in VS Code and use two terminals.

**Terminal 1 — backend (PowerShell):**

```powershell
cd backend
dotnet restore
dotnet build
cd src/ConversationalCVBuilder.Api
dotnet run
```

**Terminal 2 — frontend (PowerShell):**

```powershell
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`. See [SETUP_GUIDE.md](./SETUP_GUIDE.md) for Copilot authentication, HTTPS setup, and troubleshooting.
