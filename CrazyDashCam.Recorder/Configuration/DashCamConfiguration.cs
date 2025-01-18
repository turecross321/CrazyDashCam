using CrazyDashCam.Shared;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder.Configuration;

public class DashCamConfiguration
{
    public Camera[] Cameras { get; init; } = [new Camera()];
    public string VideoPath { get; init; } = Path.Combine(Environment.CurrentDirectory, "trips/");
    public string Obd2BluetoothAddress { get; init; } = "";
    public bool AutomaticallyConnectToObdBluetooth { get; init; } = false;
    public string ObdSerialPort { get; init; } = "";
    public bool UseObd { get; init; } = false;
    public string VehicleName { get; init; } =  "SKODA Octavia C 1.0TSI";
    public string VideoEncoder { get; init; } = "h264_v4l2m2m";
    public string AudioEncoder { get; init; } = "aac";

    private static string FilePath => Path.Combine(Directory.GetCurrentDirectory(), "config.json");

    public static DashCamConfiguration LoadOrCreate(ILogger logger)
    {
        DashCamConfiguration? configuration = LoadFromFile(logger, FilePath);

        if (configuration != null)
            return configuration;
        
        configuration = new DashCamConfiguration();
        logger.LogInformation("Configuration file could not be loaded. Using default configuration.");
        SaveToFile(logger, FilePath, configuration);

        return configuration;
    }
    
    private static DashCamConfiguration? LoadFromFile(ILogger logger, string filePath)
    {
        logger.LogInformation($"Attempting to load configuration from {filePath}.");
        
        if (!File.Exists(filePath))
        {
            logger.LogWarning("Configuration file could not be found.");
            return null;
        }

        string json = File.ReadAllText(filePath);
        return CrazyJsonSerializer.Deserialize<DashCamConfiguration>(json);
    }

    private static void SaveToFile(ILogger logger, string filePath, DashCamConfiguration dashCamConfiguration)
    {
        logger.LogInformation($"Saving configuration to {filePath}");
        string json = CrazyJsonSerializer.Serialize(dashCamConfiguration);
        File.WriteAllText(filePath, json);
    }
}