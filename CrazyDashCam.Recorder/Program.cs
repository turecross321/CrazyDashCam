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
    // Prevent the application from terminating immediately
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
    // Handle graceful shutdown if needed
    Console.WriteLine("Application shutdown initiated...");
}
finally
{
    // Ensure Dispose is called when application is shutting down
    controller.Dispose();
}