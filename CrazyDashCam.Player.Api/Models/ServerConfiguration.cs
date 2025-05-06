using CrazyDashCam.Shared;

namespace CrazyDashCam.PlayerAPI.Models;

public class ServerConfiguration
{
    public string TripsPath { get; init; } = "";
    private static string FilePath => Path.Combine(Directory.GetCurrentDirectory(), "config.json");

    public static ServerConfiguration LoadOrCreate(ILogger logger)
    {
        ServerConfiguration? configuration = LoadFromFile(logger, FilePath);

        if (configuration != null)
            return configuration;
        
        configuration = new ServerConfiguration();
        logger.LogInformation("Configuration file could not be loaded. Using default configuration.");
        SaveToFile(logger, FilePath, configuration);

        return configuration;
    }
    
    private static ServerConfiguration? LoadFromFile(ILogger logger, string filePath)
    {
        logger.LogInformation($"Loading configuration from {filePath}.");
        
        if (!File.Exists(filePath))
        {
            logger.LogWarning("Configuration file could not be found.");
            return null;
        }

        string json = File.ReadAllText(filePath);
        return CrazyJsonSerializer.Deserialize<ServerConfiguration>(json);
    }

    private static void SaveToFile(ILogger logger, string filePath, ServerConfiguration dashCamConfiguration)
    {
        logger.LogInformation($"Saving configuration to {filePath}");
        string json = CrazyJsonSerializer.Serialize(dashCamConfiguration);
        File.WriteAllText(filePath, json);
    }
}