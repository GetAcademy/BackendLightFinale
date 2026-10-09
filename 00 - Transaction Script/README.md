# V0 – Transaction Script

## Hva er likt?

Vi beholder det konkrete bookingcaset og de kjente teknikkene Minimal API, service,
repository og DI. Det finnes ikke et separat Core-prosjekt.

## Hva endret vi?

`BookingService.GetQuoteAsync` og `BookInsideGateAsync` inneholder bevisst de samme reglene:
ukedag, klokkeslett, hele timer, frist, overlapp og pris. `CancelAsync` kjenner refusjonsreglene.
Etter beslutningen utfører tjenesten lagring, fake betaling, e-post og audit.
`BookingCalendar` er bare en liten kalenderhjelper for riktig uke-/sommertidsberegning.

## Hvilket problem prøver dette å løse?

Transaction Script er ikke i seg selv «feil». For enkle applikasjoner kan dette være en god løsning.
Problemet oppstår når domenelogikken vokser og blir repetert/spredt.
Prøv å endre ekstratimeprisen: både tilbud og bestilling må oppdateres.
Gjentakelsen er realistisk når to use cases først utvikles hver for seg.

## Hva koster løsningen?

Pluss: direkte kontrollflyt, få typer, lett å følge en request.
Minus: samme regel må vedlikeholdes flere steder; tjenesten blander beslutninger og I/O.
`IClock` er med allerede her fordi frist/refusjon må testes kontrollert. Andre porter representerer faktisk I/O.

## Kode å vise

Start i `KareBooking.V0.Api/Application/BookingService.cs` og sammenlign quote/book.
Se `KareBooking.V0.Tests/BookingRulesTests.cs` og `ShellTests.cs`.
Ikke refaktorer bort gjentakelsen før du har sammenlignet med V1.

Se [rot-README](../README.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
