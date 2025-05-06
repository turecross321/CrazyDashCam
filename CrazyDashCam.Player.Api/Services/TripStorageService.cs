using CrazyDashCam.PlayerAPI.Models;
using CrazyDashCam.Shared;
using Microsoft.Extensions.Caching.Memory;

namespace CrazyDashCam.PlayerAPI.Services;

public class TripStorageService(ILogger<TripStorageService> logger, ConfigurationService configService, IMemoryCache cache)
{
    
    private readonly string _tripsDirectory = configService.Config.TripsPath;

    private const string MemoryCacheKey = "IndexedTrips";
    
    public async Task<StoredTrip?> GetTrip(string directoryName)
    {
        string tripPath = Path.Combine(_tripsDirectory, directoryName);
        string metadataPath = Path.Combine(tripPath, "metadata.json");
            
        if (!File.Exists(metadataPath))
            return null;

        TripMetadata? metadata = CrazyJsonSerializer.Deserialize<TripMetadata>(await File.ReadAllTextAsync(metadataPath));
        return metadata == null ? null : new StoredTrip(tripPath, directoryName, metadata);
    }
    
    public async Task<List<StoredTrip>> GetTrips()
    {
        if (cache.TryGetValue(MemoryCacheKey, out List<StoredTrip>? cachedTrips) && cachedTrips != null)
            return cachedTrips;
        
        logger.LogInformation("Getting trips");
        
        string[] tripDirectories = Directory.GetDirectories(_tripsDirectory);
        List<StoredTrip> trips = [];
        
        foreach (string tripPath in tripDirectories)
        {
            string directoryName = Path.GetRelativePath(_tripsDirectory, tripPath);
            StoredTrip? trip = await GetTrip(directoryName);
            if (trip == null)
                continue;
            
            trips.Add(trip);
        }

        cache.Set(MemoryCacheKey, trips, TimeSpan.FromSeconds(60));
        
        return trips;
    }
}