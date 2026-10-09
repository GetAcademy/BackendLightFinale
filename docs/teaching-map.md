# Undervisningskart – for læreren

La studentene åpne to steg side om side. Begynn med en regelendring, og spør hvor den må gjøres.
Ikke presenter hver nye teknikk som en automatisk forbedring.

| Version | Problem experienced | New concept | Main code to show | Suggested discussion question |
| --- | --- | --- | --- | --- |
| V0, steg 00 | Pris/validering gjentas i quote og book | Transaction Script | V0.Api/Application/BookingService.cs | Når er dette nok? Hva må endres om ekstratimen blir dyrere? |
| V1, steg 01 | Forretningskunnskap bor i use case-tjenester | Rich Domain Model | V1.Core/KareSchedule.cs, Booking.cs | Hvilke beslutninger tilhører domenet, og hvilke tilhører orkestreringen? |
| V2, steg 02 | Primitive verdier tillater ugyldig tid og utydelig penger | Value Objects | V2.Core/TimeSlot.cs, Money.cs; ValueObjectTests | Hvilke regler kan TimeSlot garantere, og hvilke trenger kalenderen? |
| V3, steg 03 | Tjenesten kjenner hver følgehendelse | Domain Events | V3.Core/DomainEvents.cs; V3.Api/Application/BookingEventHandler.cs | Er SendEmail et forretningsfaktum? Hva om audit feiler? |
| Pure, steg 04 | Skjult klokke/random/I/O og mutasjon gjør beregning uforutsigbar | Pure functions / records / with | PureFunctions.Console/Examples.cs | Hvorfor gjør ikke record alene en funksjon pure? |
| V4, steg 05 | Workflow-grensen og mutasjonen kan gjøres mer eksplisitt | Functional Core / Imperative Shell | V4.Core/BookingWorkflow.cs; FunctionalCoreTests | Hvor stopper DI, og hvem skaffer now/id/tidssone? |
| V5, steg 06 | Samme feilsjekk avbryter flyten mellom steg | Map / Bind / composition | V5.Core/Result.cs, BookingWorkflow.cs; Composition.Console | Hvorfor trenger et steg som kan feile Bind, mens ToDecision bruker Map? |
| Optional, steg 90 | En entity beskytter ikke invariants mellom søsken | Aggregate Root + optimistic save | Aggregates.Core/KareSchedule.cs; AggregateTests | Hvorfor kan to gyldige roots likevel kollidere ved save? |

Stiene i tabellen er forkortet; prosjektene ligger i de navngitte solution folders.

## Forslag til rekkefølge

1. V0 → V1: endre en prisregel og sammenlign ansvarsplassering.
2. V1 → V2: forsøk start 12:37 og diskuter representerbar state.
3. V2 → V3: følg booking til e-post og skil fakta fra reaksjon.
4. Pure Functions: 10–15 minutter med tid, tilfeldighet og `with`.
5. V3 → V4: kall hele workflowen direkte fra en test; original state er urørt.
6. V4 → V5: skriv samme feilkjede med if og Bind, og sammenlign lesbarhet.
7. Optional: last to snapshots og vis at root-regler og samtidighetskontroll begge trengs.

La studentene kjøre `BookingRulesTests` først. Regeltestene gjentas bevisst i hver versjon og
gir sammenlignbare eksempler på helg, arbeidstid, frist, pris, overlapp og refusjon.
ShellTests dekker I/O-reaksjonene; rene domenetester bruker ingen I/O-mocks.

Undervis ikke CQRS og Event Sourcing som ferdig kode. Bruk [videre-lesning](where-to-go-next.md)
til å gi begrepene et konkret feste i det kjente caset.
