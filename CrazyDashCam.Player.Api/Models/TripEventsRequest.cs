namespace CrazyDashCam.PlayerAPI.Models;

public record TripEventsRequest
{
    public DateTime Current { get; set; }
    public DateTime To { get; set; }
}