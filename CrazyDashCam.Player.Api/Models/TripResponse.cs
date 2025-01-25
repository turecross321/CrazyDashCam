using CrazyDashCam.Shared.Database;

namespace CrazyDashCam.PlayerAPI.Models;

public record TripResponse
{
    public required string DirectoryName { get; set; }
    public required DateTimeOffset StartDate { get; set; }
    public required DateTimeOffset? EndDate { get; set; }
    public required string VehicleName { get; set; }
    public required DateTimeOffset? AllVideosStartedDate { get; set; }
    public required IEnumerable<TripVideoResponse>? Videos { get; set; }
    public required IEnumerable<DbHighlight> Highlights { get; set; }

    public static TripResponse FromStoredTrip(StoredTrip item)
    {
        return new TripResponse
        {
            DirectoryName = item.DirectoryName,
            StartDate = item.MetaData.StartDate,
            EndDate = item.MetaData.EndDate,
            VehicleName = item.MetaData.VehicleName,
            AllVideosStartedDate = item.MetaData.AllVideosStartedDate,
            Videos = item.MetaData.Videos.Select(TripVideoResponse.FromTripMetadataVideo),
            Highlights = item.Highlights
        };
    }
}