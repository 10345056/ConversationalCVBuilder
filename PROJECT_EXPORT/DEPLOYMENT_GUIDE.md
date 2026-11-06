# Deployment Guide

## Local developer URLs

- Frontend: `http://localhost:5173`
- Backend API: `https://localhost:5001` (also listens at `http://localhost:5000`)
- Swagger UI: `https://localhost:5001/swagger`
- Health: `https://localhost:5001/health`

The API's `https` launch profile starts both HTTP and HTTPS. Vite proxies `/api` to `https://localhost:5001` with local proxy certificate validation disabled. Trust the ASP.NET Core development certificate if opening the API directly in a browser:

```powershell
dotnet dev-certs https --trust
```

## Local run

Start the API and frontend in separate terminals from the project root.

**Backend:**

```powershell
cd backend
dotnet restore
dotnet build
cd src/ConversationalCVBuilder.Api
dotnet run
```

**Frontend:**

```powershell
cd frontend
npm install
npm run dev
```

For complete onboarding steps, see [SETUP_GUIDE.md](./SETUP_GUIDE.md).

## Docker Compose

Requirements: Docker Desktop (or another Docker Compose v2 installation) must be installed and running.

From the project root:

```powershell
docker compose up --build
```

Docker URLs:

- Frontend: `http://localhost:3000`
- Backend: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`

The Compose file sets the API environment to `Development`, so Swagger is enabled. Change this before exposing a deployment to a network. The frontend Nginx container forwards `/api/` to the backend container. Compose mounts the named `sqlite-data` volume at `/data`.

## Configuration and security notes

- Local Copilot AI depends on the signed-in Copilot CLI. Docker Compose does not configure CLI credentials or AI access; do not assume AI features work in the container.
- The current workspace is a single-user in-memory state store; restarting/redeploying the API clears that workspace.
- Do not deploy the local Copilot CLI account integration as a shared multi-user service.
- Review CORS, authentication, user isolation, data retention, and secrets handling before any non-local deployment.
- HTTPS termination and production secrets management are not configured by the local Compose setup.
- Verify QuestPDF license eligibility for the deployment organization.
- EF Core SQLite repositories are configured, but migrations and active workspace persistence are not included in the current flow.
