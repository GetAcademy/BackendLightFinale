namespace KareBooking.Aggregates;

public sealed class InMemoryScheduleRepository(TimeZoneInfo businessZone) : IScheduleRepository
{
    private KareSchedule state = new(businessZone);
    private long version;
    private readonly object gate = new();
    public Task<ScheduleSnapshot> LoadAsync()
    {
        lock (gate) return Task.FromResult(new ScheduleSnapshot(state.Copy(), version));
    }
    public Task<bool> TrySaveAsync(KareSchedule schedule, long expectedVersion)
    {
        lock (gate)
        {
            if (version != expectedVersion) return Task.FromResult(false);
            state = schedule.Copy();
            version++;
            return Task.FromResult(true);
        }
    }
}
