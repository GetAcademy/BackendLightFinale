# Optional – Aggregate Root

## Hva er likt?

Denne sidegrenen kopierer den rike modellen med TimeSlot og Money og egne tester.
Den er uavhengig av V0–V4 og V6. Events, event sourcing og CQRS er ikke nødvendige her.

## Hva endret vi?

`KareSchedule` eier den private bookinglisten og er eneste inngang til Book/Cancel.
`Booking.Create` og `Booking.Cancel` er internal. Read-visningen er readonly og gir kopier.
Rooten beskytter ingen-overlapp og unik id på tvers av children; et barn kan ikke kjenne alle søsknene.
`IScheduleRepository` laster/lagrer hele rooten, uten et repository per booking/tabell.
Repository-et bruker versjon på snapshot og avviser stale writes. Det er en teaching fake uten trådsikkerhet; testen lagrer to tidligere innleste snapshots etter hverandre.

## Hvilket problem prøver dette å løse?

En booking kan være gyldig alene og likevel overlappe en annen booking.
Dette er en faktisk consistency boundary. Rooten beskytter endringer innen én innlest modell;
optimistisk concurrency beskytter lagring mellom to samtidig innleste modeller.
AggregateTests viser at begge snapshots kan ta en gyldig lokal beslutning, mens bare én save lykkes.
Et vellykket quote reserverer fortsatt ikke tid.

## Hva koster løsningen?

Pluss: invariants for samlingen har én inngang, og lagringskontrakten uttrykker grensen.
Minus: hele Kåres kalender er én grov root, med voksende state og contention.
Et produksjonssystem kunne delt etter uke, siden dette caset ikke tillater booking over døgn/uker.
Conflict gir failure og ingen betaling; ingen skjult automatisk retry av operasjonen.

## Kode å vise

Åpne `KareBooking.Aggregates.Core/KareSchedule.cs`, `Application/Ports.cs` i API-et,
`InMemoryScheduleRepository.cs` og `AggregateTests.cs`.
Diskuter hvorfor Aggregate Root og rowversion løser to ulike deler av konsistensproblemet.

Se [rot-README](../readme.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
