using CrazyDashCam.Recorder.Configuration;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder.Controllers;

public class CliDashCamController : DashCamController
{
    public CliDashCamController(ILogger logger, DashCam cam, CancellationTokenSource cancellationToken) : base(logger, cam)
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            // Prevent the application from terminating immediately on CTRL + C
            e.Cancel = true;
            cancellationToken.Cancel();
        };
        
        _ = MonitorKeyPressAsync(cancellationToken.Token);
    }
    
    
    private async Task MonitorKeyPressAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
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
            
            await Task.Delay(50, cancellationToken);
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
}