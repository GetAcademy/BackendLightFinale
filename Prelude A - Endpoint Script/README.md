# Prelude A – Endpoint Script

All use-case orchestration and business rules live directly in the Minimal API endpoints.
Booking is a simple data model; Infrastructure contains concrete in-memory and console services.

The endpoints use Dependency Injection of concrete classes, without Dependency Inversion.
There is one API project, no Application Service and no Onion Core boundary.
This represents a realistic simple API before separating application flow and domain knowledge.

Compare A → B to move business knowledge into the model, or A → V0 to move orchestration
into an Application Service and introduce interfaces and a Core boundary.
The Prelude variants are comparisons outside the numbered journey and have no test projects.

Run from the repository root:

~~~powershell
dotnet run --project "Prelude A - Endpoint Script/KareBooking.PreludeA.Api"
~~~

Use the included KareBooking.PreludeA.http on port 5098 for quote, booking and cancellation.
All DateTime values are local business time. State is in memory; payment and email are fake.
