# Known Issues and Open Items

## Current limitations

- **Single-user workspace:** the active workspace is an in-memory singleton, not user-scoped, and resets when the API process restarts.
- **Persistence is incomplete:** EF Core SQLite repositories are configured, but there are no checked-in migrations and the active workspace workflow does not use them.
- **Copilot requires local authentication:** AI features depend on an installed, signed-in Copilot CLI and available account access/quota. The integration is intended for local, single-user development, not shared hosting.
- **Container AI setup is incomplete:** Compose does not provide Copilot CLI authentication or securely pass credentials into the backend container.
- **Voice compatibility:** voice input depends on browser Web Speech API support and microphone permission; behavior varies by browser and operating system.
- **Optional AI services:** Ollama and Whisper settings point to local services that must be installed and run separately if needed.
- **No production deployment profile:** Compose currently sets the API environment to Development; review Swagger exposure, HTTPS, CORS, authentication, and secrets before network deployment.
- **License review:** QuestPDF uses the Community license configuration; confirm the deploying organization's eligibility.
- **Extraction variability:** document parsers depend on readable text in PDFs and correctly formed Office documents. Scanned/image-only documents may yield no usable text without OCR.
- **Frontend bundle notice:** production Vite builds currently report a JavaScript chunk over 500 kB. The build succeeds; code-splitting is a future optimization.

## Missing or incomplete capabilities

- Authentication, authorization, tenant/user isolation, and secure shared hosting.
- Persisted workspace and conversation flow, database migrations, and automated database initialization.
- OCR for scanned resumes.
- Reliable cross-browser voice transcription and server-side Whisper workflow integration.
- Automated end-to-end UI tests and a dedicated backend test suite.
- Production-grade operational monitoring, backup/restore, and deployment secrets management.
- Consistent app-level retries and actionable diagnostics for Copilot account/quota failures.

## Recommended future enhancements

1. Add user authentication and scope all profiles and conversations to a user.
2. Persist the workspace flow, add EF Core migrations, and test upgrades/backup/restore.
3. Decide on a supported production AI provider and authentication model; keep local Copilot CLI usage development-only.
4. Add OCR and document-size/format test fixtures.
5. Add backend unit/integration tests and frontend end-to-end tests for upload, chat, generation, and exports.
6. Prepare production Docker settings (non-Development environment, HTTPS/reverse proxy, secrets, health checks, and logging).
7. Split/lazy-load frontend bundles and add accessibility/browser coverage.
8. Review privacy, retention, license, and security requirements for resume data.

## Open items before a production release

- Choose and document the intended production hosting model.
- Decide which AI, voice transcription, and OCR providers are supported in production.
- Define retention/deletion policy for personal resume data.
- Confirm QuestPDF license eligibility.
- Implement and verify durable, user-scoped persistence and migrations.
- Complete a security/privacy review and release validation.
