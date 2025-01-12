namespace CrazyDashCam.Shared;

public record TripMetadata
{
    public required DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public required string VehicleName { get; set; }
    public TripMetadataVideo[]? Videos { get; set; }
}