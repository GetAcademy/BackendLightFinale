# V6 – Functional Composition / Railway Oriented Programming

Uke 10 fredag: compose pure functions that can fail using Result<T>, Map and FlatMap.

## Samme arkitektur som V4

Core/ApplicationServices/BookingService er fortsatt pure og static.
Core/DomainModel inneholder domenet og Result. Api/ImperativeShell har BookingHandler,
BookingEventHandler og Ports; Infrastructure implementerer portene og Program.cs kobler DI.

BookingHandler, event handling, Ports, Infrastructure og Program.cs er identiske med V4
bortsett fra versjonsnamespace. Nåtid, id og state kommer fortsatt utenfra.
Vi endrer hvordan den rene logikken skrives, ikke arkitekturgrensen.

## Begynn med funksjoner som verdier

[Konsollen](KareBooking.Composition.Console/Program.cs) viser en Func<int, int> i en variabel.
Twice tar en funksjon som parameter og returnerer en ny funksjon: en higher-order function.
Compose kobler A → B → C. Deretter kjøres Kåres Book-pipeline.

```powershell
dotnet run --project "06 - Functional Composition/KareBooking.Composition.Console"
```

## Fra gjentatte sjekker til en pipeline

V4s Book sjekker IsFailure etter TimeSlot.Create, tilgjengelighet og Booking.Create.
[V6s BookingService](KareBooking.V6.Core/ApplicationServices/BookingService.cs) uttrykker samme kjede:

```csharp
return TimeSlot.Create(command.Start, command.Hours)
    .FlatMap(slot => ValidateAvailability(slot, existing, now))
    .FlatMap(slot => Booking.Create(id, command.CustomerEmail, slot))
    .Map(ToDecision);
```

[Result<T>](KareBooking.V6.Core/DomainModel/Result.cs) har to små metoder:
- Map tar T → U og pakker den vellykkede verdien i Result<U>.
- FlatMap tar T → Result<U> og returnerer resultatet direkte, uten Result<Result<U>>.

FlatMap kalles ofte Bind i funksjonell programmering og biblioteker. Vi bruker bare navnet FlatMap.
Ved Failure kalles ingen av funksjonene; den opprinnelige feilen propageres.

## Railway Oriented Programming

```text
SUCCESS ───●────────●────────●────► decision
           │        │        │
FAILURE ───▼────────▼────────▼────► first error
```

Success kjører neste funksjon. Failure hopper over resten av success-stegene.
ResultTests viser feil ved starten og midt i kjeden, og at senere steg ikke kjøres.
Konsollen viser også en avvist booking. Result + Map + FlatMap er hele mekanismen.

Cancel beholder enkel Result-kode fordi det er tydelig her. GetQuote beholder samme
valideringsrekkefølge som V4. Composition er et verktøy, ikke et krav til alle metoder.
Map/FlatMap fanger ikke exceptions og inneholder ingen repository-, payment- eller andre I/O-kall.

Se [V4](../04%20-%20Functional%20Core%20Imperative%20Shell/README.md) og
[rot-README](../readme.md) for API, forretningsregler og forenklinger.
