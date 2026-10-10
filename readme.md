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
Pure Functions
↓
Functional Core / Imperative Shell
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

# Forretningsregler

## Arbeidstid

Kåre arbeider:

```text
mandag–fredag
kl. 12:00–16:00
```

Tidssonereglene bruker:

```text
Europe/Oslo
```

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

Regelen håndteres i `Europe/Oslo`, inkludert overganger mellom sommer- og vintertid.

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

Refusjonen avhenger av hvor lang faktisk tid det er igjen til oppdraget starter.

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

Solutionen inneholder åtte Solution Folders:

```text
00 - Transaction Script
01 - Rich Domain Model
02 - Value Objects
03 - Domain Events
04 - Pure Functions
05 - Functional Core Imperative Shell
06 - Functional Composition
90 - Optional Aggregate Root
```

Versjonene er bevisst selvstendige.

Det finnes ikke ett felles domain-library som alle versjoner deler. Litt duplisering mellom versjonene gjør det lettere å åpne to steg side om side og sammenligne dem.

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

V0 er en legitim, ryddig Transaction Script-løsning. Core inneholder relativt anemiske typer;
`BookingService` eier nesten all forretningskunnskap og orkestrerer I/O.

Transaction Script kan være et godt og enklere valg for et lite domene. V1 utforsker om viktige,
navngitte regler bør ha sitt hjem i domenemodellen.

**Start her:**

```text
00 - Transaction Script/KareBooking.V0.Api/Application/BookingService.cs
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

Forretningskunnskap flyttes inn i Core. V0s `BookingService` kjenner reglene; i V1 kjenner
domenemodellen reglene og `BookingService` orkestrerer. Begge har allerede intern regelgjenbruk.

Blant annet får vi:

```text
KareSchedule
Booking
```

Application-laget blir mer:

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

> Hva er egentlig det vi ønsker å ha i Core?

**Start her:**

```text
01 - Rich Domain Model/KareBooking.V1.Core/KareSchedule.cs
01 - Rich Domain Model/KareBooking.V1.Core/Booking.cs
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
02 - Value Objects/KareBooking.V2.Core/TimeSlot.cs
02 - Value Objects/KareBooking.V2.Core/Money.cs
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
03 - Domain Events/KareBooking.V3.Core/DomainEvents.cs
03 - Domain Events/KareBooking.V3.Api/Application/BookingEventHandler.cs
```

[Les mer om V3](03%20-%20Domain%20Events/README.md)

Se også:

[Forretningsfakta og tekniske handlinger som data](docs/actions-as-data.md)

---

# 04 – Pure Functions

Dette er et kort sidesprang fra API-arkitekturen.

Prosjekter:

```text
KareBooking.PureFunctions.Console
KareBooking.PureFunctions.Tests
```

Vi sammenligner impure og pure funksjoner og ser blant annet på:

- skjult tid
- `Random.Shared`
- `Guid.NewGuid()`
- fil-I/O
- mutasjon
- immutable records
- `with`

En viktig idé er:

```text
samme input
→ samme output
```

uten observerbare sideeffekter.

Ting som:

```text
nåværende tidspunkt
randomness
generert id
data lest fra disk
```

kan betraktes som input til en beregning, selv når inputen opprinnelig er skjult inne i funksjonen.

**Start her:**

```text
04 - Pure Functions/KareBooking.PureFunctions.Console/Examples.cs
```

[Les mer om Pure Functions](04%20-%20Pure%20Functions/README.md)

---

# 05 – Functional Core / Imperative Shell

**Problem:** Kan vi gjøre selve workflowen eksplisitt deterministisk og flytte all I/O til utsiden?

Prosjekter:

```text
KareBooking.V4.Api
KareBooking.V4.Core
KareBooking.V4.Tests
```

Shell-et skaffer ting som:

```text
eksisterende bookings
nåværende tidspunkt
generert id
tidssone
```

og sender dem som vanlige data til Core.

For eksempel:

```text
data inn
↓
BookingWorkflow
↓
business decision
↓
state + domain events ut
```

Core kjenner ikke:

- repository
- database
- `IClock`
- logger
- DI-container
- nettverk
- e-post
- betaling

Dette betyr **ikke** at hele applikasjonen slutter å bruke Dependency Injection.

Det betyr:

> **Core trenger ikke DI.**

Imperative shell bruker fortsatt DI for sine I/O-avhengigheter.

**Start her:**

```text
05 - Functional Core Imperative Shell/KareBooking.V4.Core/BookingWorkflow.cs
```

Se også testene som kaller workflowen direkte uten I/O-mocks.

[Les mer om V4](05%20-%20Functional%20Core%20Imperative%20Shell/README.md)

---

# 06 – Functional Composition

**Problem:** `Result<T>` fungerer, men gjentatte failure-sjekker kan skjule selve business-flowen.

Prosjekter:

```text
KareBooking.V5.Api
KareBooking.V5.Core
KareBooking.V5.Tests
KareBooking.Composition.Console
```

Vi introduserer blant annet:

```text
Func
Action
higher-order functions
currying
partial application
functional composition
```

og deretter:

```text
Result<T>.Map
Result<T>.Bind
```

`Bind` kalles ofte `FlatMap` i andre språk og biblioteker.

Et workflow kan da uttrykkes mer i retning av:

```text
Create TimeSlot
↓
Validate Availability
↓
Create Booking
↓
Create Decision
```

uten at den samme:

```text
if failure → return failure
```

-koden må ligge mellom hvert steg.

Dette er praktisk functional composition, ikke en full introduksjon til funksjonell teori eller monader.

**Start her:**

```text
06 - Functional Composition/KareBooking.V5.Core/Result.cs
06 - Functional Composition/KareBooking.V5.Core/BookingWorkflow.cs
```

og:

```text
06 - Functional Composition/KareBooking.Composition.Console/Program.cs
```

[Les mer om V5](06%20-%20Functional%20Composition/README.md)

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
| V0 Transaction Script | 5100 |
| V1 Rich Domain Model | 5101 |
| V2 Value Objects | 5102 |
| V3 Domain Events | 5103 |
| V4 Functional Core | 5104 |
| V5 Functional Composition | 5105 |
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
  "start": "2030-01-14T12:00:00+01:00",
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

V0–V5 fokuserer på domenemodellering og arkitektur. In-memory-implementasjonene gjør ikke
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
- sommer-/vintertid
- pris
- overlap
- avbestillingsgrenser
- gjentatt avbestilling
- Value Object-invariants
- Domain Events
- pure functions
- immutable state
- `Result<T>.Map`
- `Result<T>.Bind`
- optimistic concurrency i Aggregate Root-versjonen

Ved siste fullstendige verifikasjon:

```text
372 / 372 tester bestod (10. oktober 2026)
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
4. **Pure Functions:** Hva skjer når tid, randomness og I/O blir eksplisitte input?
5. **V3 → V4:** Hvor stopper DI? Kan hele workflowen testes uten I/O?
6. **V4 → V5:** Hvordan kan `Map` og `Bind` gjøre Result-basert kode mer komponerbar?
7. **Optional Aggregate Root:** Hva beskytter domenemodellen, og hva krever fortsatt concurrency control?

Se:

[Undervisningskart](docs/teaching-map.md)

---

# Actions as Data

V3–V5 bruker domain events som data:

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

Repositoryet viser den implementerte varianten hvor Core produserer domain events og shell/handlers reagerer på dem, og diskuterer også hvordan en høyere workflow kunne produsert eksplisitte effect-data.

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
- V0–V5 gjør ikke hele read-decide-write-flyten concurrency-safe
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
