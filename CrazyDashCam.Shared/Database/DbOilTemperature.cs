namespace CrazyDashCam.Shared.Database;

public class DbOilTemperature : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbOilTemperature()
    {
        
    }
    
    public DbOilTemperature(DateTimeOffset date, float value) : base(date, value)
    {
    }
}