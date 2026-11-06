# Project History

This handover records the project as currently implemented; it does not prescribe or perform business-functionality changes.

## Implemented capabilities

- **Resume upload:** PDF, DOCX, and PPTX upload with a 10 MB limit.
- **Document parsing:** extract readable text from supported documents and provide an extraction summary.
- **NTT DATA CV generation:** structured profile, guided chat, CV completion indicator, NTT DATA-themed preview, and AI-assisted optimization when Copilot is available.
- **PDF export:** branded PDF through QuestPDF.
- **DOCX export:** branded Word document through DocumentFormat.OpenXml.
- **JSON output:** profile/workspace JSON export endpoint.
- **SQLite:** EF Core SQLite provider and repositories configured for CV-related aggregates.
- **Swagger:** API OpenAPI/Swagger UI in Development.
- **Docker support:** Dockerfiles and Compose configuration for frontend, API, and a named SQLite volume.
- **NTT DATA theme:** logo asset, light-blue/secondary-blue visual styling, and branded export layout.
- **Voice and chat:** typed conversation and browser-based voice recording/transcription where supported.
- **Copilot SDK:** replaced the earlier OpenAI API-key approach with local GitHub Copilot CLI authentication for development.

## Architecture and delivery notes

- Backend uses ASP.NET Core 8 and Clean Architecture project boundaries.
- Application use cases use CQRS with MediatR and FluentValidation.
- The current workspace is an in-memory local MVP; SQLite repositories are configured but are not the persistence mechanism for the active workspace flow.
- Local AI depends on the developer's Copilot CLI installation, login, plan access, and quota.
- No functional changes are made by this documentation export.
