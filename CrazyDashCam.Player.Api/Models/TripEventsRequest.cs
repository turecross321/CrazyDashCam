namespace CrazyDashCam.PlayerAPI.Models;

public record TripEventsRequest
{
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
}