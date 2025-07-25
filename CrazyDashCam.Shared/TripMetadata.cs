using CrazyDashCam.Shared.Database;

namespace CrazyDashCam.Shared;

public record TripMetadata
{
    public required DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public required string VehicleName { get; set; }
    public DateTimeOffset? AllVideosStartedDate { get; set; }
    public required List<TripMetadataVideo>? Videos { get; set; }
    // Nullable to support old recordings
    public List<TripHighlight>? Highlights { get; set; }
}