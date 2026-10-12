# Forretningsfakta og tekniske handlinger som data

`BookingCreated` betyr at en forretningsmessig beslutning om booking er tatt.
`BookingCancelled` beskriver avbestilling og den beregnede refusjonen. I den rene kjernen
produseres disse som del av beslutningen; shell må lykkes med lagring før fakta publiseres
til omverdenen. En returnert beslutning er ikke bevis på at databasen allerede er oppdatert.

`SendEmail` og `WriteAuditLog` beskriver hva en teknisk interpreter skal gjøre. De er effects,
ikke domain events. Samme domeneevent kan utløse ulike effects i ulike applikasjoner.

## Variant A – implementert i V3, V4 og V5

```text
DomainModel: input → Result(state + domain events)
Shell: save state → fake payment/refund → dispatch events
Handler: BookingCreated → email + audit
         BookingCancelled → email + audit
```

DomainModel returnerer altså fakta som data. Handleren tolker dem og utfører handlingene direkte.
V4/V5 endrer ikke innsendt state og trenger alle nødvendige input (inklusive now og id).
Dette er valgt fordi det lar domenet uttrykke virksomheten uten å bestemme e-posttekst,
audit-format eller adapter. Handleren er liten og eksplisitt, uten MediatR eller event bus.

## Variant B – bare en liten designillustrasjon

En høyere applikasjons-workflow kunne også vært en ren funksjon som oversetter fakta til
eksplisitte effect-data. Disse typene hører til applikasjonen, ikke bookingdomenet:

```csharp
abstract record Effect;
record SendEmail(string Recipient, string Message) : Effect;
record WriteAuditLog(string Message) : Effect;

static IReadOnlyList<Effect> Plan(BookingCreated fact) =>
[
    new SendEmail(fact.CustomerEmail, $"Booking {fact.BookingId} confirmed"),
    new WriteAuditLog($"BookingCreated {fact.BookingId}")
];
```

Shell kan tolke hvert effect med switch og sine adapters. Dette gjør også planen testbar uten
å sende noe, men gir ekstra typer og en interpreter for et lite eksempel. Derfor er variant A
implementert; B illustrerer hvordan actions as data kan tas ett steg videre uten å kalle
SendEmail et domain event. Ingen komplett alternativ arkitektur er nødvendig for poenget.

## Konsistens og forenklinger

Eksemplet lagrer state før fake charge/refund. FakePaymentGateway lykkes alltid og flytter ingen
ekte penger. Derfor kan vi følge save → payment → email → audit uten å late som at vi har
løst distribuert atomisitet. I et ekte system kan payment feile etter save, eller email feile
etter charge. Et lagret cancelled-flagg er heller ikke garanti for at refusjonen har skjedd.

Da må systemet modellere betalingsstatus og retries/idempotency eller kompensasjon, og lagre
events/effect-plan i en durable outbox i samme transaksjon som state hvis leveringen skal være
robust. Dette er diskusjonspunkter, ikke skjult implementerte egenskaper. Kjøring av domain
events gjør ikke viktig synkron betalingskonsistens automatisk trygg.

V0–V5 fokuserer på domenemodellering og arkitektur og gjør ikke hele read-decide-write-flyten
concurrency-safe. Repository returnerer kopier (eller immutable snapshots), så en domenemutasjon
ikke endrer lagret state før SaveAsync. Ved ekte lagring brukes transaksjoner og optimistic
concurrency fra tidligere Backend Light-økter. Et quote er bare et øyeblikksbilde og reserverer ingen tid.

Aggregate-siden bruker version på hele rootsnapshotet og TrySaveAsync med sjekk av expectedVersion. Konflikt
returneres før eksterne calls. Den har samme forenklede betalings-/leveringsmodell etter save.
