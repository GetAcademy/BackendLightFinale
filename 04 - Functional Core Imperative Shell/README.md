# V4 – Functional Core / Imperative Shell

Uke 10 mandag: separate effects from business decisions.

## Fra V3 til V4

I V3 beskytter Dependency Inversion Core mot konkret infrastructure.
Core/ApplicationServices/BookingService bruker IBookingRepository, IClock og IPaymentGateway.
I V4 går Functional Core videre: business-beslutningen trenger ikke engang infrastructure-abstraksjoner.

Den gamle effectful Application Service er konseptuelt delt i to:
- Core/ApplicationServices/BookingService: pure application/business orchestration.
- Api/ImperativeShell/BookingHandler: effectful orchestration.

Application Service forsvinner ikke. Før orkestrerte den domenet ved hjelp av services;
nå orkestrerer den domenet ved hjelp av data.

```text
Core
├── ApplicationServices/BookingService.cs
└── DomainModel/  KareSchedule, Booking, TimeSlot, Money, BookingCalendar, Result, DomainEvents

Api (hele Imperative Shell)
├── Program.cs
├── ImperativeShell/
│   ├── BookingHandler.cs
│   ├── BookingEventHandler.cs
│   └── Ports/  IBookingRepository, IClock, IPaymentGateway, IEmailSender, IAuditLog
└── Infrastructure/  in-memory repository og fake adapters
```

V0–V3 har Core som Onion Architecture sitt indre prosjekt. I V4/V6 er hele
Core-prosjektet også faktisk Functional Core. Core har ingen DomainServices-mappe.

## Følg ett kall

```text
Shell reads state + clock + generates id
  → BookingService.Book(command, existing, now, id)
  → KareSchedule / Booking
  → Result<BookingDecision> (new state + Domain Events)
  → Shell saves → charges → dispatches events
```

BookingService er static og har GetQuote, Book og Cancel. Samme input gir samme output.
Den orkestrerer KareSchedule og Booking med vanlige verdier, uten I/O, klokke eller id-generering.
Booking er et immutable snapshot; Cancel returnerer en ny booking og lar originalen være urørt.
Forretningsregler, verdiobjekter, hendelser, HTTP API og fakes videreføres.

BookingHandler tolker Result og utfører lagring, betaling/refusjon og event-dispatch.
BookingEventHandler reagerer med e-post og audit. Ports og Dependency Injection brukes i shell;
Program.cs kobler interfaces til Infrastructure-implementasjoner.

BookingCreated og BookingCancelled er fakta om domenebeslutningen, ikke tekniske commands.
En beslutning i minnet betyr ikke at lagring allerede har lykkes. Dispatch skjer etter save/payment.
Ingen generell effect-plan introduseres her.

## Sammenlign senere med V6

Book bruker en eksplisitt IsFailure-sjekk etter KareSchedule.ValidateSlot før Booking.Create. Ingen Map/FlatMap ennå.
Åpne [BookingService](KareBooking.V4.Core/ApplicationServices/BookingService.cs) og
[BookingHandler](KareBooking.V4.Api/ImperativeShell/BookingHandler.cs) side om side.
FunctionalCoreTests bruker vanlige input; ShellTests kontrollerer effects og avvisninger.

Løsningen gjør ikke save/payment atomisk eller hele read-decide-write-flyten concurrency-safe.
Se [rot-README](../readme.md) for regler, kjøring og forenklinger.

## Rich Domain Model beholdes

Vi splitter den gamle Application Service langs effect-grensen.
BookingHandler eier reads/effects; BookingService eier pure use-case orchestration.
KareSchedule og Booking beholder business-kunnskapen fra V3:

- KareSchedule validerer bookingfrist og overlap/availability, og tilbyr GetQuote og Book.
- TimeSlot beholder invariants for arbeidsdag, arbeidstid og varighet, som i V3.
- BookingCalendar beregner fristen, og Money eier prisberegningen.
- Booking.Create lager BookingDecision med BookingCreated.
- Booking.Cancel eier eligibility og refusjonsgrensene og returnerer CancellationDecision med BookingCancelled.

V3 muterer ved kansellering. V4 bruker with inne i Booking.Cancel og returnerer en ny verdi.
Original booking og eksisterende snapshots forblir urørt. Application Service lager ingen events.

GetQuote delegerer til KareSchedule.GetQuote; Cancel delegerer til Booking.Cancel.
Book viser to domenesteg: ValidateSlot og Booking.Create. Feilsjekken er synlig for sammenligning
med V6; de konkrete reglene ligger fortsatt i DomainModel.