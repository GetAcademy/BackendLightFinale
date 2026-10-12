# V4 – Functional Core / Imperative Shell

Alle hovedversjonene bruker samme Onion-struktur: `Api → Core`. Core har `ApplicationServices`, `DomainServices` og `DomainModel`. API har HTTP, composition root og konkrete implementasjoner i `Infrastructure`.

## Hva er likt?

Caset, TimeSlot, Money og domain events fra V3 beholdes.
API, I/O-porter og adaptere er kjent stoff. Core trenger fortsatt ingen eksterne pakker.

## Hva endret vi?

`BookingWorkflow.Book(command, existing, now, id)` er en statisk funksjon.
Nåtid, eksisterende bookings og generert id kommer inn som parametre.
Ut kommer `Result<BookingDecision>`: immutable state + domain events.
Cancel bruker `with` via `AsCancelled` og lar originalen være urørt.
Booking er nå record fordi den fungerer som immutable snapshot ved funksjonsgrensen.
DomainModel/BookingWorkflow bruker bare vanlige data. ApplicationServices/BookingService bruker interfacene i DomainServices for I/O.

## Hvilket problem prøver dette å løse?

V3 kunne allerede testes uten I/O, men returnerte en mutert entity og brukte en objektmodell.
V4 gjør hele workflow-grensen eksplisitt og uten mutasjon: data inn → beslutning → data ut.
Shell-et henter én now-verdi og ett id før kallet og tolker resultatet etterpå.
FunctionalCoreTests bruker vanlige verdier, uten I/O-mocks.
**Functional Core trenger ikke DI. Shell-et i Onion Core bruker fortsatt DI** til repository, klokke, betaling, e-post og audit.

## Hva koster løsningen?

Pluss: enkel deterministisk testing, synlige dependencies, original state endres ikke.
Minus: flere data må sendes inn/ut, og store snapshots kan bli kostbare.
Denne casens rike DomainModel hadde allerede god I/O-separasjon; forbedringen er eksplisitt workflow og immutability,
ikke at vi skaper et kunstig repository-avhengig domene for å «fikse» det.

## Kode å vise

Åpne `KareBooking.V4.Core/DomainModel/BookingWorkflow.cs`, `Booking.cs` og `Core/ApplicationServices/BookingService.cs`.
Sammenlign CancellationDecision i V3/V4. Se `../docs/actions-as-data.md` for valg av variant A.

Se [rot-README](../readme.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
