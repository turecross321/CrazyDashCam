using CrazyDashCam.Shared;
using CrazyDashCam.Shared.Database;

namespace CrazyDashCam.PlayerAPI.Models;

public record StoredTrip(string Path, string DirectoryName, TripMetadata MetaData, DbHighlight[] Highlights);