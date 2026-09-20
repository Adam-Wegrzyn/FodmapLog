# Azure beta deploy (Mova / FodmapLog)

Ship a **public HTTPS web URL** (no Play Store, no payments). Voice and AI
need Speech + OpenAI, so this is **low-cost**, not fully free.

Existing Azure names in this repo (verify in the portal):

- App Service: `healthygultlog-app` (Poland Central)
- SQL: `sql-symptom-tracker-dev` / `sqldb-symptom-tracker`
- Key Vault (commented): `healthygutlog-kv`
- Function host in comments: `example-function-…` — **placeholder, not production-ready**

## 0. Rotate secrets first

`FodmapLog.Server/appsettings.json` currently contains a **SQL password**,
**JWT signing key**, and **Application Insights key**. Treat them as leaked.

1. Rotate the Azure SQL password and JWT key immediately.
2. Put replacements only in App Service / Key Vault / GitHub secrets.
3. Leave placeholders in git. Do not commit new secrets.

## 1. Provision (or confirm) Azure resources

| Resource | Suggested SKU | Required? |
|----------|---------------|-----------|
| App Service (Windows or Linux) + .NET | F1 / B1 | Yes — hosts API + Angular `wwwroot` |
| Azure SQL | Basic or serverless | Yes |
| Function App (`TranscribeAudio`) | Consumption | Yes for voice |
| Speech (Cognitive Services) | F0 | Yes for voice |
| OpenAI API key | Pay-as-you-go + **hard spend cap** | Yes for voice → table |
| Key Vault | Standard | Strongly recommended |
| Storage account | For Functions | Yes (`AzureWebJobsStorage`) |

App Service F1 has no Always On (cold starts) and a CPU quota. Fine for a
small tester beta.

## 2. App Service settings (or Key Vault references)

Do **not** set `UseLocalSqlite` or `UseAiStubs` in Production.

| Name | Purpose |
|------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Production` (turns off Swagger, enables HTTPS redirect, uses SQL) |
| `ConnectionStrings__prodConnection` | Azure SQL |
| `Jwt__Key` | Rotated signing key |
| `Jwt__Issuer` | `FodmapLogApi` |
| `Jwt__Audience` | `your-app-users` |
| `openAIApiKey` | Server-side only |
| `Azure__TranscriptionFunctionUrl` | `https://<function-app>/api/Function1` |
| `TranscribeFunctionKey` | Function host key (`x-functions-key`) |
| `googleAuthSecret` + `Authentication__Google__ClientId` | Optional Google login |
| `KeyVaultName` + Azure AD / **managed identity** | Optional KV bootstrap |

Angular `environment.prod.ts` already points at
`https://healthygultlog-app-g7g3hheydccweaam.polandcentral-01.azurewebsites.net`.
Rebuild the SPA if that hostname changes.

## 3. Function App settings

| Name | Purpose |
|------|---------|
| `AzureSpeechApiKey` | Speech resource key |
| `AzureSpeechRegion` | Must match the Speech resource |
| `AzureWebJobsStorage` | Required by Functions |

Fix `.github/workflows/azure-functions-app-dotnet.yml`: replace
`AZURE_FUNCTIONAPP_NAME: 'example-function'` and add secret
`AZURE_FUNCTIONAPP_PUBLISH_PROFILE`.

## 4. Database

Migrations are **not** applied on App Service startup (only SQLite
`EnsureCreated` in Development).

```bash
dotnet ef database update --project DataAccess --startup-project FodmapLog.Server
```

Must include `20260902183000_AddUserIdToMealAndSymptomsLogs`. Open SQL
firewall to the App Service outbound IPs (and your machine for the
one-time migrate).

## 5. Build the SPA into the server (required)

CI currently publishes **only** `FodmapLog.Server`. There is no `wwwroot`
in git. Without this step testers get API-only / a stale shell.

```bash
cd fodmaplog.client && npm ci && npm run build
# Angular 17 application builder output:
cp -R dist/fodmaplog.client/browser/. ../FodmapLog.Server/wwwroot/
cd ../FodmapLog.Server && dotnet publish -c Release
```

Then extend `master_healthygultlog-app.yml` to:

- trigger on `fodmaplog.client/**`, `Core/**`, `DataAccess/**` (not only Server)
- `actions/setup-dotnet` for **9.x** (Server csproj is `net10.0` — retarget to
  `net9.0` or install the .NET 10 SDK on the agent **and** App Service)
- run the npm build + copy before `dotnet publish`

Same-origin hosting means CORS does not need the Azure hostname. Add it
only if the SPA is ever hosted on a different origin.

## 6. Auth / HTTPS / Google

- App Service HTTPS is on by default; `UseHttpsRedirection` runs outside Development.
- Email/password register + login work without extra config.
- Google login is shown in the UI but only works if ClientId + secret are set
  and the authorized redirect is
  `https://<app>/api/Auth/external-login-callback`.
- Forgot-password UI exists; emails are **not** sent (`LoggingEmailSender`
  writes to logs). For beta, tell testers to pick a password they will
  remember, or hide the forgot-password link.

## 7. Smoke test on the public URL

1. Open `https://<app>.azurewebsites.net` → signup → login.
2. Add a meal and a symptom manually; confirm they appear on Today + timeline.
3. Record a short **multi-event** story (meal + later symptom). Review sheet
   → edit one row → **Save all**. Confirm rows land on the selected day.
4. Switch **EN | PL**; record again; confirm speech locale and UI language.
5. Export PDF for a range that includes Polish food/symptom labels.
6. Confirm user B cannot see user A’s logs.
7. Confirm unauthenticated `/api/FodmapLog/...` returns 401.

## 8. Tester-beta product gaps (not required to get a URL)

- No delete-account / data-purge
- No privacy policy or terms
- No dietitian share link (PDF download only)
- No invite / waitlist (signup is open)
- Password reset needs real email before you advertise it
