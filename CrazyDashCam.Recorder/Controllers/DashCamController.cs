using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder.Controllers;

public abstract class DashCamController : IDisposable
{
    protected readonly ILogger Logger;
    private readonly DashCam _cam;
    private CancellationTokenSource _cancellationTokenSource;

    protected DashCamController(ILogger logger, DashCam cam)
    {
        Logger = logger;
        _cam = cam;
        _cancellationTokenSource = new CancellationTokenSource();
        
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
        _cancellationTokenSource.Cancel();
    }

    protected void StartRecording()
    {
        if (_cam.IsRecording())
            return;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _cam.StartRecording(_cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _cam.Warning -= CamOnWarning;
        _cam.ObdActivity -= CamOnObdActivity;
        _cam.RecordingActivity -= CamOnRecordingActivity;
        
        _cam.Dispose();
        _cancellationTokenSource.Dispose();
    }
}