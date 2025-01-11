using CrazyDashCam.Shared;

namespace CrazyDashCam.PlayerAPI.Models;

public record StoredTrip(string Path, string DirectoryName, TripMetadata MetaData);