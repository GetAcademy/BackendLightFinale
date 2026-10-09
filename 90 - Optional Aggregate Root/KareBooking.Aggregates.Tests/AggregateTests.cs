using NUnit.Framework;

namespace KareBooking.Aggregates.Tests;


public sealed class AggregateTests
{
    private static readonly TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo");
    private static readonly BookCommand command = new("ada@example.test", DateTimeOffset.Parse("2030-01-14T12:00:00+01:00"), 2);
    private static readonly DateTimeOffset now = command.Start.AddDays(-14);

    [Test]
    public void Root_protects_the_booking_collection_and_releases_a_cancelled_slot()
    {
        var root = new KareSchedule(zone);
        var booked = root.Book(command, now, Guid.NewGuid());
        Assert.That(booked.IsSuccess, Is.True);
        Assert.That(root.Book(command, now, Guid.NewGuid()).IsFailure, Is.True);
        Assert.That(root.Bookings, Has.Count.EqualTo(1));
        Assert.That(root.Bookings, Is.Not.InstanceOf<List<Booking>>());
        Assert.That(root.Cancel(booked.Value.Id, now).Value.Amount, Is.EqualTo(800m));
        Assert.That(root.Cancel(booked.Value.Id, now).IsFailure, Is.True);
        Assert.That(root.Book(command, now, Guid.NewGuid()).IsSuccess, Is.True);
        Assert.That(booked.Value.IsCancelled, Is.False, "The caller receives a detached view.");
    }

    [Test]
    public async Task Aggregate_rules_alone_cannot_prevent_two_stale_snapshots_from_overwriting_each_other()
    {
        var repository = new InMemoryScheduleRepository(zone);
        var first = await repository.LoadAsync();
        var second = await repository.LoadAsync();
        Assert.That(first.Schedule.Book(command, now, Guid.NewGuid()).IsSuccess, Is.True);
        Assert.That(second.Schedule.Book(command, now, Guid.NewGuid()).IsSuccess, Is.True);
        Assert.That(await repository.TrySaveAsync(first.Schedule, first.Version), Is.True);
        Assert.That(await repository.TrySaveAsync(second.Schedule, second.Version), Is.False);
        var saved = await repository.LoadAsync();
        Assert.That(saved.Schedule.Bookings, Has.Count.EqualTo(1));
        Assert.That(saved.Version, Is.EqualTo(1));
        Assert.That(saved.Schedule.Book(command, now, Guid.NewGuid()).IsFailure, Is.True);
    }

    [Test]
    public void Duplicate_identity_is_rejected_even_for_another_slot()
    {
        var root = new KareSchedule(zone);
        var id = Guid.NewGuid();
        root.Book(command, now, id);
        Assert.That(root.Book(command with { Start = command.Start.AddDays(1) }, now, id).IsFailure, Is.True);
    }
}
