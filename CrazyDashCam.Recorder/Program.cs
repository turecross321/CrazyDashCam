using CrazyDashCam;
using CrazyDashCam.Recorder;
using CrazyDashCam.Recorder.Configuration;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});
ILogger logger = loggerFactory.CreateLogger<Program>();

CancellationTokenSource cts = new CancellationTokenSource();

DashCamConfiguration config = DashCamConfiguration.LoadOrCreate(logger);
DashCam cam = new DashCam(logger, config);
cam.StartRecording(cts.Token);

Console.WriteLine("Press key to stop recording");
Console.ReadLine();

cts.Cancel();