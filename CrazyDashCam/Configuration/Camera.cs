namespace CrazyDashCam.Configuration;

public record Camera(string Label, string DeviceName, int Fps, long VideoBitrate, int ResolutionWidth, int ResolutionHeight);