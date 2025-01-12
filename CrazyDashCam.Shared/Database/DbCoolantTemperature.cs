namespace CrazyDashCam.Shared.Database;

public class DbCoolantTemperature : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbCoolantTemperature()
    {
        
    }
    
    public DbCoolantTemperature(DateTimeOffset date, float value) : base(date, value)
    {
    }
}