using CrazyDashCam.PlayerAPI.Models;
using CrazyDashCam.Shared;
using CrazyDashCam.Shared.Database;
using Microsoft.Extensions.Caching.Memory;

namespace CrazyDashCam.PlayerAPI.Services;

public class TripStorageService(ILogger<TripStorageService> logger, IMemoryCache cache)
{
    
    private const string TripsDirectory =
        @"C:\Users\MateyMatey\RiderProjects\CrazyDashCam\CrazyDashCam.Recorder\bin\Debug\net9.0\trips";

    private const string MemoryCacheKey = "CachedTrips";
    
    public async Task<StoredTrip?> GetTrip(string directoryName)
    {
        string tripPath = Path.Combine(TripsDirectory, directoryName);
        string metadataPath = Path.Combine(tripPath, "metadata.json");
            
        if (!File.Exists(metadataPath))
            return null;

        TripMetadata? metadata = CrazyJsonSerializer.Deserialize<TripMetadata>(await File.ReadAllTextAsync(metadataPath));
        if (metadata == null)
            return null;

        await using TripDbContext tripDb = new TripDbContext(tripPath);
        
        return new StoredTrip(tripPath, directoryName, metadata, tripDb.Highlights.ToArray());
    }
    
    public async Task<List<StoredTrip>> GetTrips()
    {
        if (cache.TryGetValue(MemoryCacheKey, out List<StoredTrip>? cachedTrips) && cachedTrips != null)
            return cachedTrips;
        
        logger.LogInformation("Getting trips");
        
        string[] tripDirectories = Directory.GetDirectories(TripsDirectory);
        List<StoredTrip> trips = [];
        
        foreach (string tripPath in tripDirectories)
        {
            string directoryName = Path.GetRelativePath(TripsDirectory, tripPath);
            StoredTrip? trip = await GetTrip(directoryName);
            if (trip == null)
                continue;
            
            trips.Add(trip);
        }

        cache.Set(MemoryCacheKey, trips, TimeSpan.FromSeconds(60));
        
        return trips;
    }
}