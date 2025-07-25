using Microsoft.EntityFrameworkCore;

namespace CrazyDashCam.Shared.Database;

public sealed class TripDbContext : DbContext
{
    public DbSet<DbAmbientAirTemperature> AmbientAirTemperatures { get; set; }
    public DbSet<DbCoolantTemperature> CoolantTemperatures { get; set; }
    public DbSet<DbFuelLevel> FuelLevels { get; set; }
    public DbSet<DbLocation> Locations { get; set; }
    public DbSet<DbOilTemperature> OilTemperatures { get; set; }
    public DbSet<DbRpm> Rpms { get; set; }
    public DbSet<DbSpeed> Speeds { get; set; }
    public DbSet<DbThrottlePosition> ThrottlePositions { get; set; }
    
    private string DbPath { get; }

    public TripDbContext(string folder)
    {
        DbPath = Path.Join(folder, "trip.db");
        
        this.Database.Migrate();
    }
    
    [Obsolete("Only meant for migrations")]
    public TripDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "trip.db");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
