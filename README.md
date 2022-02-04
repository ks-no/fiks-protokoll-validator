# Fiks-Protokollvalidator

## Formål
Validatoren brukes for å teste at systemer som svarer på meldingstypene for hver protokoll gir svar som er korrekt i henhold til standarden.
Det er da ferdige TestCases for meldingene som brukes til dette. 
Dette gjøres først ved at xml eller json meldingene validerer mot tilhørende skjema (xsd, json-schema) for den meldingstypen i protokollen.
Deretter har hver TestCase definert tester på innholdet i meldingen som blir validert.

## TestCase
TestCasene ligger i mappen `KS.FiksProtokollValidator.WebAPI/TestCases`. Deretter under mappe for den protokollen TestCase tilhører. F.eks. mappen `no.ks.fiks.arkiv.v1` for Fiks-Arkiv protokollen.
Et TestCase har sin egen mappe med et kortnavn for testen. Typisk er navnet bestående av den meldingstypen den tester + en suffix. 
F.eks. HentMoeteplanN1 som da er en test av HentMoeteplan meldingen og N1 står da for "normalsituasjon 1". En test som skal feile vil da være naturlig å bruke F1 som suffix ("feilsituasjon 1"). 
Så øker man suffix til N2, N3, F2, F3 osv etterhvert som det blir flere tester. 


#### TestCase beskrivelsen:
Beskrivelsen og innstruksene for selve TestCaset ligger i en json-fil med navnet `testInformation.json` i root av den aktuelle TestCase mappen.

Eksempel fra politisk behandling:

```json
{
    "testName": "Hent Møteplan, inkluder møter", // Navnet slik det står i listen over TestCases
    "description": "Send hent møteplan", // Beskrivelse av testen under detaljer
    "testStep": "Normalsituasjon 1 Input Hent Møteplan, retur Møteplan", // Beskrivelse av teststeg 
    "messageType": "no.ks.fiks.politisk.behandling.v1.moeteplan.hent", // Meldingstypen i protokollen
    "operation": "HentMoteplan",
    "situation": "N1",
    "expectedResult": "Møteplan med utfylte standardverdier",  // Beskrivelse av hva testen forventer av svar
    "sampleFile": "Samples/moeteplan/sampleHentMoeteplan.json", //OPTIONAL: Path til melding som sendes hvis den ligger tilgjengelig utenfor testens mappe.
    "queriesWithExpectedValues": [ // Spørringer på resultatet som xpath spørringer
        {
            "payloadQuery": "$", // Path til det som skal testes
            "expectedValue": "utvalgId", F/ forventet verdi
            "valueType": 1 // 0 = Verdi, 1 = Attributt - Denne bestemmer om man sjekker at en gitt verdi er på attributtet angitt i paylodQuery ELLER om attributtet i det hele tatt er der
        },
        {
            "payloadQuery": "$",
            "expectedValue": "utvalgNavn", // Sjekker at utvalgNavn attributtet er tilstede 
            "valueType": 1 // 0 = Verdi, 1 = Attributt
        },
        {
            "payloadQuery": "$",
            "expectedValue": "moete", // Sjekker at moete attributtet er tilstede
            "valueType": 1 // 0 = Verdi, 1 = Attributt
        },
        {
            "payloadQuery": "$.utvalgId", // Path er utvalgId attributt på root
            "expectedValue": "*", // * = en hvilken som helst verdi som ikke er tom, null eller whitespace
            "valueType": 0 // 0 = Verdi, 1 = Attributt
        },
        {
            "payloadQuery": "$.utvalgNavn",
            "expectedValue": "Plan og byggesak", // Forventer "Plan og byggesak" på utvalgNavn
            "valueType": 0 // 0 = Verdi, 1 = Attributt
        },
        {
            "payloadQuery": "$.moete[*]",
            "expectedValue": "*", // * = en hvilken som helst verdi som ikke er tom, null eller whitespace
            "valueType": 0 // 0 = Verdi, 1 = Attributt
        }
    ],
    "supported": true,
    "protocol": "no.ks.fiks.politisk.behandling.klient.v1"
```

Meldingen som skal sendes, payload.json for json meldinger, kan enten ligge i rooten på den aktuelle TestCase mappen eller man hvis man som i dette tilfellet har en samplefile inkludert fra en nuget-pakke, bruke `sampleFile` attributtet i stedet.


VIKTIG: operation og situation må tilsvar TestCase mappenavn, det vil si at mappenavn er operation + situation. Altså i dette eksempelet er mappenavnet `HentMoteplanN1`



#### Vedlegg
I TestCase mappen skal man putte evt. vedlegg som skal sendes av TestCaset i en undermappe med navnet `Attachments`. Disse blir plukket opp av validator applikasjonen ved oppstart. Det skal ikke puttes inn noen referanse i `testInformation.json`.  


## Test miljø
[Fiks-Protokollvalidator i test er her](https://forvaltning.fiks.test.ks.no/fiks-validator/#/)

Det kjører applikasjoner som simulerer et system som svarer på meldingene i test. Koden som svarer skal være da valid for hver enkelt validatortest.
Konto id for simulatorer i test:
- Arkivsystem: [8752e128-0e2b-494c-8fab-8e3577aca13d](https://forvaltning.fiks.test.ks.no/fiks-validator/#/NewTestSession?fikskonto=8752e128-0e2b-494c-8fab-8e3577aca13d)
- Fagsystem arkiv: [91307c59-0ddb-4212-bede-59f98e0edf77](https://forvaltning.fiks.test.ks.no/fiks-validator/#/NewTestSession?fikskonto=91307c59-0ddb-4212-bede-59f98e0edf77)
- Politisk behandling: [f95f6811-99db-4169-b7df-7090cffa4174](https://forvaltning.fiks.test.ks.no/fiks-validator/#/NewTestSession?fikskonto=f95f6811-99db-4169-b7df-7090cffa4174)

## (KS) Development miljø 
OBS: Dette er kun tilgjengelig for KS.

[Fiks-Protokollvalidator i development er her](https://forvaltning.fiks.dev.ks.no/fiks-validator/#/) 

Det kjører applikasjoner som simulerer et system som svarer på meldingene i develop. Koden som svarer skal være da valid for hver enkelt validatortest. 
Konto id for simulatorer i develop:
- Arkivsystem: [760fd7d6-435f-4c1b-97d5-92fbe2f603b0](https://forvaltning.fiks.dev.ks.no/fiks-validator/#/NewTestSession?fikskonto=760fd7d6-435f-4c1b-97d5-92fbe2f603b0)
- Fagsystem arkiv: [4a416cde-2aca-4eef-bec4-efddcee0fcea](https://forvaltning.fiks.dev.ks.no/fiks-validator/#/NewTestSession?fikskonto=4a416cde-2aca-4eef-bec4-efddcee0fcea)
- Politisk behandling: [33f6baa8-6645-4dca-9ffb-f95f43333f6c](https://forvaltning.fiks.dev.ks.no/fiks-validator/#/NewTestSession?fikskonto=33f6baa8-6645-4dca-9ffb-f95f43333f6c)

## Oppsett

Validatoren består av to applikasjoner, en front-end (web-app) og en back-end (.NET web-API)
Man kan kjøre det på flere måter:
* Bygge og kjøre applikasjonen 100% i Docker vha docker-compose med 2 Docker images for front-end og back-end + 1 Docker image for sqlexpress.
* Kjøre sqlexpress i Docker og front-end og back-end lokalt
* Sqlexpress på din egen maskin samt front-end og back-end lokalt.
* Bytte ut sqlexpress med en annen sql server


### Alternativ 1: kjøring lokalt vha docker-compose

Krever at man har installert følgende:
* Docker
* Docker Compose

OBS: For Windows og OSX vil man få begge deler installert hvis man bruker Docker Desktop.


#### Database
**Dette må kun gjøres første gang**:

Start sqlexpress i docker ved å kjøre `docker-compose up protokoll-validator-sqlexpress`.
Sørg for at din config (appsettings.Development.json) peker til riktig database.

**Oppdater databasen:**
Opprett/oppdater databasen ved å navigere til *fiks-protokoll-validator/api/KS.FiksProtokollValidator.WebAPI/* og kjør `dotnet ef` kommando for å oppdatere db.
* Hvis ikke du har installert dotnet-ef tool tidligere så kan det installeres slik `dotnet tool install --global dotnet-ef`
* LOKALT: Kjør oppdatering: `dotnet ef database update --connection "Data Source=localhost,1433;Initial Catalog=fiks-protokoll-validator;User Id=SA;Password=Dev#FiksProtokollValidator1234"`
* DEV og TEST: dotnet ef migrations script <evt. from-migration> -o migration-<dagens_dato_her>.sql 
  
**Ved generering av migration script er <from-migration> da siste eksisterende, allerede kjørt, migration** 


#### Docker
Hvis du ønsker å bygge docker lokalt gjør følgende.
#### API
Naviger til /api mappen: ``cd api``

Bygg koden: `docker run -v $(pwd):/source -w /source mcr.microsoft.com/dotnet/sdk:5.0-alpine dotnet publish --configuration Release KS.FiksProtokollValidator.WebAPI/KS.FiksProtokollValidator.WebAPI.csproj --output published-api`

Bygg Docker image: `docker build -t fiks-protokoll-validator-api .`

#### WEB
Naviger til /web-ui mappen: ``cd web-ui``

Bygg koden: `docker run -v $(pwd):/source -w /source node:16 npm install && npm run build -- --mode production `

Bygg Docker image: `docker build -t fiks-protokoll-validator-web .`

### Alternativ 2:  kjøring i lokalt utviklingsmiljø med lokal sqlexpress

Følgende må være installert på utviklingsmaskinen:
 - .NET 5.0 SDK https://dotnet.microsoft.com/download
 - IDE. F.eks. Jetbrains Rider eller Visualstudio (min. v.16.8) https://visualstudio.microsoft.com/downloads/
 - SQL EXPRESS https://www.microsoft.com/en-us/sql-server/sql-server-downloads
 - Node + NPM https://nodejs.org/en/download/
 
#### Back-end:

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
TODO
#### Konfigurasjon:
Konfigurer validatoren for FIKS ved å kopiere malfilen *fiks-protokoll-validator/api/KS.FiksProtokollValidator.WebAPI/FiksIO/**fiks-io-config.template.json***, til **fiks-io-config.json**, under samme katalog som malfilen, og endre innholdet i denne til aktuelt FIKS-oppsett. 

Oppsettet krever en privatnøkkel (.pem-fil) som navngis/plasseres iht. filreferansen i **fiks-io-config.json**.


#### Start back-end applikasjon
Start back-end ved å åpne **fiks-protokoll-validator/api/KS.FiksProtokollValidator.sln** i Visual Studio og bygge/kjøre prosjektet **KS.FiksProtokollValidator.WebAPI**.

### Front-end

 * Hent inn web-applikasjonens avhengigheter ved å navigere til **fiks-protokoll-validator/web-ui/** og å kjøre `npm install`.
 * Start front-end på localhost fra **fiks-protokoll-validator/web-ui/** med `npm run serve`.
