namespace CrazyDashCam;

public record TripMetadata
{
    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public required string VehicleName { get; set; }
    public required TripMetadataVideo[] Videos { get; set; }
}