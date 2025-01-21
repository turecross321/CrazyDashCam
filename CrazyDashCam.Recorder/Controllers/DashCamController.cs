using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder.Controllers;

public abstract class DashCamController : IDisposable
{
    protected readonly ILogger Logger;
    private readonly DashCam _cam;

    protected DashCamController(ILogger logger, DashCam cam)
    {
        Logger = logger;
        _cam = cam;
        
        _cam.Warning += CamOnWarning;
        _cam.ObdActivity += CamOnObdActivity;
        _cam.RecordingActivity += CamOnRecordingActivity;
    }

    protected virtual void CamOnRecordingActivity(object? sender, RecordingEventArgs recordingEventArgs)
    {

    }

    protected virtual void CamOnObdActivity(object? sender, bool value)
    {
        
    }

    protected virtual void CamOnWarning(object? sender, bool value)
    {
        
    }

    protected void StopRecording()
    {
        _cam.StopRecording();
    }

    protected void StartRecording()
    {
        if (_cam.IsRecording())
            return;
        
        _cam.StartRecording();
    }

    public virtual void Dispose()
    {
        _cam.Warning -= CamOnWarning;
        _cam.ObdActivity -= CamOnObdActivity;
        _cam.RecordingActivity -= CamOnRecordingActivity;
        
        _cam.Dispose();
        
        GC.SuppressFinalize(this);
    }
}