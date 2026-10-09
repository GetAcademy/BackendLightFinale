# Videre: CQRS og Event Sourcing

## CQRS

I Kåre-caset er `BookKare` og `CancelBooking` commands: de forsøker å endre state.
`GetAvailableTimes` og `GetBookingOverview` er queries: de henter informasjon uten endring.
Det eksisterende `GetQuote` er også en query, mens Book/Cancel er commands.

```text
BookKare / CancelBooking → domeneregler → endret bookingstate
GetAvailableTimes       → ledige tidsrom, uten hele Booking-objekter
GetBookingOverview      → kunde, tidspunkt, pris og status i en enkel oversikt
```

En command-modell kan være en KareSchedule som beskytter overlapp. Read-modellen kan være
en flat liste med ledige timer eller en SQL-projection som passer skjermen.
Modellene trenger ikke være identiske selv om de bruker samme database og samme prosess.
CQRS betyr ikke automatisk microservices, to databaser eller message bus.
Dette repositoryet bygger ingen separat CQRS-stack.

## Event Sourcing

Tenk at historikken for en booking var:

```text
BookingCreated     { id: 42, start: mandag 12, hours: 2, price: 800 }
BookingRescheduled { id: 42, newStart: tirsdag 13 }
BookingCancelled   { id: 42, refund: 700 }
```

Hva hvis vi lagret hendelsene som historikken og bygget state fra dem?
En replay ville først opprette bookingstate, så flytte tiden, og til slutt markere den kansellert.
Historikken kan forklare *hvordan* nåværende state oppstod. BookingRescheduled er kun et
konseptuelt eksempel; rescheduling er ikke en implementert use case i denne laben.

Domain Events alene er ikke Event Sourcing. V3–V5 lagrer nåværende bookingstate; hendelser
brukes til reaksjoner og er ikke den autoritative, varige historikken.
Event Sourcing ville krevd blant annet event store, versjonering av events, concurrency,
replay og bevisste regler for om sideeffekter skal kjøres på nytt. Vi bygger ikke dette her.

Diskusjon: Når er historikk så viktig at denne kompleksiteten er verdt det? Hva skjer med
en gammel BookingCreated når prisreglene endres? Skal replay sende bekreftelsesmail igjen?
