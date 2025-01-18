namespace CrazyDashCam.Recorder.Configuration;

public record Camera
{
    public string Label { get; set; } = "Front";
    public string DeviceName { get; set; } = "/dev/video0";
    public int Fps { get; set; } = 30;
    public long VideoBitrate { get; set; } = 1_500_000;
    public int ResolutionWidth { get; set; } = 640;
    public int ResolutionHeight { get; set; } = 480;
    public bool RecordAudio { get; set; } = false;
    public bool MuteAutomaticallyOnPlayback { get; set; } = false;
    public long AudioBitrate { get; set; } = 128_000;
    public long AudioSampleRate { get; set; } = 44_100;
    public float AudioOffsetSeconds { get; set; } = 0.0f;
    public string? AudioDevice { get; set; } = "";
    public int Threads { get; set; } = 1;
}