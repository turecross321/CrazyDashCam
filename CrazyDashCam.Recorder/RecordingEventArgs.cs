namespace CrazyDashCam.Recorder;

public class RecordingEventArgs(string label, bool recording)
{
    public string Label { get; set; } = label;
    public bool Recording { get; set; } = recording;
}