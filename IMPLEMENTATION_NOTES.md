# Implementasjonsnotater

## Opprettet

Én Visual Studio-solution med åtte ekte Solution Folders og 24 prosjekter: sju Minimal API-er,
sju selvstendige Core-biblioteker, to konsoller og åtte NUnit-testprosjekter.
Alle hovedsteg implementerer GetQuote, Book og Cancel med de samme forretningsreglene.
Det finnes `.http`-fil og launch profile for hvert API, rot-/steg-README-er, undervisningskart,
arkitektursammenligning, actions-as-data og videre-lesning om CQRS/Event Sourcing.

## Valg

- .NET SDK 10.0.401 / net10.0, høyeste installerte stabile SDK. Ingen preview-avhengighet.
- NUnit 4.3.2, NUnit3TestAdapter 5.0.0 og Microsoft.NET.Test.Sdk 17.14.1. NUnit erstatter
  xUnit etter brukerens eksplisitte oppdatering. Ingen domene-/functional-frameworks.
- V0–V5 har Core/ApplicationServices, Core/DomainServices og Core/DomainModel. API inneholder HTTP, composition root og Infrastructure. Ingen felles domene-library eller lenkede kildefiler mellom versjoner.
- V0 har også Core, med Booking, BookingCalendar, BookCommand og Result<T> i namespace KareBooking.V0.DomainModel.
  BookingService eier reglene; GetQuoteAsync og BookAsync deler den private EvaluateBookingAsync.
  V1 flytter kunnskapen til KareSchedule og Booking. Transaction Script kan fortsatt passe et lite domene.
- V4/V5 refererer til Functional Core/Composition. Pure Functions er et separat steg 04.
- Alle tidspunkt er DateTime og lokal tid for Kåres virksomhet. Ukestart er mandag; fredag kl. 16 inkluderer grensen. Refusjon bruker differansen mellom lokale tidspunkt (168/48 timer). IClock beholdes; SystemClock bruker DateTime.Now.
- Overlapp bruker halvåpne intervaller. Avbestilling etter start og gjentatt avbestilling avvises.
- TimeSlot/Money er immutable records med private constructors og validering. Booking er class
  i den rike modellen og immutable record i de funksjonelle versjonene. Guid beholdes som id.
- Variant A for actions as data er implementert. Tekniske effect-data illustreres separat i docs.
- Optional KareSchedule er én grov root med versjonert repository; mer DDD er ikke innført.
- Eksisterende frontend-materiale er bevart. Tidligere rot-README er kopiert til docs med tilpassede lenker.

## Kjøring

```powershell
dotnet build
dotnet test
dotnet run --project "00 - Transaction Script/KareBooking.V0.Api"
dotnet run --project "04 - Pure Functions/KareBooking.PureFunctions.Console"
dotnet run --project "06 - Functional Composition/KareBooking.Composition.Console"
```

API-porter: V0–V5 på 5100–5105, Optional på 5190. Start ett eller flere API-er,
åpne tilhørende `.http`, og kopier booking-id fra create-responsen før cancellation.
Live klokke brukes; testene har faste verdier og bruker ikke eksterne tjenester.

## Mest interessante steder

1. V0.BookingService kontra V1.KareSchedule: hvor bor reglene?
2. V2.TimeSlot/Money: invariants kontra kontekstregler, og typebetydning.
3. V3.DomainEvents/BookingEventHandler: fakta kontra tekniske konsekvenser.
4. V4.BookingWorkflow/FunctionalCoreTests: hele workflowen uten I/O eller mutasjon.
5. V5.Result/BookingWorkflow/ResultTests: Map, Bind og short-circuiting.
6. Optional.AggregateTests: lokale invariants kontra stale writes ved lagring.

## Bevisste forenklinger

In-memory state, enkel e-postvalidering, alltid vellykket fake betaling og synkron event-dispatch.
Ingen event store, message bus, outbox, saga eller autentisering; ingen produksjonsgaranti for
atomisk save+payment eller durable levering. V0–V5 gjør ikke hele read-decide-write-flyten
concurrency-safe; bruk transaksjoner og optimistic concurrency fra tidligere Backend Light-økter ved ekte lagring.
Aggregate-siden demonstrerer optimistisk concurrency med version på snapshotet.
Selvstendig kode og gjentatte regeltester mellom versjoner gjør sammenligningen enklere.
Samtidighetstesten og BlockingReadRepository er fjernet fra V0–V5. V0s QuoteAndBookingTests
beskriver konsistent validering, pris og tilgjengelighet, uten å beskrive implementasjonen som duplisert.
Labens JSON eksponerer enkle domenedata; det er ingen omfattende DTO-/mapping-stack.

## Struktur etter refaktorering

V0–V5 har BookingService i Core/ApplicationServices og fem separate interface-filer i Core/DomainServices. V3–V5 har også BookingEventHandler i ApplicationServices. Domenetyper, BookCommand og Result ligger i DomainModel. V4 og V5 har identisk mappe- og filstruktur. Namespaces følger mappene; Core refererer ikke API.

Optional Aggregate Root beholder sin eksisterende mappestruktur, siden mappeendringen gjelder V0–V5. Den bruker samme lokale DateTime-forenkling og et repository uten låsing, men beholder version, expectedVersion og TrySaveAsync. Stale-snapshot-testen er beholdt og bestått.

## Verifikasjon

Verifisert 12. oktober 2026 med .NET SDK 10.0.401:

- `dotnet build BackendLight.ArchitectureLab.sln --no-restore -m:1`: alle 24 prosjekter bygger, 0 feil og 0 advarsler.
- `dotnet test BackendLight.ArchitectureLab.sln --no-restore -m:1 --logger trx --results-directory TestResults`: **372 av 372 NUnit-tester grønne**,
  ingen skipped. Testresultatene ligger lokalt i git-ignorert `TestResults/`.
  Testhost-forbindelsen fikk timeout i sandboxen; hele testsuiten kjørte grønt utenfor sandboxen.
- Alle sju `.http`-filer er kontrollert mot routes, lokal DateTime og datoer uten soneangivelse.
  Endpoints og IClock er beholdt; datoene sendes uten soneangivelse.
- Solutionen har åtte faktiske Solution Folders med alle 24 prosjekter riktig nested.
  Alle 23 project references finnes og holder seg innen hvert steg. V0 API og Tests refererer til V0 Core.
- Repositoryene er enkle teaching fakes uten låsing eller andre synkroniseringsmekanismer.
  Vanlige overlap-tester og hele Optional Aggregate Root-grenen er beholdt.
- Pedagogisk kontroll: V0 deler regler internt, men tjenesten tar beslutningene. V1s tjeneste
  orkestrerer KareSchedule og Booking, som eier booking-, overlap- og cancellation/refund-reglene.
  Ingen nye patterns eller use cases er innført. Build-/testartefakter er git-ignorert.

| Testprosjekt | Bestått |
| --- | ---: |
| V0 | 60 |
| V1 | 43 |
| V2 | 50 |
| V3 | 51 |
| Pure Functions | 7 |
| V4 Functional Core | 52 |
| V5 Composition | 56 |
| Optional Aggregate Root | 53 |
| **Totalt** | **372** |
