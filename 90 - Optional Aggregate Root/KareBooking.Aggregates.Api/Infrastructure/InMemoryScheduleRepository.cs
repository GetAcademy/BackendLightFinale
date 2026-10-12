namespace KareBooking.Aggregates;

public sealed class InMemoryScheduleRepository : IScheduleRepository
{
    private KareSchedule state = new();
    private long version;
    public Task<ScheduleSnapshot> LoadAsync()
    {
        return Task.FromResult(new ScheduleSnapshot(state.Copy(), version));
    }
    public Task<bool> TrySaveAsync(KareSchedule schedule, long expectedVersion)
    {
        if (version != expectedVersion) return Task.FromResult(false);
        state = schedule.Copy();
        version++;
        return Task.FromResult(true);
    }
}
