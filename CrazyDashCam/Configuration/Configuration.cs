using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam;

public class Configuration
{
    public Camera[] Cameras { get; init; } = new Camera[] { new Camera("windshield", "/dev/video0", 30, "1500k") };
    public string VideoPath { get; init; } = Path.Combine(Environment.CurrentDirectory, "trips/");
    public string Obd2BluetoothAddress { get; init; } = "";
    public bool AutomaticallyConnectToObdBluetooth { get; init; } = false;
    public string ObdComPort { get; init; } = "";
    public bool UseObd { get; init; } = false;
    public string FileFormat { get; init; } = "mkv";
    public string VehicleName = "Car 2";

    public static string FilePath => Path.Combine(Directory.GetCurrentDirectory(), "config.json");

    public static Configuration LoadOrCreate(ILogger logger)
    {
        Configuration? configuration = LoadFromFile(logger, FilePath);

        if (configuration != null)
            return configuration;
        
        configuration = new Configuration();
        logger.LogInformation("Configuration file could not be loaded. Using default configuration.");
        SaveToFile(logger, FilePath, configuration);

        return configuration;
    }
    
    private static Configuration? LoadFromFile(ILogger logger, string filePath)
    {
        logger.LogInformation($"Attempting to load configuration from {filePath}.");
        
        if (!File.Exists(filePath))
        {
            logger.LogWarning("Configuration file could not be found.");
            return null;
        }

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Configuration>(json);
    }

    private static void SaveToFile(ILogger logger, string filePath, Configuration configuration)
    {
        logger.LogInformation($"Saving configuration to {filePath}");
        string json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}