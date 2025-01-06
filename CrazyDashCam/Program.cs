using CrazyDashCam;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});
ILogger logger = loggerFactory.CreateLogger<Program>();

Configuration config = Configuration.LoadOrCreate(logger);
DashCam cam = new DashCam(logger, config);

cam.StartRecording();

Thread.Sleep(10_000);

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    cam.StopRecording();
    // Add cleanup or other logic here
};