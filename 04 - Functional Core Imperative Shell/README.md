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
└── DomainModel/  Booking, TimeSlot, Money, BookingCalendar, Result, DomainEvents

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
  → Result<BookingDecision> (new state + Domain Events)
  → Shell saves → charges → dispatches events
```

BookingService er static og har GetQuote, Book og Cancel. Samme input gir samme output.
Den kaller TimeSlot, Booking og Money med vanlige verdier, uten I/O, klokke eller id-generering.
Booking er et immutable snapshot; Cancel returnerer en ny booking og lar originalen være urørt.
Forretningsregler, verdiobjekter, hendelser, HTTP API og fakes videreføres.

BookingHandler tolker Result og utfører lagring, betaling/refusjon og event-dispatch.
BookingEventHandler reagerer med e-post og audit. Ports og Dependency Injection brukes i shell;
Program.cs kobler interfaces til Infrastructure-implementasjoner.

BookingCreated og BookingCancelled er fakta om domenebeslutningen, ikke tekniske commands.
En beslutning i minnet betyr ikke at lagring allerede har lykkes. Dispatch skjer etter save/payment.
Ingen generell effect-plan introduseres her.

## Sammenlign senere med V6

Book bruker eksplisitte IsFailure-sjekker mellom stegene. Ingen Map/FlatMap ennå.
Åpne [BookingService](KareBooking.V4.Core/ApplicationServices/BookingService.cs) og
[BookingHandler](KareBooking.V4.Api/ImperativeShell/BookingHandler.cs) side om side.
FunctionalCoreTests bruker vanlige input; ShellTests kontrollerer effects og avvisninger.

Løsningen gjør ikke save/payment atomisk eller hele read-decide-write-flyten concurrency-safe.
Se [rot-README](../readme.md) for regler, kjøring og forenklinger.
