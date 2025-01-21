using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder.Controllers;

public class CliDashCamController : DashCamController, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public CliDashCamController(ILogger logger, DashCam cam) : base(logger, cam)
    {
        _ = MonitorKeyPressAsync(_cancellationTokenSource.Token);
    }
    
    private async Task MonitorKeyPressAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.R:
                        StartRecording();
                        break;
                    case ConsoleKey.T:
                        StopRecording();
                        break;
                }
            }
            
            await Task.Delay(50, token);
        }
    }

    protected override void CamOnRecordingActivity(object? sender, RecordingEventArgs recordingEventArgs)
    {
        Console.WriteLine($"[RECORDING]: {recordingEventArgs.Label}={recordingEventArgs.Recording}");
        
        base.CamOnRecordingActivity(sender, recordingEventArgs);
    }

    protected override void CamOnWarning(object? sender, bool value)
    {
        Console.WriteLine($"[WARNING]: {value}");
        
        base.CamOnWarning(sender, value);
    }

    protected override void CamOnObdActivity(object? sender, bool value)
    {
        Console.WriteLine($"[OBD]: {value}");
        
        base.CamOnObdActivity(sender, value);
    }

    public override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        
        base.Dispose();
    }
}