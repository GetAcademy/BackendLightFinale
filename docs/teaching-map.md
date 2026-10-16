# Undervisningskart – for læreren

La studentene åpne to steg side om side. Begynn med en regelendring, og spør hvor den må gjøres.
Ikke presenter hver nye teknikk som en automatisk forbedring.

| Version | Problem experienced | New concept | Main code to show | Suggested discussion question |
| --- | --- | --- | --- | --- |
| V0, steg 00 | Tjenesten eier regler og orkestrerer I/O; Extract Method gir allerede intern gjenbruk | Transaction Script | V0.Core/ApplicationServices/BookingService.cs; V0.Core/DomainModel/Booking.cs | V0 er ryddig. Bør navngitte regler bo i private hjelpemetoder eller i domenemodellen? |
| V1, steg 01 | Forretningskunnskap bor i use case-tjenester | Rich Domain Model | V1.Core/DomainModel/KareSchedule.cs, Booking.cs | Hvilke beslutninger tilhører domenet, og hvilke tilhører orkestreringen? |
| V2, steg 02 | Primitive verdier tillater ugyldig tid og utydelig penger | Value Objects | V2.Core/DomainModel/TimeSlot.cs, Money.cs; ValueObjectTests | Hvilke regler kan TimeSlot garantere, og hvilke trenger kalenderen? |
| V3, steg 03 | Tjenesten kjenner hver følgehendelse | Domain Events | V3.Core/DomainModel/DomainEvents.cs; V3.Core/ApplicationServices/BookingEventHandler.cs | Er SendEmail et forretningsfaktum? Hva om audit feiler? |
| V4, steg 04, mandag | Application Service bruker tjenester mot utsiden | Functional Core / Imperative Shell | V4.Core/ApplicationServices/BookingService.cs; V4.Api/ImperativeShell/BookingHandler.cs | Hvor stopper DI, og hvem skaffer now/id? |
| V5, steg 05, onsdag | Generalisere skillet mellom beregninger og effects | Pure functions / immutability / facts og effects som data | PureFunctions.Console/Examples.cs, Effects.cs | Hvorfor er BookingCreated et faktum, mens SaveBooking er en planlagt handling? |
| V6, steg 06, fredag | Samme feilsjekk avbryter flyten mellom steg | Map / FlatMap / Railway Oriented Programming | V6.Core/DomainModel/Result.cs; V6.Core/ApplicationServices/BookingService.cs; Composition.Console | Hvorfor trenger et steg som kan feile FlatMap, mens ToDecision bruker Map? |
| Optional, steg 90 | En entity beskytter ikke invariants mellom søsken | Aggregate Root + optimistic save | Aggregates.Core/KareSchedule.cs; AggregateTests | Hvorfor kan to gyldige roots likevel kollidere ved save? |

Stiene i tabellen er forkortet; prosjektene ligger i de navngitte solution folders.

## Forslag til rekkefølge

1. V0 → V1: endre en prisregel og sammenlign ansvarsplassering. Begge gjenbruker reglene internt.
2. V1 → V2: forsøk start 12:37 og diskuter representerbar state.
3. V2 → V3: følg booking til e-post og skil fakta fra reaksjon.
4. V3 → V4 (mandag): skill pure BookingService fra effectful BookingHandler; kall tjenesten med data.
5. V5 Pure Functions (onsdag): tid, tilfeldighet, I/O, immutability og facts vs effects som data.
6. V4 → V6: skriv samme feilkjede med if og FlatMap, og sammenlign lesbarhet.
7. Optional: last to snapshots og vis at root-regler og samtidighetskontroll begge trengs.

La studentene kjøre `BookingRulesTests` først. Regeltestene gjentas bevisst i hver versjon og
gir sammenlignbare eksempler på helg, arbeidstid, frist, pris, overlapp og refusjon.
ShellTests dekker I/O-reaksjonene; rene domenetester bruker ingen I/O-mocks.

V0s `GetQuoteAsync` og `BookAsync` deler `EvaluateBookingAsync`; `CancelAsync` bestemmer refusjon.
I V1 ligger disse beslutningene i `KareSchedule` og `Booking`, mens tjenesten orkestrerer.
Spør hva V1 forbedrer når V0 allerede er ryddig. Transaction Script kan fortsatt passe et lite domene.

V0–V4 og V6 beskytter ikke hele read-decide-write-flyten mot concurrent writes. Knytt ekte lagring
til tidligere økter om transaksjoner og optimistic concurrency. Steg 90 beholder stale-snapshot-demonstrasjonen.

Undervis ikke CQRS og Event Sourcing som ferdig kode. Bruk [videre-lesning](where-to-go-next.md)
til å gi begrepene et konkret feste i det kjente caset.
