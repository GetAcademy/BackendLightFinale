# Arkitekturideene svarer på forskjellige spørsmål

| Idé | Spørsmålet den svarer på | Eksempel i Kåre-caset | Kostnad |
| --- | --- | --- | --- |
| Transaction Script | Hvordan organiserer vi en use case? | BookingService deler regler via en privat hjelpemetode og orkestrerer I/O | Tjenesten eier både forretningsbeslutninger og I/O |
| Rich Domain Model / DDD | Hvordan modellerer vi virksomheten og beskytter business rules? | Booking.Cancel, TimeSlot og KareSchedule | Flere konsepter og grenser å lære |
| Onion/Clean-lignende retning | Hvor peker dependencies? | Api → Core; Core kjenner ikke Api | Flere prosjekter/grensesnitt når de trengs |
| Functional Core / Imperative Shell | Hvor plasserer vi side effects og I/O? | Shell leser/henter tid; BookingWorkflow tar vanlig data | Flere eksplisitte input/output og snapshots |

Disse er ikke nødvendigvis konkurrerende kategorier. En applikasjon kan samtidig ha:

```text
Onion-lignende dependency direction
+ rik domenemodell med verdiobjekter og invariants
+ Functional Core / Imperative Shell
```

V4/V5 illustrerer kombinasjonen: API-et avhenger av Core. Core uttrykker virksomheten med
TimeSlot, Money, Booking og beslutninger. Shell håndterer I/O, og den rene grensen tar data
inn og gir data ut. Dette krever verken mange ringer eller en full Clean Architecture-mal.

V0 har allerede Core og innoverpekende prosjektavhengighet, men Core-typene er relativt anemiske.
Intern gjenbruk er løst med Extract Method i `BookingService`. V1 flytter eierskapet til reglene:
`KareSchedule` kjenner bookingregler, tilgjengelighet og pris; `Booking` kjenner overlap,
egen tilstand og cancellation/refund. Tjenesten leser, henter nåtid/id, kaller domenet og utfører I/O.

V1 har I/O-fri Core. Likevel bruker den mutable
entities og objekter for å organisere kunnskapen. V4 legger en eksplisitt, deterministisk
workflow-grense og immutable snapshots til dette. Vi unngår å fremstille V1 som dårlig bare
for å gjøre V4 attraktiv.

Dependency inversion kan plassere I/O-interfaces i Core i andre systemer. Her ligger de i API-et,
fordi domenet ikke bruker dem. Core trenger ikke en repository-interface bare fordi prosjektet
heter Core. Functional Core er strengere om runtime-grensen: innsendt klokkeverdi er data,
mens et IClock-kall fra Core fremdeles er I/O selv om interfacet peker «riktig» vei.

En rik modell trenger heller ikke mutable objekter i alle deler. Verdiobjekter og immutable
entities kan beskytte forretningsregler. En aggregate root kan modelleres funksjonelt, men
den valgfrie sidegrenen bruker vanlige objekter for å gjøre rootens ansvar lett å vise.

Velg etter problemet: få enkle use cases kan passe godt som Transaction Script; mange delte
regler gjør en rik modell attraktiv; mange rene beregninger gjør en funksjonell grense nyttig.
Ingen av valgene fjerner behovet for transaksjoner og samtidighetskontroll rundt ekte lagring.
V0–V5s in-memory teaching examples gjør ikke hele read-decide-write-flyten concurrency-safe.
Bruk transaksjoner og optimistic concurrency fra tidligere Backend Light-økter ved ekte lagring.
