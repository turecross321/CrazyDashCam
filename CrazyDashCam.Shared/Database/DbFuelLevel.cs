namespace CrazyDashCam.Shared.Database;

public class DbFuelLevel : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbFuelLevel()
    {
        
    }
    
    public DbFuelLevel(DateTimeOffset date, float value) : base(date, value)
    {
    }
}