namespace CrazyDashCam.Shared;

public record TripMetadataVideo(string Label, string VideoFileName, string ThumbnailFileName, DateTimeOffset? StartDate, bool MuteAutomaticallyOnPlayback);