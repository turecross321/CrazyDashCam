using CrazyDashCam.Shared.Database;

namespace CrazyDashCam.PlayerAPI;

public static class QueryableExtensions
{
    // The extension method that filters based on the Timestamp property of IHasTimestamp
    public static IEnumerable<T> FilterByTimestamp<T>(this IQueryable<T> source, DateTimeOffset start, DateTimeOffset end) 
        where T : IHasTimestamp
    {
        // todo: why did i not need to make this an ienumerable when i used DateTimes?
        IEnumerable<T> within = source.AsEnumerable().Where(i => i.Date >= start && i.Date <= end);
        T? first = source.AsEnumerable().Where(i => i.Date < start).OrderBy(i => i.Date).LastOrDefault();

        return first != null ? within.AsEnumerable().Prepend(first) : within;
    }
}