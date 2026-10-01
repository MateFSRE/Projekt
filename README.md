# CodeGames

Web aplikacija za učenje programiranja kroz kratke igre. Nakon svakog odgovora poslužitelj vraća objašnjenje, bodove, razinu i mjesto na rang listi.

## Preduvjeti

- .NET SDK 10
- Node.js 22 (ili noviji LTS koji pokreće Vite 8)
- SQL Server Express na istom računalu
- `sqlcmd` nije obavezan, ali pomaže pri provjeri baze

Provjera:

```powershell
dotnet --version
node --version
```

## Tajne i konfiguracija

U Git ne idu connection string ni JWT tajna. Stoje u user-secrets ili u `CodeGames/appsettings.Development.json` (datoteka je u `.gitignore`).

Iz mape `CodeGames`:

```powershell
dotnet user-secrets set "Jwt:Secret" "najmanje-32-znaka-slucajni-tekst"
dotnet user-secrets set "Seed:AdminPassword" "lozinka-admina"
dotnet user-secrets set "Seed:PlayerPassword" "lozinka-igraca"
```

`CodeGames/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "CodeGamesDb": "Server=.\\SQLEXPRESS;Database=CodeGames;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

`appsettings.json` (ide u Git) drži samo javne postavke:

- `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes`
- `DemoChallengeId` — id aktivnog SwipeBug ili FillBlanks zadatka za `/demo` (zadano 16)
- `Cors:Origins` — popis originâ frontenda, zadano `http://localhost:5173`

## Baza i migracije

Iz korijena repozitorija, dok API nije pokrenut:

```powershell
dotnet ef database update --project CodeGames
```

Migracije su code-first i nalaze se u `CodeGames/Data/Migrations`. `dotnet ef` alat, ako ga nema:

```powershell
dotnet tool install --global dotnet-ef
```

Prvo pokretanje API-ja stvara admina i igrača ako ih nema. Admin: `admin@codegames.local` / `admin`, lozinka `Seed:AdminPassword`. Igrač: `igrac1@codegames.local` / `igrac1`, lozinka `Seed:PlayerPassword`. Bodovi i pokušaji s drugog računala se ne prenose. Zadaci dolaze migracijom.

## Pokretanje

Backend, iz korijena:

```powershell
dotnet run --project CodeGames\CodeGames.csproj --launch-profile http
```

API: http://localhost:5080  
Swagger: http://localhost:5080/swagger

Frontend, u drugom terminalu:

```powershell
cd codegames-frontend
npm install
npm run dev
```

Aplikacija: http://localhost:5173  
Vite prosljeđuje `/api` na port 5080. Kolačić se šalje jer axios ima `withCredentials: true`.

## Testovi

Jedinični i integracijski (xUnit):

```powershell
dotnet test CodeGames.Tests\CodeGames.Tests.csproj
```

Integracijski testovi dižu API preko `WebApplicationFactory` i bazu `CodeGamesTests` na istom SQL Serveru. Connection string čitaju iz `appsettings.Development.json` i mijenjaju ime baze, ili iz varijable `CODEGAMES_TEST_CONNECTION` ako želiš drugi poslužitelj. Ne diraju bazu `CodeGames`.

E2E (Playwright), jednom instaliraj preglednik:

```powershell
cd codegames-frontend
npx playwright install chromium
$env:PLAYWRIGHT_ADMIN_PASSWORD = "lozinka iz Seed:AdminPassword"
npm run test:e2e
```

E2E pokreće backend i Vite ako već ne slušaju. Demo test očekuje SwipeBug (`DemoChallengeId` 16) i gumb „Ima bug”. Registracija i admin test upisuju podatke u lokalnu bazu `CodeGames`. Admin test zadatak nakon toga briše.

## Drugo računalo

Windows autentifikacija (`Trusted_Connection=True`) vrijedi samo za računalo na kojem radi SQL Server Express i na kojem je Windows korisnik vlasnik instance. Na drugom računalu connection string treba pokazivati na tamošnji poslužitelj.

Ako SQL nije na tom laptopu, koristi SQL prijavu i upiši je samo u user-secrets ili u lokalni `appsettings.Development.json`:

```text
Server=IME_RACUNALA\SQLEXPRESS;Database=CodeGames;User Id=codegames;Password=tajna;TrustServerCertificate=True
```

Zatim ponovo:

1. `dotnet user-secrets set "Jwt:Secret" "..."`, `Seed:AdminPassword` i `Seed:PlayerPassword`
2. `dotnet ef database update --project CodeGames`
3. u `appsettings.json` promijeni `Cors:Origins` ako frontend nije na `http://localhost:5173`
4. `dotnet run` i `npm run dev`

Isti connection string, s bazom `CodeGamesTests` ili varijablom `CODEGAMES_TEST_CONNECTION`, treba i integracijskim testovima.
