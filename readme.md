# Backend Light – Architecture Lab

Dette repositoryet er et undervisningslaboratorium for de siste ukene av **Backend Light**.

Det samme konkrete domenet – booking av Kåres arbeidstid – implementeres i flere versjoner. Hver versjon introduserer én ny idé som svar på et problem vi kan se i forrige versjon.

Målet er **ikke** å ende med én «riktig arkitektur».

Målet er å kunne sammenligne kode og spørre:

> Hvilket problem prøver denne teknikken å løse?

Reisen er:

```text
Transaction Script
↓
Rich Domain Model
↓
Value Objects
↓
Domain Events
↓
Functional Core / Imperative Shell
↓
Pure Functions
↓
Functional Composition
```

I tillegg finnes en valgfri sidegren om:

```text
Aggregate Root
```

---

# Caset: Kåre Booking

Kåre er pensjonist og tar småoppdrag noen timer om dagen.

Systemet lar kunder:

- få et pristilbud
- booke Kåre
- se bookinger
- avbestille en booking

Caset er bevisst konkret og har nok forretningsregler til at forskjellen mellom application logic og domain logic blir synlig.

---

## Core før og etter uke 10

V0–V3 bruker Onion Architecture: Api → Core. Core har ApplicationServices,
DomainServices (interfaces mot omverdenen) og DomainModel. Infrastructure implementerer
portene, og Program.cs kobler dem med Dependency Injection.

I V3 bruker Core/ApplicationServices/BookingService repository, clock og payment.
Dependency Inversion beskytter Core mot konkrete infrastructure-klasser.

I V4/V6 går Functional Core videre: business-beslutningen avhenger heller ikke av
infrastructure-abstraksjoner. Den tidligere effectful Application Service deles i:

- Core/ApplicationServices/BookingService: pure application/business orchestration med vanlige data.
- Api/ImperativeShell/BookingHandler: leser state, henter tid/id, kaller Core og utfører effects.

Api/ImperativeShell/Ports eier interfaces, Infrastructure implementerer dem,
og Program.cs kobler DI. Core har bare ApplicationServices og DomainModel.
Application Service-konseptet beholdes; den orkestrerer nå domenet med data fremfor services.
V0–V3s Core er Onion sitt indre prosjekt. V4/V6s Core er også faktisk Functional Core.
# Forretningsregler

## Arbeidstid

Kåre arbeider:

```text
mandag–fredag
kl. 12:00–16:00
```

Alle tidspunkt er `DateTime` og representerer lokal tid for Kåres virksomhet.

En booking:

- må starte på en hel klokktime
- må vare mellom 1 og 4 hele timer
- må ligge innenfor kl. 12:00–16:00
- kan ikke overlappe en eksisterende aktiv booking

En booking som starter kl. 16:00 er derfor ugyldig.

En booking fra kl. 12:00 til kl. 16:00 er gyldig.

---

# Bookingfrist

Kåre ønsker å vite neste ukes oppdrag på forhånd.

En booking må derfor gjøres senest:

> **fredag kl. 16:00 i uken før oppdraget**

Nøyaktig kl. 16:00 er tillatt.

Etter kl. 16:00 avvises bookingen.

Fristen beregnes direkte fra det lokale tidspunktet.

---

# Pris

Prisen er:

```text
første time:       500 kr
hver ekstra time:  300 kr
```

Dermed:

| Varighet | Pris |
| ---: | ---: |
| 1 time | 500 kr |
| 2 timer | 800 kr |
| 3 timer | 1100 kr |
| 4 timer | 1400 kr |

---

# Avbestilling

Refusjonen avhenger av hvor lang lokal tid det er igjen til oppdraget starter.

## Minst 7 døgn før

```text
full refusjon
```

Eksempel:

```text
800 kr betalt
→ 800 kr refundert
```

Nøyaktig 168 timer før gir full refusjon.

## Fra 48 timer og opp til 7 døgn før

Det trekkes:

```text
100 kr avbestillingsgebyr
```

Eksempel:

```text
800 kr betalt
→ 700 kr refundert
```

Nøyaktig 48 timer før hører til denne regelen.

## Mindre enn 48 timer før

```text
ingen refusjon
```

En booking som allerede har startet kan ikke avbestilles.

En allerede avbestilt booking kan heller ikke avbestilles på nytt.

---

# Solution

Åpne:

```text
BackendLight.ArchitectureLab.sln
```

Solutionen inneholder ti Solution Folders:

```text
Prelude A - Endpoint Script
Prelude B - Rich Domain without Application Service
00 - Transaction Script
01 - Rich Domain Model
02 - Value Objects
03 - Domain Events
04 - Functional Core Imperative Shell
05 - Pure Functions
06 - Functional Composition
90 - Optional Aggregate Root
```

Versjonene er bevisst selvstendige.

Det finnes ikke ett felles domain-library som alle versjoner deler. Litt duplisering mellom versjonene gjør det lettere å åpne to steg side om side og sammenligne dem.

---

# Prelude – to uavhengige designvalg

Prelude-variantene er ekstra sammenligningsvarianter før V0, ikke nummererte steg i hovedreisen.

| Orkestrering | Script: business-regler i use caset | Rich Domain: business-regler i modellen |
| --- | --- | --- |
| Endpoint | [Prelude A – Endpoint Script](Prelude%20A%20-%20Endpoint%20Script/README.md) | [Prelude B – Rich Domain without Application Service](Prelude%20B%20-%20Rich%20Domain%20without%20Application%20Service/README.md) |
| Application Service | V0 – Transaction Script | V1 – Rich Domain Model |

Begge Prelude-variantene har ett Minimal API-prosjekt og injiserer konkrete Infrastructure-klasser.
De bruker **Dependency Injection**, men ikke **Dependency Inversion**: ingen interfaces eller separat Onion Core.
Prelude-eksemplene har bevisst ingen testprosjekter.

- **A → B:** Flytt business-kunnskap til DomainModel; behold orkestrering i endpoint og konkrete dependencies.
- **A → V0:** Flytt orkestrering til Application Service og innfør Core-grense og interfaces; behold script-orienterte regler.
- **B → V1:** Behold omtrent samme domenemodell; flytt orkestrering til Application Service og innfør Dependency Inversion.

Domain modeling er ikke avhengig av Onion Architecture. Trenger denne lille applikasjonen faktisk
Application Service og Onion Architecture? Ikke nødvendigvis.

Alle bookingvariantene bruker samme lesbare overlappslogikk: intervaller som slutter før eller ved
start, eller starter etter eller ved slutt, overlapper ikke. Kansellerte bookinger blokkerer ikke.

---

# 00 – Transaction Script

**Spørsmål:** Hvem bør eie forretningsreglene når application service også orkestrerer I/O?

Prosjekter:

```text
KareBooking.V0.Api
KareBooking.V0.Core
KareBooking.V0.Tests
```

Her bruker vi teknikker studentene allerede kjenner:

- Minimal API
- service
- repository
- Dependency Injection
- `Result<T>`
- interfaces
- testbare I/O-adapters

`BookingService.GetQuoteAsync()` og `BookAsync()` deler validering, tilgjengelighet og pris
gjennom den private hjelpemetoden `EvaluateBookingAsync`. Vi har allerede brukt Extract Method.

V0 er en legitim, ryddig Transaction Script-løsning. DomainModel inneholder relativt anemiske typer;
`BookingService` eier nesten all forretningskunnskap og orkestrerer I/O.

Transaction Script kan være et godt og enklere valg for et lite domene. V1 utforsker om viktige,
navngitte regler bør ha sitt hjem i domenemodellen.

**Start her:**

```text
00 - Transaction Script/KareBooking.V0.Core/ApplicationServices/BookingService.cs
```

[Les mer om V0](00%20-%20Transaction%20Script/README.md)

---

# 01 – Rich Domain Model

**Problem:** Application service vet for mye om virksomheten.

Prosjekter:

```text
KareBooking.V1.Api
KareBooking.V1.Core
KareBooking.V1.Tests
```

Forretningskunnskap flyttes fra ApplicationServices til DomainModel i samme Core-prosjekt. V0s `BookingService` kjenner reglene; i V1 kjenner
domenemodellen reglene og `BookingService` orkestrerer. Begge har allerede intern regelgjenbruk.

Blant annet får vi:

```text
KareSchedule
Booking
```

ApplicationServices blir mer:

```text
les data
↓
kall domenet
↓
lagre resultat
↓
utfør sideeffekter
```

Det sentrale spørsmålet er:

> Hva er egentlig det vi ønsker å ha i DomainModel?

**Start her:**

```text
01 - Rich Domain Model/KareBooking.V1.Core/DomainModel/KareSchedule.cs
01 - Rich Domain Model/KareBooking.V1.Core/DomainModel/Booking.cs
```

Sammenlign deretter `BookingService` med V0.

[Les mer om V1](01%20-%20Rich%20Domain%20Model/README.md)

---

# 02 – Value Objects

**Problem:** Primitive typer kan representere ugyldige eller uklare domeneverdier.

Prosjekter:

```text
KareBooking.V2.Api
KareBooking.V2.Core
KareBooking.V2.Tests
```

Vi introduserer blant annet:

```text
TimeSlot
Money
```

Et gyldig `TimeSlot` kan ikke representere for eksempel:

```text
start kl. 12:37
0 timers varighet
helg
slutt etter kl. 16
```

Value Objects gir to forskjellige gevinster:

1. invariants og validering får et naturlig hjem
2. typesystemet uttrykker domenet sterkere

Et gyldig `TimeSlot` betyr derimot ikke at tidspunktet er ledig. Tilgjengelighet er fremdeles en regel som krever kontekst.

**Start her:**

```text
02 - Value Objects/KareBooking.V2.Core/DomainModel/TimeSlot.cs
02 - Value Objects/KareBooking.V2.Core/DomainModel/Money.cs
```

[Les mer om V2](02%20-%20Value%20Objects/README.md)

---

# 03 – Domain Events

**Problem:** Domenebeslutningen er koblet direkte til alle tingene som skal skje etterpå.

Prosjekter:

```text
KareBooking.V3.Api
KareBooking.V3.Core
KareBooking.V3.Tests
```

Domenet produserer nå forretningsfakta som:

```text
BookingCreated
BookingCancelled
```

Core kjenner ikke:

```text
SMTP
HTTP
database
payment SDK
audit implementation
```

Andre deler av applikasjonen kan reagere på domain events.

For eksempel:

```text
BookingCreated
↓
send bekreftelsesmail
↓
skriv audit-logg
```

Det er viktig å skille:

```text
BookingCreated
```

som er et **domain event**, fra:

```text
SendEmail
WriteAuditLog
```

som er tekniske actions/effects.

**Start her:**

```text
03 - Domain Events/KareBooking.V3.Core/DomainModel/DomainEvents.cs
03 - Domain Events/KareBooking.V3.Core/ApplicationServices/BookingEventHandler.cs
```

[Les mer om V3](03%20-%20Domain%20Events/README.md)

Se også:

[Forretningsfakta og tekniske handlinger som data](docs/actions-as-data.md)

---

# 04 – Functional Core / Imperative Shell

**Uke 10 mandag:** Separate effects from business decisions.

Kåre Booking fortsetter med samme regler, verdiobjekter, Domain Events, HTTP API og fakes.
KareBooking.V4.Core/ApplicationServices/BookingService.cs er nå pure og static.
GetQuote, Book og Cancel får state, tid og id som vanlige parametre; samme input gir samme output.

KareBooking.V4.Api/ImperativeShell/BookingHandler.cs leser repository, henter nåtid,
genererer id, kaller BookingService og utfører lagring, betaling/refusjon og event-dispatch.
BookingEventHandler håndterer e-post/audit. Ports ligger i Api/ImperativeShell/Ports.
KareSchedule eier kalenderreglene; Booking eier creation/cancellation, refusjon og Domain Events.
BookingService orkestrerer domenet. Vi splitter den gamle Application Service langs effect-grensen.
Domain Events er fakta; vi lager ingen generell effect-plan i denne versjonen.

Book bruker eksplisitte Result-sjekker. Åpne BookingService og BookingHandler side om side.
[Les mer om V4](04%20-%20Functional%20Core%20Imperative%20Shell/README.md).

---

# 05 – Pure Functions

**Uke 10 onsdag:** Generalize the ideas: pure functions, explicit inputs, immutability and effects as data.

KareBooking.PureFunctions.Console er en selvstendig lab; KareBooking.PureFunctions.Tests
har små tester med vanlige verdier. Examples.cs viser Price, skjult tid/id/randomness,
fil-I/O, mutasjon og record + with. Records gir ikke automatisk dyp immutability.

Effects.cs skiller BookingCreated (et faktum) fra SaveBooking, ChargePayment og SendEmail
(planlagte handlinger). En pure Plan returnerer data; en liten interpreter simulerer I/O.
Ingen ASP.NET, DI eller reell database/nettverk kreves. Composition kommer i V6.

[Les mer om V5](05%20-%20Pure%20Functions/README.md).

---

# 06 – Functional Composition / Railway Oriented Programming

**Uke 10 fredag:** Compose pure functions that can fail using Result<T>, Map and FlatMap.

KareBooking.V6.Core, KareBooking.V6.Api og KareBooking.V6.Tests har samme arkitektur som V4.
BookingHandler, event handling, Ports og Infrastructure beholdes likt.
KareBooking.Composition.Console introduserer functions as values, higher-order functions
og A → B → C før Kåres Book-pipeline.

Core/ApplicationServices/BookingService komponerer KareSchedule.ValidateSlot og
Booking.Create. GetQuote kombinerer KareSchedule.ValidateQuoteSlot og Money.ForHours.
Result<T>.Map bruker T → U; FlatMap bruker T → Result<U>
uten nesting. FlatMap kalles ofte Bind i andre biblioteker; her bruker vi bare FlatMap.

Success kjører neste steg. Failure hopper over resten og propagerer første feil:
dette er Railway Oriented Programming. Cancel delegerer direkte til Booking.Cancel.
Composition er et verktøy; all I/O ligger fortsatt i shell.

[Les mer om V6](06%20-%20Functional%20Composition/README.md).

---
# 90 – Optional Aggregate Root

Denne versjonen er en **valgfri sidegren**.

Prosjekter:

```text
KareBooking.Aggregates.Api
KareBooking.Aggregates.Core
KareBooking.Aggregates.Tests
```

Her ser vi på hvorfor flere individuelt gyldige entities likevel kan inngå i én consistency boundary.

`KareSchedule` eier bookingene og er eneste inngang til endringer.

Repository-et arbeider med hele rooten, ikke med ett repository per database-tabell.

Versjonen viser også hvorfor:

```text
Aggregate Root
```

og:

```text
optimistic concurrency
```

løser forskjellige deler av samme konsistensproblem.

To snapshots kan begge ta en lokalt gyldig domenebeslutning, mens bare ett stale-safe save kan lykkes.

**Start her:**

```text
90 - Optional Aggregate Root/KareBooking.Aggregates.Core/KareSchedule.cs
90 - Optional Aggregate Root/KareBooking.Aggregates.Tests/AggregateTests.cs
```

[Les mer om Aggregate Root](90%20-%20Optional%20Aggregate%20Root/README.md)

---

# Arkitekturideene er ikke konkurrenter

Repositoryet viser ikke en reise fra:

```text
gammel dårlig arkitektur
```

til:

```text
ny korrekt arkitektur
```

Ideene svarer på forskjellige spørsmål.

## Transaction Script

> Hvordan organiserer vi en use case?

## Rich Domain Model / DDD

> Hvordan modellerer vi virksomheten og beskytter forretningsreglene?

## Onion/Clean-lignende dependency direction

> Hvilken vei peker avhengighetene?

## Functional Core / Imperative Shell

> Hvor plasserer vi I/O og andre sideeffekter?

De kan kombineres.

For eksempel kan samme applikasjon ha:

```text
Onion-lignende dependency direction
+
rik domenemodell
+
Value Objects
+
Functional Core / Imperative Shell
```

Les mer:

[Arkitektursammenligning](docs/architecture-comparison.md)

---

# Kjøre solutionen

Repositoryet bruker:

```text
.NET 10
```

og er utviklet/verifisert med:

```text
.NET SDK 10.0.401
```

Åpne `BackendLight.ArchitectureLab.sln` i **Visual Studio 2026 (18.x)** med
arbeidsbelastningen **ASP.NET and web development** installert. Visual Studio 2022
kan ikke laste prosjektene med denne SDK-en, selv om .NET 10 er installert.
`global.json` krever SDK 10.0.401 eller en nyere patch i 10.0.4xx-serien.

Hvis du får «The SDK 'Microsoft.NET.Sdk' specified could not be found» eller
tilsvarende for `Microsoft.NET.Sdk.Web`, kontroller at du bruker Visual Studio
2026. Hvis dobbeltklikk åpner feil versjon, start Visual Studio 2026 og åpne
solutionen via **File → Open → Project/Solution**. Kontroller også installerte
SDK-er med `dotnet --list-sdks`.

Bygg alt:

```powershell
dotnet build
```

Kjør alle tester:

```powershell
dotnet test
```

---

# Kjøre API-versjonene

Eksempel V0:

```powershell
dotnet run --project "00 - Transaction Script/KareBooking.V0.Api"
```

API-ene bruker disse portene:

| Versjon | Port |
| --- | ---: |
| Prelude A Endpoint Script | 5098 |
| Prelude B Rich Domain | 5099 |
| V0 Transaction Script | 5100 |
| V1 Rich Domain Model | 5101 |
| V2 Value Objects | 5102 |
| V3 Domain Events | 5103 |
| V4 Functional Core | 5104 |
| V6 Functional Composition | 5105 |
| Optional Aggregate Root | 5190 |

Hver API-versjon har sin egen `.http`-fil med ferdige requests.

For eksempel:

```text
00 - Transaction Script/KareBooking.V0.Api/KareBooking.V0.http
```

---

# API-endpoints

Hovedversjonene har samme grunnleggende API.

## Pristilbud

```http
GET /quote?start=...&hours=2&customerEmail=...
```

## Opprett booking

```http
POST /bookings
Content-Type: application/json

{
  "customerEmail": "ada@example.test",
  "start": "2030-01-14T12:00:00",
  "hours": 2
}
```

## Se alle bookinger

```http
GET /bookings
```

## Hent én booking

```http
GET /bookings/{id}
```

## Avbestill

```http
POST /bookings/{id}/cancel
```

Kopier booking-id fra responsen på `POST /bookings` inn i `.http`-filen før cancellation-requesten kjøres.

---

# In-memory og fake Infrastructure

Dette er undervisningskode.

API-ene bruker blant annet:

```text
InMemoryBookingRepository
SystemClock
ConsoleEmailSender
FakePaymentGateway
ConsoleAuditLog
```

Ingen ekte e-post sendes.

Ingen ekte penger flyttes.

State ligger i minnet og forsvinner når API-prosessen restartes.

Dette er bevisst: undervisningen skal fokusere på domain modeling, programmeringsstil og arkitektur – ikke oppsett av eksterne tjenester.

V0–V4 og V6 fokuserer på domenemodellering og arkitektur. In-memory-implementasjonene gjør ikke
hele read-decide-write-flyten concurrency-safe og løser ikke concurrent writes på produksjonsnivå.
I en ekte persistent applikasjon brukes transaksjoner og optimistic concurrency fra tidligere
økter i Backend Light. Optional Aggregate Root beholder sin egen demonstrasjon av stale snapshots.

---

# Tester

Testene bruker:

```text
NUnit
```

Forretningsreglene testes på tvers av versjonene, slik at implementasjonene kan sammenlignes mot samme forventede oppførsel.

Blant annet testes:

- helg
- arbeidstid
- hel klokktime
- varighet
- bookingfrist
- lokale tidspunkt og uke-/årsgrenser
- pris
- overlap
- avbestillingsgrenser
- gjentatt avbestilling
- Value Object-invariants
- Domain Events
- pure functions
- immutable state
- `Result<T>.Map`
- `Result<T>.FlatMap`
- optimistic concurrency i Aggregate Root-versjonen

Ved siste fullstendige verifikasjon:

```text
372 / 372 tester bestod (16. oktober 2026)
0 skipped
```

---

# Foreslått undervisningsbruk

Ikke les repositoryet fra start til slutt som en lærebok.

Åpne heller to versjoner side om side.

En mulig rekkefølge er:

1. **V0 → V1:** Hvor bør en prisregel bo?
2. **V1 → V2:** Kan vi representere start kl. 12:37?
3. **V2 → V3:** Hva er selve domenebeslutningen, og hva er bare en reaksjon?
4. **V3 → V4 (mandag):** Hvor stopper DI? Kan hele use caset testes uten I/O?
5. **V5 Pure Functions (onsdag):** Hva er pure functions, immutable data, facts og effects?
6. **V4 → V6 (fredag):** Hvordan kan `Map` og `FlatMap` gjøre Result-basert kode mer komponerbar?
7. **Optional Aggregate Root:** Hva beskytter domenemodellen, og hva krever fortsatt concurrency control?

Se:

[Undervisningskart](docs/teaching-map.md)

---

# Actions as Data

V3, V4 og V6 bruker domain events som data:

```text
BookingCreated
BookingCancelled
```

Disse beskriver **forretningsfakta**.

Tekniske handlinger som:

```text
SendEmail
WriteAuditLog
```

er noe annet.

Repositoryet viser den implementerte varianten hvor Core produserer domain events og shell/handlers reagerer på dem, mens V5-labben viser en egen pure Plan og interpreter for SaveBooking, ChargePayment og SendEmail.

Les:

[Forretningsfakta og tekniske handlinger som data](docs/actions-as-data.md)

---

# Videre etter dette repositoryet

Repositoryet implementerer bevisst ikke full:

```text
CQRS
Event Sourcing
microservices
message bus
outbox
saga
```

Men Kåre-caset kan brukes til å forstå hva noen av begrepene betyr.

For eksempel:

```text
BookKare
CancelBooking
```

kan forstås som commands, mens:

```text
GetAvailableTimes
GetBookingOverview
```

er queries.

Og Domain Events gir et naturlig spørsmål:

> Hva om hendelsene ikke bare var varsler, men selve historikken vi lagret?

Les:

[Videre: CQRS og Event Sourcing](docs/where-to-go-next.md)

---

# Viktige forenklinger

Dette er ikke ment som produksjonsarkitektur.

Blant annet:

- repositoryene bruker in-memory state
- betaling lykkes alltid
- e-post er fake/console
- event dispatch er synkron
- det finnes ingen durable message delivery
- state + payment + email er ikke gjort distribuert atomisk
- det finnes ingen outbox
- det finnes ingen autentisering
- V0–V4 og V6 gjør ikke hele read-decide-write-flyten concurrency-safe
- Aggregate Root-versjonen bruker en enkel versjon på snapshotet for å demonstrere optimistic concurrency

Forenklingene er bevisste.

Poenget er at hver versjon skal gjøre **én ny idé synlig** uten at den drukner i produksjonsinfrastruktur.

---

# Dokumentasjon

- [Undervisningskart](docs/teaching-map.md)
- [Arkitektursammenligning](docs/architecture-comparison.md)
- [Domain Events, effects og actions as data](docs/actions-as-data.md)
- [CQRS og Event Sourcing – hvor går veien videre?](docs/where-to-go-next.md)
- [Implementasjonsnotater](IMPLEMENTATION_NOTES.md)

---

# Hovedideen

Ingen av versjonene er fasiten.

Spør gjennom hele repositoryet:

> Hvor bor forretningskunnskapen?

> Hvilke dependencies trenger egentlig Core?

> Hvilke verdier bør ha egne typer?

> Hva er en forretningsbeslutning, og hva er en sideeffekt?

> Kan skjulte inputs gjøres eksplisitte?

> Kan business logic testes som vanlig data inn og data ut?

> Gjør den nye abstraksjonen koden bedre nok til å forsvare kostnaden?

Det er disse spørsmålene Architecture Lab er laget for å utforske.
