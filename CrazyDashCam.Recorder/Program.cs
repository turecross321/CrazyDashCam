using System.Device.Gpio;
using CrazyDashCam.Recorder;
using CrazyDashCam.Recorder.Configuration;
using CrazyDashCam.Recorder.Controllers;
using Microsoft.Extensions.Logging;

bool verbose = args.Contains("-v") || args.Contains("--verbose");

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(verbose ? LogLevel.Trace : LogLevel.Information);
});
ILogger logger = loggerFactory.CreateLogger<Program>();

CancellationTokenSource programCancellationTokenSource = new();
Console.CancelKeyPress += (sender, e) =>
{
    // Prevent the application from terminating immediately on CTRL + C
    e.Cancel = true;
    programCancellationTokenSource.Cancel();
};

DashCamConfiguration config = DashCamConfiguration.LoadOrCreate(logger);
using DashCam cam = new DashCam(logger, config);

List<DashCamController> controllers = [];

if (config.UseCliController) controllers.Add(new CliDashCamController(logger, cam));
if (config.UseGpioController) controllers.Add(new GpioDashCamController(logger, cam, config));

foreach (var controller in controllers)
{
    logger.LogInformation("Using {type}", controller.GetType().Name);
}

try
{
    await Task.Delay(Timeout.Infinite, programCancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
    logger.LogInformation("Quitting...");

    foreach (var controller in controllers)
    {
        controller.Dispose();
    }
}