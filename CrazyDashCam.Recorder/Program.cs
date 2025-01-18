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
DashCamGpioController controller = new(cam, config);

await Task.Delay(Timeout.Infinite);