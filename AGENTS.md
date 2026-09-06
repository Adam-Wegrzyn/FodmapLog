# AGENTS.md

Persistent instructions for AI agents working on **FodmapLog** (HealthyGutLog) — a health/symptom tracking MVP.

## Project overview

Angular 17 SPA + ASP.NET Core 9 monolith, deployed to Azure App Service. Voice → transcription (Azure Function + Speech SDK) → OpenAI extraction → structured meal/symptom logs displayed on a mobile-first daily calendar.

**Solution:** `FodmapLog.sln`

| Project | Purpose |
|---------|---------|
| `FodmapLog.Server` | Web host, controllers, auth, static SPA |
| `fodmaplog.client` | Angular frontend |
| `Core` | Services, MediatR handlers, AutoMapper |
| `DataAccess` | EF Core, entities, migrations, repositories |
| `Data.Common` | DTOs (`*Dto`) |
| `TranscribeAudio` | Azure Function — base64 WAV → Azure Speech |
| `Tests` | xUnit (minimal today) |

## Architecture (do not replace)

```
Angular (fodmaplog.client)
  → HTTP + JWT (auth.interceptor.ts)
  → FodmapLog.Server controllers
  → Core services / MediatR
  → DataAccess repositories
  → SQL Server (FodmapLogDbContext)
  → Azure Function (transcription), OpenAI API, Key Vault
```

- **Keep the monolith + Azure Function split.** Transcription stays in `TranscribeAudio`; OpenAI stays server-side.
- **Do not reintroduce Azure Service Bus** — meal notifications via Service Bus were removed as unused.
- **Do not rewrite** working voice, transcription, LLM, or CRUD flows unless fixing a concrete bug or security gap.
- **Extend** existing layers; prefer minimal diffs over new abstractions.

## .NET conventions

### Layering

- **Controllers** (`FodmapLog.Server/Controllers/`, plus `AudioTranscriptionController.cs`): HTTP only; delegate to services or MediatR.
- **Services** (`Core/Services/`, `Core/Interfaces/I*Service.cs`): business logic, DTO mapping, external HTTP (e.g. `AudioTranscriptionService`).
- **Repositories** (`DataAccess/Repositories/`, `DataAccess/Interfaces/I*Repository.cs`): EF Core data access only.
- **Entities** (`DataAccess/Entities/`): persistence model; inherit `BaseEntity` (`Id`).
- **DTOs** (`Data.Common/DTO/`): API contracts; inherit `BaseDto`; suffix `Dto`.

### Patterns in use

- **Repository:** `IFodmapLogRepository` → `FodmapLogRepository` with `CancellationToken` on async methods.
- **Service:** `IFodmapLogService` → `FodmapLogService`; uses `IMapper`, `IFodmapLogRepository`.
- **MediatR CQRS** (reference-data reads, auth): `GetSymptomTypesQuery` + `GetSymptomTypesQueryHandler` in `Core/CQRS/`; commands/handlers under `FodmapLog.Server/Commands/` and `Handlers/`.
- **AutoMapper:** profiles in `Core/mapperConfig.cs` (`CreateMap<Entity, Dto>().ReverseMap()`).
- **DI registration:** `Program.cs` — `AddScoped` for repository/service; `AddMediatR`; `AddAutoMapper`.

### Naming

- **Namespaces:** match folder (`Core.Services`, `DataAccess.Entities`, `Data.Common.DTO`).
- **Interfaces:** `I` prefix (`IFodmapLogService`).
- **Controllers:** `{Name}Controller`, route `[Route("api/[controller]")]`.
- **Actions:** existing code uses camelCase route segments (e.g. `getMealLogById/{id}`, `addMealLog`) — **match this style** when adding endpoints.
- **Target frameworks:** Server/Core/DataAccess = `net9.0`; Tests = `net8.0`.

### Configuration & secrets

- Production secrets via **Azure Key Vault** (`KeyVaultName` in `appsettings.json`, loaded in `Program.cs`).
- Local dev: User Secrets (`UserSecretsId` in `FodmapLog.Server.csproj`, `FodmapLogDbContextFactory.cs`).
- **Never commit** API keys, JWT keys, or connection strings. Remove accidental logging of secrets (see `TranscribeAudio/Function1.cs` — do not log `AzureSpeechApiKey`).

## Angular conventions

### Structure

- **Module-based app:** `AppModule` declares components; `AppRoutingModule` defines routes.
- **Feature folders:** `src/app/{feature}/` with `.component.ts/html/css` (+ `.spec.ts`).
- **Domain models:** `src/app/domain/` (`MealLog`, `SymptomsLog`, `DailyLog`, etc.).
- **Services:** `src/app/services/`, `@Injectable({ providedIn: 'root' })`.
- **Environments:** `src/environments/environment.ts` / `environment.prod.ts` — all API base URLs live here.

### Naming (as in repo)

- **Selectors:** `app-kebab-case` (e.g. `app-daily-log`).
- **Files:** kebab-case folders; service files use `-service.ts` (e.g. `fodmap-log-service.ts`, `openAi-service.ts`).
- **Note:** some component **classes** use lowercase camelCase (`addMealLogComponent`) — follow the naming of the file you are editing.

### Patterns in use

- **HTTP:** `HttpClient` in services; URLs from `environment`.
- **Interceptors:** functional interceptors in `auth.interceptor.ts` (Bearer token from `localStorage`) and `error.interceptor.ts`.
- **Auth guard:** `AuthGuard.ts` — checks `localStorage.getItem('token')`.
- **Forms:** Reactive forms (`FormBuilder`, `FormArray`) in add-meal/symptoms components.
- **Transfer services:** `MealLogTransferService`, `SymptomsLogTransferService` pass pending AI edits between routes.
- **Standalone exception:** `DateTimeInputComponent` is `standalone: true` and imported into `AppModule`.

### UI stack

- Bootstrap 5, Font Awesome, `ngx-material-timepicker`, Angular Material theme (purple-green).
- Global styles: `src/styles.css`; component styles in `.css` files.

## Domain concepts

| Concept | Meaning |
|---------|---------|
| **MealLog** | A logged meal at a `Date` with `ProductQuantity[]` (product name, quantity, unit). |
| **SymptomsLog** | A log entry at a `Date` with `Symptoms[]` (symptom type + scale). |
| **DailyLog** | Combined day view: one entry is either a meal or symptoms (`mealLog` or `symptomsLog`). Built server-side in `FodmapLogService.GetDailyLogsByDate`. |
| **DailyLogUI** | Frontend extension with `isPending` / `isEditing` for AI-generated entries awaiting review. |
| **SymptomType** | Reference data (21 seeded in `FodmapLogDbContext`). |
| **Unit** | Reference data (21 seeded). |
| **Product** | Free-text food name (not a full nutrition catalog today). |

**Symptom scale (known inconsistency — fix carefully, do not invent a third scheme):**

- Frontend enum `SymptomScale` (`domain/SymptomScale.ts`): 0–5 (`None` … `Serious`).
- Backend enum `DataAccess.Enums.SymptomScale`: 1–6 (`Great` … `Awful`).
- OpenAI prompt in `OpenAIController.cs`: asks for 0–10.

Align these deliberately when touching scale logic; do not assume they match.

## API conventions

- **Base path:** `/api/{ControllerName}/...`
- **Auth:** JWT Bearer; issued by `JwtTokenService`, login at `POST /api/Auth/login`; Identity register at `POST /register` (`MapIdentityApi`).
- **Existing endpoints (representative):**
  - `FodmapLogController`: `getDailyLogsByDate/{date}`, `addMealLog`, `updateMealLog`, `addSymptomsLog`, …
  - `OpenAIController`: `POST /api/OpenAI/GeneratemealLogFromAI`
  - `AudioTranscriptionController`: `POST /api/AudioTranscription/transcribe`
  - `SymptomTypesController`, `UnitsController`: `GET` lists
- **Request/response:** JSON; server uses DTOs; dates as ISO strings/datetimes.
- **Cancellation:** pass `CancellationToken` through service/repository calls (existing pattern).

When adding endpoints, follow existing camelCase action routes and DTO shapes in `Data.Common/DTO/`.

## Testing commands

```bash
# Backend (from repo root)
dotnet test Tests/Tests.csproj

# Frontend (from fodmaplog.client/)
npm test          # runs ng test (Karma + Jasmine)

# Build
dotnet build FodmapLog.sln
cd fodmaplog.client && npm run build
```

Tests are mostly boilerplate today. Add **behavioral** tests for auth scoping, LLM JSON parsing, and critical flows — not trivial "should be created" specs.

## Rules against unnecessary rewrites

1. **Do not replace** Angular with another framework, or .NET with another backend.
2. **Do not migrate** NgModule → standalone across the app unless explicitly requested.
3. **Do not introduce** new state libraries, ORMs, or API styles if the existing service/repository/MediatR pattern suffices.
4. **Reuse** existing components (`app-audio-recorder`, `app-date-time-input`, daily-log review flow) before building new ones.
5. **Prefer** fixing and hardening existing code over parallel implementations.
6. **Match** surrounding naming, file layout, and patterns even when they diverge from official style guides.

## Security requirements

Agents **must** preserve and move toward these constraints (several are gaps in the current codebase):

1. **OpenAI and Azure Speech keys stay server-side only** — never in Angular `environment` or client bundles.
2. **All health-data endpoints require authentication** — use `[Authorize]`; only login/register/OAuth callbacks are anonymous.
3. **Scope data by user** — domain entities need `UserId`; repositories must filter by authenticated user (`JwtRegisteredClaimNames.Sub` from `JwtTokenService`, not `oid`).
4. **No secrets in source control or logs** — use Key Vault / user secrets; never log API keys or connection strings.
5. **Disable Swagger in production** (currently enabled unconditionally in `Program.cs` — do not worsen this).
6. **Rate-limit and cap input** on OpenAI and transcription endpoints (cost abuse prevention).
7. **JWT:** keep signing key out of committed config; do not pass tokens in URL query strings for new flows (existing Google callback uses `?token=` — treat as legacy).
8. **CORS:** update `Program.cs` policy when adding origins; do not use `AllowAnyOrigin` with credentials.

## OpenAI integration rules

Implementation lives in `FodmapLog.Server/Controllers/OpenAIController.cs`. When changing:

1. **Keep calls server-side** via `OpenAI.Chat.ChatClient` and `configuration["openAIApiKey"]` (Key Vault in prod).
2. **Preserve the prompt contract** unless deliberately versioning it: user transcript → JSON array of daily-log objects with `date`, `mealLog` / `symptomsLog` (see inline `jsonExample` in the controller).
3. **Require authentication** before invoking the model.
4. **Bound input size** (transcript length) and set timeouts.
5. **Do not expose** raw OpenAI responses or API keys to the client.
6. **Log** request metadata for monitoring, not full transcript content (health data).

## LLM-generated JSON rules

The core flow: transcript → OpenAI → structured logs → user review (`isPending`) → save via existing CRUD.

1. **Parse and validate on the server** before returning to Angular. Today the controller returns raw `completion.Content[0].Text` — agents should harden this, not rely on the client parsing strings.
2. **Strip markdown fences** (` ```json `) if present before deserialization.
3. **Validate against the expected shape** (`DailyLogDto[]` / frontend `DailyLog[]`): required dates, meal vs symptom discriminant, nested `productQuantity` / `symptoms`.
4. **Return typed JSON** (array of DTOs), not a JSON string wrapped in quotes.
5. **On failure:** return structured errors (422/400); do not throw unhandled exceptions that break the daily-log UI.
6. **Resolve reference data** (symptom types, units) by name against seeded IDs where possible; avoid creating duplicate reference rows from LLM output unless existing repository behavior intentionally does so.
7. **Frontend:** pending AI entries use `DailyLogUI.isPending` and transfer services for edit-before-save — extend this flow; do not bypass review for auto-save without explicit product decision.

## Mobile UI principles

Derived from `app.component.html`, `app.component.css`, and daily-log layout:

1. **Mobile-first:** viewport meta in `index.html`; primary UX targets phone screens.
2. **Bottom navigation on small screens** (`navbar-bottom`, `d-lg-none`); top navbar on desktop (`d-none d-lg-flex`).
3. **Full-width action buttons** on add-meal/symptoms screens (`btn w-100`, split cancel/save rows).
4. **Thumb-friendly controls:** floating add icon (`selectModalIcon`), date chevrons, modal anchored bottom-right (`custom-modal-position`).
5. **Readable log rows:** meal (M) vs symptom (S) badge, time in `shortTime`, product list left-aligned.
6. **Symptom input:** range slider 0–5 with color-coded labels (match `add-symptoms-log` / daily-log templates).
7. **Voice entry:** `app-audio-recorder` embedded in daily-log; keep record → transcribe → AI generate as an inline flow.
8. **Do not break** Bootstrap grid patterns (`row` / `col-*`) already used across templates.

## Deployment notes (for agents changing build/config)

- CI: `.github/workflows/master_healthygultlog-app.yml` publishes `FodmapLog.Server` only.
- Angular output: `fodmaplog.client/dist/fodmaplog.client/` (`fodmaplog.client.esproj`); `ShouldRunBuildScript` is `false` — frontend must be built explicitly for prod static hosting.
- Azure Function deploy: `.github/workflows/azure-functions-app-dotnet.yml` for `TranscribeAudio/`.

## Known gaps (do not ignore when touching these areas)

- No `UserId` on `MealLog` / `SymptomsLog` yet — cross-user data access is possible until fixed.
- Most controllers lack `[Authorize]`.
- `ProductsApi` is referenced in Angular but has no backend controller.
- `environment.prod.ts` is incomplete relative to `environment.ts`.
- `products-api-service.ts` imports `environment.prod` directly — likely unintentional.

Fix these when working in adjacent code; do not introduce new instances of the same problems.
