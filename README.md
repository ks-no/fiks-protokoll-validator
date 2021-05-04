# Fiks-Protokollvalidator


Validatoren består av to applikasjoner, en front-end (web-app) og en back-end (.NET web-API)
Man kan kjøre det på flere måter:
* Bygge og kjøre applikasjonen 100% i Docker vha docker-compose med 2 Docker images for front-end og back-end + 1 Docker image for sqlexpress.
* Kjøre sqlexpress i Docker og front-end og back-end lokalt
* Sqlexpress på din egen maskin samt front-end og back-end lokalt. 
* Bytte ut sqlexpress med en annen sql server 

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
Gå til web-ui mappen og bygg docker image: `docker build -t fiks-protokoll-validator-web .`
Gå til api mappen og bygg docker image: `docker build -t fiks-protokoll-validator-api .`

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
