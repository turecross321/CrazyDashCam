using CrazyDashCam.Shared.Database;
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

    protected virtual void CamOnRecordingActivity(object? sender, CameraRecorder recorder)
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
        if (!_cam.IsRecording())
            return;
        
        _cam.StopRecording();
    }

    protected void StartRecording()
    {
        if (_cam.IsRecording())
            return;
        
        _cam.StartRecording();
    }

    protected void AddHighlight()
    {
        if (!_cam.IsRecording())
            return;
        
        Logger.LogInformation("Adding highlight");
        
        _cam.AddTripData(new DbHighlight
        {
            Date = DateTimeOffset.Now
        });
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