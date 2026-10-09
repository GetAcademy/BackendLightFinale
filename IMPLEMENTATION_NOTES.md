# Implementasjonsnotater

## Opprettet

Én Visual Studio-solution med åtte ekte Solution Folders og 23 prosjekter: sju Minimal API-er,
seks selvstendige Core-biblioteker, to konsoller og åtte NUnit-testprosjekter.
Alle hovedsteg implementerer GetQuote, Book og Cancel med de samme forretningsreglene.
Det finnes `.http`-fil og launch profile for hvert API, rot-/steg-README-er, undervisningskart,
arkitektursammenligning, actions-as-data og videre-lesning om CQRS/Event Sourcing.

## Valg

- .NET SDK 10.0.401 / net10.0, høyeste installerte stabile SDK. Ingen preview-avhengighet.
- NUnit 4.3.2, NUnit3TestAdapter 5.0.0 og Microsoft.NET.Test.Sdk 17.14.1. NUnit erstatter
  xUnit etter brukerens eksplisitte oppdatering. Ingen domene-/functional-frameworks.
- API-prosjektet inneholder application og infrastructure, for å begrense antall prosjekter.
  Core har ingen I/O-porter. Ingen felles domene-library eller lenkede kildefiler mellom versjoner.
- V4/V5 refererer til Functional Core/Composition. Pure Functions er et separat steg 04.
- Tid er DateTimeOffset og eksplisitt Europe/Oslo-regelsett. Ukestart er mandag; fredag kl. 16
  inkluderer grensen. Refusjon bruker faktisk forløpt tid (168/48 timer), ikke datoer alene.
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
atomisk save+payment eller durable levering. Shells i hovedsporet serialiserer writes i én prosess.
Aggregate-siden demonstrerer optimistisk concurrency med version på snapshotet.
Den første versjonens gjentakelse og duplisert kode/tester mellom versjoner er pedagogisk tilsiktet.
Labens JSON eksponerer enkle domenedata; det er ingen omfattende DTO-/mapping-stack.

## Verifikasjon

Verifisert 9. oktober 2026 med .NET SDK 10.0.401:

- `dotnet build`: alle 23 prosjekter bygger, 0 feil og 0 advarsler.
- `dotnet test --logger trx --results-directory TestResults`: **378 av 378 NUnit-tester grønne**,
  ingen skipped. Testresultatene ligger lokalt i git-ignorert `TestResults/`.
- Begge konsollprogrammer er kjørt og viser de forventede eksemplene.
- Alle sju API-er er startet og kontrollert over HTTP: quote/pris, create, overlappavvisning,
  bookingoversikt, hent booking, cancel/refusjon, gjentatt cancel, frigjort tid, ugyldig
  klokkeslett, brøkdelstimer i JSON og ukjent id. Prosessene er stoppet etter kontrollen.
- Solutionen har åtte faktiske Solution Folders med alle 23 prosjekter riktig nested.
  Referanser holder seg innen hvert steg. Namespaces, NUnit-pakker, I/O-fri funksjonell Core,
  sju `.http`-filer og lenker i lab-dokumentasjonen er kontrollert.

| Testprosjekt | Bestått |
| --- | ---: |
| V0 | 61 |
| V1 | 44 |
| V2 | 51 |
| V3 | 52 |
| Pure Functions | 7 |
| V4 Functional Core | 53 |
| V5 Composition | 57 |
| Optional Aggregate Root | 53 |
| **Totalt** | **378** |
