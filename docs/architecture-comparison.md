# Arkitekturideene svarer på forskjellige spørsmål

| Idé | Spørsmålet den svarer på | Eksempel i Kåre-caset | Kostnad |
| --- | --- | --- | --- |
| Transaction Script | Hvordan organiserer vi en use case? | BookingService deler regler via en privat hjelpemetode og orkestrerer I/O | Tjenesten eier både forretningsbeslutninger og I/O |
| Rich Domain Model / DDD | Hvordan modellerer vi virksomheten og beskytter business rules? | Booking.Cancel, TimeSlot og KareSchedule | Flere konsepter og grenser å lære |
| Kursets Onion Architecture | Hvor peker dependencies? | Api → Core; Core kjenner ikke Api | To prosjekter: Api og Core |
| Functional Core / Imperative Shell | Hvor plasserer vi side effects og I/O? | Shell leser/henter tid; BookingService tar vanlig data | Flere eksplisitte input/output og snapshots |

Disse er ikke nødvendigvis konkurrerende kategorier. En applikasjon kan samtidig ha:

```text
Onion-lignende dependency direction
+ rik domenemodell med verdiobjekter og invariants
+ Functional Core / Imperative Shell
```

V4/V6 illustrerer kombinasjonen: API-et avhenger av Core. Core uttrykker virksomheten med
TimeSlot, Money, Booking og beslutninger. Shell håndterer I/O, og den rene grensen tar data
inn og gir data ut. Dette krever verken mange ringer eller en full Clean Architecture-mal.

V0 har allerede Core og innoverpekende prosjektavhengighet, men typene i DomainModel er relativt anemiske.
Intern gjenbruk er løst med Extract Method i `BookingService`. V1 flytter eierskapet til reglene:
`KareSchedule` kjenner bookingregler, tilgjengelighet og pris; `Booking` kjenner overlap,
egen tilstand og cancellation/refund. Tjenesten leser, henter nåtid/id, kaller domenet og utfører I/O.

V1 har en I/O-fri DomainModel. Likevel bruker den mutable
entities og objekter for å organisere kunnskapen. V4 legger en eksplisitt, deterministisk
workflow-grense og immutable snapshots til dette. Vi unngår å fremstille V1 som dårlig bare
for å gjøre V4 attraktiv.

V0–V3 har Core/ApplicationServices, Core/DomainServices og Core/DomainModel.
BookingService bruker interfacene i DomainServices, og Api/Infrastructure implementerer dem.
I V3 beskytter Dependency Inversion Core mot konkrete adapters.
I V4/V6 trenger den rene BookingService ingen infrastructure-abstraksjoner i det hele tatt.
Den orkestrerer fortsatt domenet, men tar now, id og eksisterende state som data.
Api/ImperativeShell/BookingHandler overtar I/O, og Ports flyttes til Api/ImperativeShell/Ports.
BookingEventHandler ligger også i shell. Program.cs kobler fortsatt DI.
V0–V3s Core er Onion sitt indre prosjekt; V4/V6s Core er også faktisk Functional Core.

En rik modell trenger heller ikke mutable objekter i alle deler. Verdiobjekter og immutable
entities kan beskytte forretningsregler. En aggregate root kan modelleres funksjonelt, men
den valgfrie sidegrenen bruker vanlige objekter for å gjøre rootens ansvar lett å vise.

Velg etter problemet: få enkle use cases kan passe godt som Transaction Script; mange delte
regler gjør en rik modell attraktiv; mange rene beregninger gjør en funksjonell grense nyttig.
Ingen av valgene fjerner behovet for transaksjoner og samtidighetskontroll rundt ekte lagring.
V0–V4 og V6s in-memory teaching examples gjør ikke hele read-decide-write-flyten concurrency-safe.
Bruk transaksjoner og optimistic concurrency fra tidligere Backend Light-økter ved ekte lagring.
