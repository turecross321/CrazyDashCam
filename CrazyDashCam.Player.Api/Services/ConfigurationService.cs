using CrazyDashCam.PlayerAPI.Models;

namespace CrazyDashCam.PlayerAPI.Services;

public class ConfigurationService(ILogger<ConfigurationService> logger)
{
    public readonly ServerConfiguration Config = ServerConfiguration.LoadOrCreate(logger);
}