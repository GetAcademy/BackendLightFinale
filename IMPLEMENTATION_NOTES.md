# Implementasjonsnotater

## Struktur og pedagogisk rekkefølge

Solutionen har ti Solution Folders og 26 prosjekter, inkludert to Prelude-varianter.
Uke 10 er nå:
- 04 / V4 mandag: Functional Core / Imperative Shell.
- 05 / V5 onsdag: Pure Functions, separat Console-lab.
- 06 / V6 fredag: Functional Composition / Railway Oriented Programming.

Pure Functions-mappen er flyttet fra 04 til 05; Functional Core-mappen fra 05 til 04.
Composition-prosjektene, namespaces og HTTP-filen er omdøpt fra KareBooking.V5 til KareBooking.V6.
Solution, prosjektavhengigheter og dokumentasjonslenker følger de nye stiene.

## Splitting av Application Service

V0–V3 bruker fortsatt Core/ApplicationServices, Core/DomainServices og Core/DomainModel.
I V4/V6 er den tidligere rene BookingWorkflow flyttet til Core/ApplicationServices/BookingService.
Den er static, bruker bare eksplisitte data og løser GetQuote, Book og Cancel.
Den gamle effectful BookingService er flyttet til Api/ImperativeShell/BookingHandler.
BookingEventHandler og de fem portene er flyttet fra Core til Api/ImperativeShell og Ports.
Infrastructure implementerer portene; Program.cs kobler DI i API-et.

V4 og V6 har samme struktur. API-kildefilene er identiske etter normalisering av versjonsnamespace.
V4 bruker eksplisitte Result-sjekker; V6 bruker Map og FlatMap i Book/GetQuote.
Cancel er fortsatt enkel Result-kode. V6s GetQuote validerer e-post først, som V4.

## Laboratoriene

Pure Functions beholder små eksempler på pris, skjult tid/id/randomness, fil-I/O og mutasjon.
Effects.cs legger til fakta som data, planlagte effects og en enkel interpreter.
BookingCreated er et faktum. SaveBooking, ChargePayment og SendEmail er ønskede handlinger.
Plan gjør ingen I/O; interpreter simulerer I/O med Console. Ingen effect-plan brukes i Kåre V4/V6.
Kommentarer forklarer at records ikke automatisk er dypt immutable.

Composition-konsollen viser funksjoner lagret i variabler, sendt inn og returnert fra funksjoner,
higher-order functions og enkel Compose før en vellykket og en avvist booking.
Currying/partial application er tatt ut for å holde introduksjonen kort.
Result<T>.FlatMap (ofte kalt Bind) unngår nesting og hopper over senere success-steg ved Failure.
Map pakker en vanlig returverdi i Result. Begge viderefører feil uten å kalle funksjonen.

## Bevarte valg og avgrensninger

.NET 10 / SDK 10.0.401 og eksisterende NUnit-pakker er beholdt.
Ingen nye frameworks, infrastrukturpakker eller felles domene-library.
API-kontrakter, regler, lokale DateTime-verdier, immutable snapshots og fakes er videreført.
V6 beholder tidligere API-port 5105; prosjektets nye versjonsnummer krever ikke en ny port.
V0–V3 og 90 har bare minimale rettelser av versjonsreferanser i README; koden er urørt.
Prelude-variantene er helt urørt.

In-memory state, alltid vellykket fake betaling og synkron event-dispatch er fortsatt forenklinger.
Save/payment er ikke atomisk, og read-decide-write er ikke concurrency-safe.
Optional Aggregate Root beholder versjonerte snapshots og optimistic save.

## Verifikasjon

Kommandoer:
```powershell
dotnet build BackendLight.ArchitectureLab.sln --no-restore -m:1
dotnet test BackendLight.ArchitectureLab.sln --no-build --no-restore -m:1 --logger trx --results-directory TestResults
dotnet run --project "05 - Pure Functions/KareBooking.PureFunctions.Console" --no-build --no-restore
dotnet run --project "06 - Functional Composition/KareBooking.Composition.Console" --no-build --no-restore
```

Begge konsollene er kjørt. Prosjektreferanser og lokale Markdown-lenker er kontrollert.
Core i V4/V6 har ingen DomainServices, infrastructure-interfaces eller skjulte I/O-kall.

Verifisert 16. oktober 2026:
- Alle 26 prosjekter bygget med 0 feil og 0 advarsler.
- Alle 372 NUnit-tester bestod; 0 skipped.
- V0: 60, V1: 43, V2: 50, V3: 51, V4: 52, V6: 56, Pure Functions: 7, Aggregate: 53.
- Første restore trengte nettverk for NuGet-sårbarhetsdata; full build lyktes med nettverkstilgang.
- Testhost koblet ikke til i sandboxen; samme testsuite lyktes utenfor sandboxen.
- Begge konsolllabbene kjørte uten feil.
- Alle 26 solution-prosjekter, prosjektreferanser og lokale Markdown-lenker finnes.
- API-kildefilene i V4/V6 er identiske bortsett fra versjonsnamespace.
