namespace CrazyDashCam.Recorder.Configuration;

public class GpioPinsConfiguration
{
    public int WarningLedPin { get; set; } = 19;
    public int RunningLedPin { get; set; } = 26;
    public int ObdLedPin { get; set; } = 19;

    public int StartRecordingButtonPin { get; set; } = 13;
    public int StopRecordingButtonPin { get; set; } = 6;
    public int AddHighlightPin { get; set; } = 22;
}