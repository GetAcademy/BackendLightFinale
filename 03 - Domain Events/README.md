# V3 – Domain Events

V0–V3 bruker samme Onion-struktur: `Api → Core`. Core har `ApplicationServices`, `DomainServices` og `DomainModel`. API har HTTP, composition root og konkrete implementasjoner i `Infrastructure`.

## Hva er likt?

Verdiobjekter, identitet, kontrollert mutasjon og booking-/refusjonsregler beholdes fra V2.
Betaling ligger fortsatt eksplisitt i tjenesten.

## Hva endret vi?

`Booking.Create` returnerer `BookingDecision` med `BookingCreated`.
`Booking.Cancel` returnerer `CancellationDecision` med `BookingCancelled`.
`BookingEventHandler` i `Core/ApplicationServices` reagerer med e-post og audit; Core kjenner ingen adapters.
Hendelser returneres sammen med akkurat denne operasjonen, uten en skjult global event queue.

## Hvilket problem prøver dette å løse?

I V2 besluttet domenet hva som skjer, mens tjenesten navnga alle konsekvensene direkte.
I V3 uttrykker domenet forretningsfakta; en separat reaksjon avgjør følgehendelsene.
Det tidligere «domain operation → e-post» er en kausal kjede via tjenesten, ikke et SMTP-kall i Core.
Dette reduserer koblingen mellom use case-orkestrering og reaksjoner og gir broen til fakta som data.
`SendEmail` er en teknisk handling; `BookingCreated` er et domeneevent.

## Hva koster løsningen?

Pluss: nye reaksjoner kan legges utenfor domenet; domenetestene inspiserer fakta.
Minus: kontrollflyten blir mer indirekte, og feil i en handler krever et bevisst konsistensvalg.
Dispatch skjer synkront etter save/payment. Ingen durable levering eller automatisk retry er lovet.

## Kode å vise

Åpne `KareBooking.V3.Core/DomainModel/DomainEvents.cs`, `Booking.cs` og `Core/ApplicationServices/BookingEventHandler.cs`.
Se `DomainEventTests.cs` og `ShellTests.cs`. Les også `../docs/actions-as-data.md`.
Domain Events gjør ikke lagring og betaling atomisk.

Se [rot-README](../readme.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
