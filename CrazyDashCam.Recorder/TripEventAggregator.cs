namespace CrazyDashCam.Recorder;

public class TripEventAggregator : IDisposable
{
    private readonly List<Action<DateTime, object>> _subscribers = new List<Action<DateTime, object>>();

    // Subscribe to events
    public void Subscribe(Action<DateTime, object> subscriber)
    {
        _subscribers.Add(subscriber);
    }

    // Publish events to all subscribers
    public void Publish(DateTime date, object eventData)
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