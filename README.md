# Fiks-Protokollvalidator


Validatoren består av to applikasjoner, en front-end (web-app) og en back-end (.NET web-API)
Man kan kjøre det på flere måter:
* Bygge og kjøre applikasjonen 100% i Docker vha docker-compose med 2 Docker images for front-end og back-end + 1 Docker image for sqlexpress.
* Kjøre sqlexpress i Docker og front-end og back-end lokalt
* Sqlexpress på din egen maskin samt front-end og back-end lokalt. 
* Bytte ut sqlexpress med en annen sql server 

## Testing i Development miljø
[Fiks-Protokollvalidator i development er her](https://forvaltning.fiks.dev.ks.no/fiks-validator/#/) 

Konto id man kan benytte:
- Arkivsystem: [760fd7d6-435f-4c1b-97d5-92fbe2f603b0](https://forvaltning.fiks.dev.ks.no/fiks-validator/#/NewTestSession?fikskonto=760fd7d6-435f-4c1b-97d5-92fbe2f603b0)
- Fagsystem arkiv: [4a416cde-2aca-4eef-bec4-efddcee0fcea](https://forvaltning.fiks.dev.ks.no/fiks-validator/#/NewTestSession?fikskonto=4a416cde-2aca-4eef-bec4-efddcee0fcea)
- Politisk behandling: [33f6baa8-6645-4dca-9ffb-f95f43333f6c](https://forvaltning.fiks.dev.ks.no/fiks-validator/#/NewTestSession?fikskonto=33f6baa8-6645-4dca-9ffb-f95f43333f6c)

## Testing i Test miljø
[Fiks-Protokollvalidator i test er her](https://forvaltning.fiks.test.ks.no/fiks-validator/#/)

Konto id man kan benytte:
- Arkivsystem: [8752e128-0e2b-494c-8fab-8e3577aca13d](https://forvaltning.fiks.test.ks.no/fiks-validator/#/NewTestSession?fikskonto=8752e128-0e2b-494c-8fab-8e3577aca13d)
- Fagsystem arkiv: [91307c59-0ddb-4212-bede-59f98e0edf77](https://forvaltning.fiks.test.ks.no/fiks-validator/#/NewTestSession?fikskonto=91307c59-0ddb-4212-bede-59f98e0edf77)
- Politisk behandling: [f95f6811-99db-4169-b7df-7090cffa4174](https://forvaltning.fiks.test.ks.no/fiks-validator/#/NewTestSession?fikskonto=f95f6811-99db-4169-b7df-7090cffa4174)

## Oppsett og kjøring lokalt vha docker-compose

Krever at man har installert følgende:
* Docker
* Docker Compose

OBS: For Windows og OSX vil man få begge deler installert hvis man bruker Docker Desktop.


### Database
**Dette må kun gjøres første gang**:

Start sqlexpress i docker ved å kjøre `docker-compose up protokoll-validator-sqlexpress`.
Sørg for at din config (appsettings.Development.json) peker til riktig database.

**Oppdater databasen:**
Opprett/oppdater databasen ved å navigere til *fiks-protokoll-validator/api/KS.FiksProtokollValidator.WebAPI/* og kjør `dotnet ef` kommando for å oppdatere db.
* Hvis ikke du har installert dotnet-ef tool tidligere så kan det installeres slik `dotnet tool install --global dotnet-ef`
* LOKALT: Kjør oppdatering: `dotnet ef database update --connection "Data Source=localhost,1433;Initial Catalog=fiks-protokoll-validator;User Id=SA;Password=Dev#FiksProtokollValidator1234"`
* DEV og TEST: dotnet ef migrations script <evt. from-migration> -o migration-<dagens_dato_her>.sql 
  
**Ved generering av migration script er <from-migration> da siste eksisterende, allerede kjørt, migration** 



### Docker
Hvis du ønsker å bygge docker lokalt gjør følgende.
#### API
Naviger til /api mappen: ``cd api``

Bygg koden: `docker run -v $(pwd):/source -w /source mcr.microsoft.com/dotnet/sdk:5.0-alpine dotnet publish --configuration Release KS.FiksProtokollValidator.WebAPI/KS.FiksProtokollValidator.WebAPI.csproj --output published-api`

Bygg Docker image: `docker build -t fiks-protokoll-validator-api .`

#### WEB
Naviger til /web-ui mappen: ``cd web-ui``

Bygg koden: `docker run -v $(pwd):/source -w /source node:16 npm install && npm run build -- --mode production `

Bygg Docker image: `docker build -t fiks-protokoll-validator-web .`

## Oppsett og kjøring i lokalt utviklingsmiljø med lokal sqlexpress

Følgende må være installert på utviklingsmaskinen:
 - .NET 5.0 SDK https://dotnet.microsoft.com/download
 - IDE. F.eks. Jetbrains Rider eller Visualstudio (min. v.16.8) https://visualstudio.microsoft.com/downloads/
 - SQL EXPRESS https://www.microsoft.com/en-us/sql-server/sql-server-downloads
 - Node + NPM https://nodejs.org/en/download/
 
### Back-end

#### Appsettings
 Kopier appsettings.json til appsettings.Development.json. Denne kan du endre og tilpasse ditt utviklingsmiljø og ignoreres i .gitignore.

#### Database
**Dette må kun gjøres første gang**:

 Installer sqlexpress på din utviklingsmaskin. 
 Sørg for at din config (appsettings.Development.json) peker til riktig database.

 *Oppdater databasen:**
 Opprett/oppdater databasen ved å navigere til *fiks-protokoll-validator/api/KS.FiksProtokollValidator.WebAPI/* og kjør `dotnet ef` kommando for å oppdatere db.
* Hvis ikke du har installert dotnet-ef tool tidligere så kan det installeres slik `dotnet tool install --global dotnet-ef`
* Kjør oppdatering: `dotnet ef database update"`


#### Pem-fil


#### Konfigurasjon:
Konfigurer validatoren for FIKS ved å kopiere malfilen *fiks-protokoll-validator/api/KS.FiksProtokollValidator.WebAPI/FiksIO/**fiks-io-config.template.json***, til **fiks-io-config.json**, under samme katalog som malfilen, og endre innholdet i denne til aktuelt FIKS-oppsett. 

Oppsettet krever en privatnøkkel (.pem-fil) som navngis/plasseres iht. filreferansen i **fiks-io-config.json**.


#### Start back-end applikasjon
Start back-end ved å åpne **fiks-protokoll-validator/api/KS.FiksProtokollValidator.sln** i Visual Studio og bygge/kjøre prosjektet **KS.FiksProtokollValidator.WebAPI**.

### Front-end

 * Hent inn web-applikasjonens avhengigheter ved å navigere til **fiks-protokoll-validator/web-ui/** og å kjøre `npm install`.
 * Start front-end på localhost fra **fiks-protokoll-validator/web-ui/** med `npm run serve`.
