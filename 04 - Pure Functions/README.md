# Pure Functions – et kort sidesprang

## Hva er likt?

Prising og bookingdata er hentet fra samme case, men dette er et selvstendig konsollspor,
ikke en ny API-arkitektur. De andre versjonene trenger ikke referere dette prosjektet.

## Hva endret vi?

`ImpureExamples` og `PureExamples` står ved siden av hverandre i `Examples.cs`.
Skjult klokke, Random.Shared og Guid.NewGuid blir vanlige input. Fil-lesing flyttes bort fra
beregningen. Mutasjon av class sammenlignes med record + `with`, som gir en ny verdi.
`Program.cs` demonstrerer eksemplene; all Console-utskrift ligger i programmet.

## Hvilket problem prøver dette å løse?

Samme input gir samme output uten observerbare sideeffekter.
En funksjon som leser en fil midt i prisingen, har skjult input og kan feile på I/O.
En innsendt Guid er vanlig data; en ny Guid inne i funksjonen gjør den ikke deterministisk.
PureExamples er små illustrasjoner, ikke en komplett erstatning for bookingreglene.

## Hva koster løsningen?

Pluss: forutsigbare beregninger, svært korte tester.
Minus: tid og genererte verdier må hentes av en caller. Applikasjonen trenger fremdeles I/O.
Record alene gjør ikke en funksjon pure; records kan også inneholde mutable lister.

## Kode å vise

Kjør konsollen fra rot-README-en. Åpne `KareBooking.PureFunctions.Console/Examples.cs`.
Diskuter hvorfor `Price(2)` er enklere å teste enn `QuoteFromFile(path)`.

Se [rot-README](../README.md) for kjøring, API-port, `.http`-requests og de eksakte forretningsgrensene.
Testene kjører med NUnit og kontrollerte input. API-ene bruker bare in-memory state og fake eksterne tjenester.
