# Technology Stack

Versions below are defined by the project manifests; the accompanying package export files contain the package-manager output captured during handover.

## Frontend

- React 18
- TypeScript 5
- Fluent UI React Components v9 and Fluent UI React Icons
- Vite 5 with the React plugin
- Framer Motion
- Browser Web Speech API for speech recognition where supported

See [frontend-packages.txt](./frontend-packages.txt) for `npm list --depth=0`.

## Backend

- .NET 8 / ASP.NET Core Web API
- MediatR for CQRS request dispatch
- FluentValidation and its dependency-injection integration
- Swashbuckle.AspNetCore for Swagger/OpenAPI
- Entity Framework Core SQLite provider for configured repositories
- Microsoft.Extensions configuration binding and HTTP client packages

See [backend-packages.txt](./backend-packages.txt) for `dotnet list package` output.

## Database

- SQLite, configured through Entity Framework Core.
- The default connection string uses `conversational-cv-builder.db`.
- Compose configures the database path under `/data` and mounts a named volume.
- The active workspace remains in memory. No EF Core migrations are currently checked in, and database creation is not automatically run at startup.

## AI integration

- GitHub Copilot .NET SDK (`GitHub.Copilot.SDK`).
- Copilot CLI mode uses the locally signed-in GitHub Copilot account and model `auto`.
- No OpenAI API key is required.
- AI operations include structured resume extraction, gap/follow-up analysis, conversation updates, and CV optimization.
- Copilot CLI authentication and quota are developer-machine/account dependencies. Docker Compose does not mount or configure them.
- Ollama and Whisper clients/options are also registered/configured for optional local services; defaults are `http://localhost:11434/` and `http://localhost:9000/`.

## Export libraries

- QuestPDF generates PDF output and is configured for the Community license. Verify that the license is appropriate for the deploying organization.
- DocumentFormat.OpenXml generates DOCX output.

## Document parsing libraries

- PdfPig extracts text from PDFs.
- DocumentFormat.OpenXml reads DOCX/PPTX package contents.

## Build and deployment

- npm and Vite for frontend install/build/dev server.
- `dotnet restore`, `dotnet build`, and `dotnet run` for the backend.
- Docker Compose v2 for containerized local startup.
- Nginx serves the frontend container and proxies API calls.
