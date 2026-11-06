# Setup Guide

## Prerequisites

Install these on the developer machine:

- Visual Studio Code
- Git for Windows
- .NET 8 SDK
- Node.js LTS, version 20 or newer (npm is included)
- Optional: Docker Desktop with Docker Compose v2
- Optional for AI features: GitHub Copilot CLI and an account with Copilot access

Verify the required command-line tools from PowerShell:

```powershell
dotnet --version
node -v
npm -v
git --version
```

The .NET SDK should report an 8.0.x version. Node.js should be version 20 or newer.

## Extract and open the project

1. Extract the handover ZIP to a writable folder.
2. In VS Code, choose **File → Open Folder…** and select the extracted `ConversationalCVBuilder` root folder.
3. Open a PowerShell terminal in VS Code (**Terminal → New Terminal**).

Do not open only `frontend` or `backend` as the workspace root; Docker files, documentation, and solution are at the project root.

## Restore and build the backend

From the project root:

```powershell
cd backend
dotnet restore
dotnet build
```

Start the API in a terminal:

```powershell
cd backend/src/ConversationalCVBuilder.Api
dotnet run
```

The default `https` launch profile listens on `https://localhost:5001` and `http://localhost:5000`. The Vite development proxy targets the HTTPS endpoint.

If local HTTPS is not configured, trust the .NET development certificate from an elevated or regular PowerShell terminal as appropriate for the machine:

```powershell
dotnet dev-certs https --trust
```

Restart the API after trusting the certificate.

## Install and start the frontend

Open a second VS Code terminal from the project root:

```powershell
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`.

The checked-in `frontend/package-lock.json` records the frontend dependency resolution. For repeatable clean installs, use `npm ci` instead of `npm install`.

## Enable Copilot-powered AI

The backend uses the GitHub Copilot .NET SDK in Copilot CLI mode. It does not use an OpenAI API key. AI requests use the account signed in to the local Copilot CLI.

1. Install GitHub Copilot CLI using the official GitHub installation instructions.
2. Open a new PowerShell terminal and run `copilot`.
3. Follow the CLI sign-in flow (use `/login` when prompted).
4. Confirm the CLI is available with `copilot --version`.
5. Start or restart the API, then use an AI feature.

Copilot Free has usage limits. AI may be unavailable when the CLI is missing, signed out, blocked by organization policy, or out of quota. The app should retain extracted/manual data for editing. This local CLI integration is intended for a single-user development workspace; do not expose it as a shared production service.

## Configuration and secrets

Backend defaults are in `backend/src/ConversationalCVBuilder.Api/appsettings.json`. Copilot currently uses model `auto` and a 90-second timeout. .NET environment variables can override configuration using double underscores, for example:

```powershell
$env:Copilot__Model = "auto"
$env:Copilot__TimeoutSeconds = "90"
```

Do not put personal access tokens or other secrets in checked-in settings files or the handover ZIP. The normal Copilot CLI login is local to the developer account and should not be copied between machines.

## Optional configuration services

The API settings also include Ollama and Whisper endpoints. Defaults point to services on the API host (`localhost:11434` and `localhost:9000`). Those services must be installed and started separately if a workflow uses them. Browser-based voice input uses the Web Speech API when available and is not the same as server-side Whisper transcription.

## Troubleshooting

- **Frontend reports API HTTP 500 or cannot connect:** Verify `https://localhost:5001/health`, make sure the API is running with the `https` launch profile, and check the API terminal for the actual error. The frontend's Vite proxy points to this URL.
- **HTTPS browser warning:** Run `dotnet dev-certs https --trust`, restart the API, and reload the page.
- **AI unavailable:** Run `copilot --version`, complete CLI sign-in, and check Copilot account or organization access. Continue with manual editing if the account is unavailable or out of quota.
- **Port already in use:** Stop the other process using the port or adjust the backend launch profile and Vite proxy together.
- **Voice input unavailable:** Use a current Chrome or Edge browser, grant microphone permission, and check browser support for `SpeechRecognition`.
- **Missing restore/build tools:** Confirm the .NET 8 SDK and Node.js 20+ are installed, then rerun the verify commands above.

For known functional and deployment limits, see [KNOWN_ISSUES.md](./KNOWN_ISSUES.md).
