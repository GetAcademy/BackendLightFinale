# V1 – Rich Domain Model

## Hva er likt?

Regler, pris, HTTP-kontrakt og adapters er beholdt. Primitive datoer, timer og decimal-priser
er fremdeles synlige. Vi introduserer ingen Aggregate Root-terminologi.

## Hva endret vi?

`KareSchedule.GetQuote` samler kunnskapen om arbeidstid, frist, overlapp og pris.
`KareSchedule.Book` bruker den samme beslutningen. `Booking.Overlaps` og `Booking.Cancel`
uttrykker oppførsel på et objekt med identitet. `IsCancelled` har privat setter.
Core har ingen avhengighet til API; I/O-porter og adapters ligger i API.

## Hvilket problem prøver dette å løse?

V0: application service visste reglene.
V1: domenet vet reglene; application service orkestrerer.
Endring av pris eller refusjon har nå et naturlig hjem. Domenetestene kaller objektene direkte.
Tjenesten leser data, kaller domenet, lagrer resultat og utfører sideeffekter.

## Hva koster løsningen?

Pluss: forretningsspråket blir synlig og duplisering av regler innen versjonen fjernes.
Minus: flere objekter/prosjekter; de primitive parameterne kan fremdeles være ugyldige.
V1-objektene muterer state kontrollert. Dette er et troverdig objektorientert valg.

## Kode å vise

Åpne `KareBooking.V1.Core/KareSchedule.cs`, `Booking.cs` og API-ets `BookingService.cs`.
Sammenlign med V0. Diskuter hvem som nå vet hvor mye en avbestilling koster.

Se [rot-README](../README.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
