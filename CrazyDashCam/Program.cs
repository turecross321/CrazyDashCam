using CrazyDashCam;
using CrazyDashCam.Configuration;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});
ILogger logger = loggerFactory.CreateLogger<Program>();

DashCamConfiguration config = DashCamConfiguration.LoadOrCreate(logger);
DashCam cam = new DashCam(logger, config);

cam.StartRecording();

Thread.Sleep(30_000);

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    cam.StopRecording();
    // Add cleanup or other logic here
};