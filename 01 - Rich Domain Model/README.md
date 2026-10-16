# V1 – Rich Domain Model

V0–V3 bruker samme Onion-struktur: `Api → Core`. Core har `ApplicationServices`, `DomainServices` og `DomainModel`. API har HTTP, composition root og konkrete implementasjoner i `Infrastructure`.

## Hva er likt?

Regler, pris, HTTP-kontrakt og adapters er beholdt. Primitive datoer, timer og decimal-priser
er fremdeles synlige. Vi introduserer ingen Aggregate Root-terminologi.

## Hva endret vi?

`KareSchedule.GetQuote` samler kunnskapen om arbeidstid, frist, overlapp og pris.
`KareSchedule.Book` bruker den samme beslutningen. `Booking.Overlaps` og `Booking.Cancel`
uttrykker oppførsel på et objekt med identitet. `IsCancelled` har privat setter.
Core har ingen avhengighet til API. Interfacene ligger i Core/DomainServices; adapterne ligger i Api/Infrastructure.

## Hvilket problem prøver dette å løse?

V0: `BookingService` kjenner reglene og bruker allerede private hjelpemetoder for intern gjenbruk.
V1: domenemodellen kjenner reglene; `BookingService` orkestrerer.
Forskjellen er eierskap til forretningskunnskap. V0 har samme Onion-struktur, men beslutningene ligger hovedsakelig i ApplicationServices/BookingService.
Endring av pris eller refusjon har nå et naturlig hjem. Domenetestene kaller objektene direkte.
Tjenesten leser data, henter nåtid/id, kaller domenet, lagrer resultat og utfører sideeffekter.
Transaction Script er fortsatt et godt og enklere valg for et lite domene; V1 er ikke en
nødvendig løsning for enhver applikasjon.

## Hva koster løsningen?

Pluss: forretningsspråket og ansvaret blir synlig i domenemodellen, som kan testes direkte.
Minus: flere domeneobjekter å lære; de primitive parameterne kan fremdeles være ugyldige.
V1-objektene muterer state kontrollert. Dette er et troverdig objektorientert valg.

## Kode å vise

Åpne `KareBooking.V1.Core/DomainModel/KareSchedule.cs`, `Booking.cs` og `Core/ApplicationServices/BookingService.cs`.
Sammenlign med V0. Diskuter hvem som nå vet hvor mye en avbestilling koster.

Se [rot-README](../readme.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
V0–V4 og V6 gjør ikke hele read-decide-write-flyten concurrency-safe. I en ekte persistent applikasjon
brukes transaksjoner og optimistic concurrency fra tidligere økter i Backend Light.
