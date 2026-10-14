# Prelude B – Rich Domain without Application Service

The Minimal API endpoints still orchestrate the use cases: load, call the model, save,
charge or refund, email, audit and return HTTP responses.

Business knowledge now lives in DomainModel: KareSchedule owns validation, availability
and pricing; Booking owns overlap, cancellation and refunds; BookingCalendar calculates the deadline.
The model follows V1 closely. Across the variants, cancelled bookings do not block availability
and adjacent intervals do not overlap.

Concrete infrastructure classes are injected directly. There is one API project, no interfaces,
no Application Service and no Onion Core boundary: Dependency Injection, without Dependency Inversion.
Rich Domain Modeling is useful independently of Onion Architecture.

Compare B → V1: does this small application need an Application Service and Onion Architecture?
Not necessarily. These are separate design choices.
The Prelude variants are comparisons outside the numbered journey and have no test projects.

Run from the repository root:

~~~powershell
dotnet run --project "Prelude B - Rich Domain without Application Service/KareBooking.PreludeB.Api"
~~~

Use the included KareBooking.PreludeB.http on port 5099. All DateTime values are local business time.
State is in memory; payment and email are fake.
