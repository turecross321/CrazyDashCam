namespace CrazyDashCam;

public record TripMetadata
{
    public required DateTime StartTime { get; set; }
    public required string VehicleName { get; set; }
}