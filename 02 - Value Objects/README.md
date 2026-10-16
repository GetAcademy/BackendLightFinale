# V2 – Value Objects

V0–V3 bruker samme Onion-struktur: `Api → Core`. Core har `ApplicationServices`, `DomainServices` og `DomainModel`. API har HTTP, composition root og konkrete implementasjoner i `Infrastructure`.

## Hva er likt?

Rik domenemodell, orkestrering og forretningsregler er de samme som i V1.
Booking er fortsatt en vanlig class med identitet og kontrollert mutasjon.

## Hva endret vi?

`TimeSlot.Create(start, hours, zone)` kan feile og er eneste vei til et tidsrom.
Den avviser helg, minutter/sekunder, 0/negative timer og slutt etter 16.
Deadline og tilgjengelighet er kontekstregler og blir i `KareSchedule`.
`Money` gir immutable value semantics og eksplisitt NOK. Vi lager ingen typed identifier
bare for å få en ekstra type: Guid er tilstrekkelig for dette læringssteget.

## Hvilket problem prøver dette å løse?

To gevinster: (1) validering/invariants blir samlet, og (2) typesystemet uttrykker domenet sterkere.
`Booking.Create(id, email, slot)` mottar allerede gyldig tid; `Price` er `Money`, ikke en vilkårlig decimal.
Sammenlign primitive argumenter som `Book(customerId, 3, 4, ...)` med et navngitt `TimeSlot`
og en `Money`-verdi. Records brukes for verdier, ikke for entity-mutasjon.

## Hva koster løsningen?

Pluss: ugyldige tidsrom kan ikke representeres, verdi-likhet og mer presise parametre.
Minus: flere typer/factory-resultater og en synlig endring av HTTP-responsen.
Et gyldig TimeSlot betyr fortsatt ikke at kalenderen er ledig. Verdiobjektet vet ikke alt.

## Kode å vise

Åpne `KareBooking.V2.Core/DomainModel/TimeSlot.cs`, `Money.cs` og `ValueObjectTests.cs` i testprosjektet.
Forsøk å lage start 12:37; ingen gyldig `TimeSlot` kommer ut av factoryen.

Se [rot-README](../readme.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
