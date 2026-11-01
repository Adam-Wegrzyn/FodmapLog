# Azure beta deploy (Mova / FodmapLog)

Ship a **public HTTPS web URL**. Voice uses **Azure Speech from the web app**
(no Function hop). OpenAI is also server-side. Low-cost, not fully free.

Resources already created (Nov 2026):

- Resource group: `rg-symptomtracker` (Poland Central)
- App Service: `healthygultlog-app` → https://healthygultlog-app.azurewebsites.net
- Plan: `plan-mova-beta` (Linux B1, `DOTNETCORE|10.0`)
- SQL: `sql-symptom-tracker-dev` / `sqldb-symptom-tracker` (admin `adam`)
- Speech: `speech-service-symptomtracker` (**eastus**)
- Key Vault: `kv-symptomtracker` (optional; App Settings are enough)
- `func-mova-transcribe` / `stmova5135f1` — leftover Function, **not used** by current code

## App Service settings (required)

Do **not** set `UseLocalSqlite` or `UseAiStubs` in Production.

| Name | Purpose |
|------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__prodConnection` | Azure SQL (user `adam` + password) |
| `Jwt__Key` | Signing key (do not reuse the one in git) |
| `Jwt__Issuer` | `FodmapLogApi` |
| `Jwt__Audience` | `your-app-users` |
| `openAIApiKey` | Server-side only + spend cap on OpenAI |
| `AzureSpeechApiKey` | Key from `speech-service-symptomtracker` |
| `AzureSpeechRegion` | `eastus` (must match the Speech resource) |

Angular `environment.prod.ts` must use `https://healthygultlog-app.azurewebsites.net`.

## Cloud Shell: settings only (resources already exist)

```bash
RG=rg-symptomtracker
APP=healthygultlog-app
SQL_SERVER=sql-symptom-tracker-dev
SQL_DB=sqldb-symptom-tracker
SQL_USER=adam
SPEECH=speech-service-symptomtracker

echo "Current SQL password:"
read -s SQL_PASS
echo
echo "OpenAI API key:"
read -s OPENAI
echo

az sql server firewall-rule create -g $RG -s $SQL_SERVER -n AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
MYIP=$(curl -s https://ifconfig.me)
az sql server firewall-rule create -g $RG -s $SQL_SERVER -n MyHome --start-ip-address $MYIP --end-ip-address $MYIP

JWT=$(openssl rand -base64 48)
SPEECH_KEY=$(az cognitiveservices account keys list -g $RG -n $SPEECH --query key1 -o tsv)

az webapp config appsettings set -g $RG -n $APP --settings \
  ASPNETCORE_ENVIRONMENT=Production \
  Jwt__Key="$JWT" \
  Jwt__Issuer=FodmapLogApi \
  Jwt__Audience=your-app-users \
  openAIApiKey="$OPENAI" \
  AzureSpeechApiKey="$SPEECH_KEY" \
  AzureSpeechRegion=eastus \
  UseLocalSqlite=false \
  UseAiStubs=false

az webapp config connection-string set -g $RG -n $APP --connection-string-type SQLAzure \
  --settings prodConnection="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${SQL_DB};User ID=${SQL_USER};Password=${SQL_PASS};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

No Function URL or function key.

## Database

```bash
dotnet ef database update --project DataAccess --startup-project FodmapLog.Server
```

## Build SPA + publish

```bash
cd fodmaplog.client && npm ci && npm run build
mkdir -p ../FodmapLog.Server/wwwroot
cp -R dist/fodmaplog.client/browser/. ../FodmapLog.Server/wwwroot/
cd ../FodmapLog.Server && dotnet publish -c Release -o ./publish
az webapp deploy -g rg-symptomtracker -n healthygultlog-app --src-path ./publish --type zip
```

## Local voice without Azure Function

```bash
dotnet user-secrets set AzureSpeechApiKey "<key>" --project FodmapLog.Server
dotnet user-secrets set AzureSpeechRegion "eastus" --project FodmapLog.Server
```

Or set `UseAiStubs` true in Development for a fake transcript.
