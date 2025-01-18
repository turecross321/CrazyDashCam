namespace CrazyDashCam.Recorder.Configuration;

public class GpioPinsConfiguration
{
    public int WarningLedPin { get; set; } = 11;
    public int RunningLedPin { get; set; } = 13;
    public int ObdLedPin { get; set; } = 15;

    public int StartRecordingButtonPin { get; set; } = 16;
    public int StopRecordingButtonPin { get; set; } = 18;
}