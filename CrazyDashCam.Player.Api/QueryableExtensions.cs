using CrazyDashCam.Shared.Database;

namespace CrazyDashCam.PlayerAPI;

public static class QueryableExtensions
{
    // The extension method that filters based on the Timestamp property of IHasTimestamp
    public static IEnumerable<T> FilterByTimestamp<T>(this IQueryable<T> source, DateTime start, DateTime end) 
        where T : IHasTimestamp
    {
        IQueryable<T> within = source.Where(i => i.Timestamp >= start && i.Timestamp <= end);
        T? first = source.Where(i => i.Timestamp < start).OrderBy(i => i.Timestamp).LastOrDefault();

        return first != null ? within.AsEnumerable().Prepend(first) : within;
    }
}