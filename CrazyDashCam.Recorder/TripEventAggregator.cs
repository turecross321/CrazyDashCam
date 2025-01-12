namespace CrazyDashCam.Recorder;

public class TripEventAggregator : IDisposable
{
    private readonly List<Action<DateTimeOffset, object>> _subscribers = new List<Action<DateTimeOffset, object>>();

    // Subscribe to events
    public void Subscribe(Action<DateTimeOffset, object> subscriber)
    {
        _subscribers.Add(subscriber);
    }

    // Publish events to all subscribers
    public void Publish(DateTimeOffset date, object eventData)
    {
        foreach (var subscriber in _subscribers)
        {
            subscriber(date, eventData);
        }
    }

    public void Dispose()
    {
        _subscribers.Clear();
    }
}