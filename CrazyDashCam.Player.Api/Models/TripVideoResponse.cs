using CrazyDashCam.Shared;

namespace CrazyDashCam.PlayerAPI.Models;

public record TripVideoResponse
{
    public required string Label { get; set; }
    public required DateTime StartDate { get; set; }

    public static TripVideoResponse FromTripMetadataVideo(TripMetadataVideo video)
    {
        return new TripVideoResponse
        {
            Label = video.Label,
            StartDate = video.StartDate,
        };
    }
}