using CrazyDashCam.Recorder;
using CrazyDashCam.Recorder.Configuration;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});
ILogger logger = loggerFactory.CreateLogger<Program>();

DashCamConfiguration config = DashCamConfiguration.LoadOrCreate(logger);
DashCam cam = new DashCam(logger, config);
DashCamGpioController controller = new(logger, cam, config);

try
{
    await Task.Delay(Timeout.Infinite);
}
finally
{
    logger.LogInformation("Disposing shit");
    // Ensure that Dispose is called even if the program ends unexpectedly
    cam.Dispose();
    controller.Dispose();
}