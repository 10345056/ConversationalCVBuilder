# Installation Checklist

## 1. Install and verify prerequisites

- [ ] Install Visual Studio Code.
- [ ] Install Git for Windows.
- [ ] Install the .NET 8 SDK.
- [ ] Install Node.js LTS (20 or newer; npm is included).
- [ ] Optional: install and start Docker Desktop with Compose v2.
- [ ] Optional for Copilot AI: install GitHub Copilot CLI and sign in.

From PowerShell:

```powershell
dotnet --version
node -v
npm -v
git --version
```

## 2. Open the extracted project

- [ ] Extract the ZIP to a writable directory.
- [ ] Open the project root folder in VS Code.
- [ ] Open a PowerShell terminal from the project root.

## 3. Restore and build backend

```powershell
cd backend
dotnet restore
dotnet build
cd src/ConversationalCVBuilder.Api
dotnet run
```

- [ ] Confirm the API starts on `https://localhost:5001` and `http://localhost:5000`.
- [ ] Confirm `https://localhost:5001/health` responds with `{"status":"ok"}`.
- [ ] If needed, trust the local certificate with `dotnet dev-certs https --trust`.

## 4. Install and start frontend

In a second terminal from the project root:

```powershell
cd frontend
npm install
npm run dev
```

- [ ] Open `http://localhost:5173`.
- [ ] Confirm the workspace loads through the Vite API proxy.
- [ ] Upload a supported PDF, DOCX, or PPTX and test the manual workflow.
- [ ] Optional: sign in to Copilot CLI and verify AI-assisted features.
- [ ] Optional: test PDF and DOCX exports.

## 5. Optional Docker validation

From the project root with Docker running:

```powershell
docker compose up --build
```

- [ ] Open `http://localhost:3000`.
- [ ] Check `http://localhost:8080/health`.
- [ ] Check `http://localhost:8080/swagger`.
- [ ] Note: Docker Compose does not configure Copilot CLI authentication.

## 6. Before creating the handover ZIP

- [ ] Include source, manifests, lockfiles, Docker files, and `PROJECT_EXPORT/`.
- [ ] Exclude generated folders: `node_modules/`, `bin/`, `obj/`, `.vs/`, `dist/`, and `build/`.
- [ ] Exclude local databases, personal `.env.local` files, credentials, and machine-specific caches.
- [ ] Keep `frontend/package-lock.json`.
- [ ] Do not delete the developer's local generated folders; exclude them only from the distribution archive.
- [ ] Extract the final ZIP into a clean temporary folder and run the local restore/build steps above.
