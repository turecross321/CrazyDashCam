using CrazyDashCam.Recorder;
using CrazyDashCam.Recorder.Configuration;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});
ILogger logger = loggerFactory.CreateLogger<Program>();

DashCamConfiguration config = DashCamConfiguration.LoadOrCreate(logger);
using DashCam cam = new DashCam(logger, config);
using DashCamGpioController controller = new(logger, cam, config);

CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (sender, e) =>
{
    // Prevent the application from terminating immediately on CTRL + C
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Application shutdown initiated...");
}