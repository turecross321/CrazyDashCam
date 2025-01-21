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

DashCamController controller = config.ControllerType switch
{
    DashCamControllerType.Cli => new CliDashCamController(logger, cam),
    DashCamControllerType.Gpio => new GpioDashCamController(logger, cam, config),
    _ => throw new ArgumentOutOfRangeException()
};

logger.LogInformation("Running CrazyDashCam using {type}", controller.GetType().Name);

try
{
    await Task.Delay(Timeout.Infinite, programCancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
    logger.LogInformation("Quitting...");
    controller.Dispose();
}