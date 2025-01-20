using System.Device.Gpio;
using CrazyDashCam.Recorder;
using CrazyDashCam.Recorder.Configuration;
using CrazyDashCam.Recorder.Controllers;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});
ILogger logger = loggerFactory.CreateLogger<Program>();
CancellationTokenSource programCancellationTokenSource = new();

DashCamConfiguration config = DashCamConfiguration.LoadOrCreate(logger);
using DashCam cam = new DashCam(logger, config);

DashCamController controller = config.ControllerType switch
{
    DashCamControllerType.Cli => new CliDashCamController(logger, cam, programCancellationTokenSource),
    DashCamControllerType.Gpio => new GpioDashCamController(logger, cam, config),
    _ => throw new ArgumentOutOfRangeException()
};

try
{
    await Task.Delay(Timeout.Infinite);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Application shutdown initiated...");
    controller.Dispose();
}