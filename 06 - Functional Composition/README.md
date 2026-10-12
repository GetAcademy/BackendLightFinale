# V5 – Functional Composition

Alle hovedversjonene bruker samme Onion-struktur: `Api → Core`. Core har `ApplicationServices`, `DomainServices` og `DomainModel`. API har HTTP, composition root og konkrete implementasjoner i `Infrastructure`.

## Hva er likt?

V4s funksjonsgrense, immutable state, events, I/O-shell og regler beholdes.
Det er fortsatt vanlig C# og egen liten Result-type.

## Hva endret vi?

`Result<T>.Map` transformerer en vellykket verdi. `Bind` kjeder et steg som selv returnerer Result;
det unngår `Result<Result<T>>`. Bind kalles ofte FlatMap andre steder.
Book-workflowen leses som Create TimeSlot → ValidateAvailability → Create Booking → ToDecision.
GetQuote har også en kjede. Cancel har få naturlige steg og beholder en rett frem implementasjon.
`KareBooking.Composition.Console` viser Func, Action, høyereordensfunksjon, currying,
partial application og vanlig f/g-komposisjon før bookingpipelinen.

## Hvilket problem prøver dette å løse?

I V4 brytes business-flowen av samme failure-sjekk mellom hvert steg.
Map/Bind lar oss holde flyten synlig og bevare den første feilen. Et senere steg blir aldri kalt ved failure.
Disse funksjonene fanger ikke exceptions; programmeringsfeil og I/O-feil må håndteres separat.
Dette er praktisk composition, ikke en forelesning om monader.

## Hva koster løsningen?

Pluss: synlig business-flow og mindre repetert feilhåndtering.
Minus: studentene må lære forskjellen på Map og Bind; lange lambda-kjeder kan bli vanskeligere å feilsøke.
Bruk navngitte steg og behold enkel if-kode der det er mer lesbart.

## Kode å vise

Åpne `KareBooking.V5.Core/DomainModel/Result.cs`, `BookingWorkflow.cs`, `ResultTests.cs` og konsollens `Program.cs`.
Sammenlign bare Book-metoden med V4 først. Se hvor kontrollflyten stopper ved opptatt tid.

Se [rot-README](../readme.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
