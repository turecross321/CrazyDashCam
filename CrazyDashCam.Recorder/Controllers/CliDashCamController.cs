﻿using System.ComponentModel.Design;
using System.Diagnostics;
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
                
                Console.WriteLine(keyInfo.KeyChar);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.R:
                        StartRecording();
                        break;
                    case ConsoleKey.T:
                        StopRecording();
                        break;
                    case ConsoleKey.Y:
                        PrintAmountOfFfmpegProccesses();
                        break;
                    case ConsoleKey.H:
                        AddHighlight();
                        break;
                }
            }
            
            await Task.Delay(50, token);
        }
    }

    private void PrintAmountOfFfmpegProccesses()
    {
        var ffmpegs = Process.GetProcesses().Where(p => p.ProcessName.Contains("ffmpeg"));
        Logger.LogInformation("{count} instances of ffmpeg are running", ffmpegs.Count());
    }

    protected override void CamOnRecordingActivity(object? sender, CameraRecorder recorder)
    {
        Console.WriteLine($"[RECORDING]: {recorder.CameraConfig.Label}={recorder.Recording}");
        
        base.CamOnRecordingActivity(sender, recorder);
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
        
        GC.SuppressFinalize(this);
    }
}