# V0 – Transaction Script

V0–V3 bruker samme Onion-struktur: `Api → Core`. Core har `ApplicationServices`, `DomainServices` og `DomainModel`. API har HTTP, composition root og konkrete implementasjoner i `Infrastructure`.

## Hva er likt?

Vi beholder det konkrete bookingcaset og de kjente teknikkene Minimal API, service,
repository og DI. V0 har API, Core og Tests, akkurat som V1.
`KareBooking.V0.Core` inneholder `Booking`, `BookingCalendar`, `BookCommand` og `Result<T>`.
Denne domenemodellen er relativt anemisk: et prosjektnavn gir ikke i seg selv en rik domenemodell.

## Hva endret vi?

`BookingService.GetQuoteAsync` og `BookAsync` bruker den private hjelpemetoden
`EvaluateBookingAsync` for e-post, ukedag, klokkeslett, hele timer, varighet, frist, overlapp og pris.
Extract Method gir enkel intern gjenbruk. `CancelAsync` kjenner avbestillings- og refusjonsreglene.
Etter beslutningen utfører tjenesten lagring, fake betaling, e-post og audit.
`BookingCalendar` er bare en liten kalenderhjelper for beregning av fredag i forrige uke.

## Hvilket problem prøver dette å løse?

V0 er en legitim og ryddig Transaction Script-løsning. For et lite domene kan dette være et
godt og enklere valg. Prøv å endre ekstratimeprisen: én endring i hjelpemetoden er nok.
Spørsmålet videre er hvem som eier forretningskunnskapen. `BookingService` kjenner nesten
alle reglene samtidig som den orkestrerer I/O; typene i DomainModel tar få beslutninger selv.

> If the business rules are reusable and important enough to have names, should they merely be private helper methods inside an application service, or do they belong to the domain model itself?

## Hva koster løsningen?

Pluss: direkte kontrollflyt, få typer, lett å følge en request.
Minus: tjenesten har ansvar for både forretningsbeslutninger og I/O.
`IClock` er med allerede her fordi frist/refusjon må testes kontrollert. Andre porter representerer faktisk I/O.

## Kode å vise

Start i `KareBooking.V0.Core/ApplicationServices/BookingService.cs`: følg quote/book til den felles
hjelpemetoden og les deretter cancellation. Sammenlign med V1s `BookingService`,
`KareSchedule` og `Booking`. V0 kjenner reglene i tjenesten; V1 kjenner dem i domenemodellen.
Se `KareBooking.V0.Tests/BookingRulesTests.cs`, `QuoteAndBookingTests.cs` og `ShellTests.cs`.

Se [rot-README](../readme.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
V0–V4 og V6 gjør ikke hele read-decide-write-flyten concurrency-safe. I en ekte persistent applikasjon
brukes transaksjoner og optimistic concurrency fra tidligere økter i Backend Light.
