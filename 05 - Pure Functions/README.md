# V5 – Pure Functions

Uke 10 onsdag: generaliser ideene fra V4 i et selvstendig Console-laboratorium.
Ingen ASP.NET, repositories eller Dependency Injection er nødvendig.

```powershell
dotnet run --project "05 - Pure Functions/KareBooking.PureFunctions.Console"
```

[Examples.cs](KareBooking.PureFunctions.Console/Examples.cs) sammenligner små før/etter-eksempler:

| Tema | Eksempel |
| --- | --- |
| Pure function | Price(hours): samme input gir samme output, ingen observerbare sideeffekter |
| Hidden inputs | DateTime.Now, Guid.NewGuid() og Random.Shared blir eksplisitte parametre |
| I/O/effects | File.ReadAllText leser omverdenen; Console.WriteLine endrer den |
| Mutation | Cancel endrer en MutableBooking som andre kan observere |
| Immutable data | record + with returnerer en ny booking; originalen beholdes |

Records gir ikke automatisk dyp immutability: en record kan inneholde en mutable liste,
og with kopierer referansen til listen. Price illustrerer bare én beregning.

## Facts og effects som data

[Effects.cs](KareBooking.PureFunctions.Console/Effects.cs) skiller to konsepter:
- BookingCreated = fact as data = dette har skjedd i domenet. Koblingen til uke 9s Domain Events.
- SaveBooking, ChargePayment og SendEmail = actions/effects as data = dette ønsker programmet utført.

BookingCreated arver ikke Effect. De planlagte handlingene er ikke Domain Events.
EffectExamples.Plan er pure og returnerer IReadOnlyList<Effect>; den gjør ingen I/O.
Interpret bruker en enkel switch og Console.WriteLine for å simulere repository, payment gateway
og email sender. Reell database/nettverk ville også gjort interpreteren effectful.
Ingen ekte database, betaling eller e-post er koblet til.

Dette er en separat konseptlab. Kåre V4/V6 bruker direkte effects i shell, ikke denne effect-planen.
Functions as values og composition kommer på fredag i V6.

Se [rot-README](../readme.md) og [facts vs effects](../docs/actions-as-data.md).
