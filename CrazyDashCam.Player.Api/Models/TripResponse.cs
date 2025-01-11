using System.Security.Cryptography;

namespace CrazyDashCam.PlayerAPI.Models;

public record TripResponse
{
    public required string DirectoryName { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime? EndDate { get; set; }
    public required string VehicleName { get; set; }
    public required IEnumerable<TripVideoResponse> Videos { get; set; }

    public static TripResponse FromStoredTrip(StoredTrip item)
    {
        return new TripResponse
        {
            DirectoryName = item.DirectoryName,
            StartDate = item.MetaData.StartDate,
            EndDate = item.MetaData.EndDate,
            VehicleName = item.MetaData.VehicleName,
            Videos = item.MetaData.Videos.Select(TripVideoResponse.FromTripMetadataVideo)
        };
    }
}