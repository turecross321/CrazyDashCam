namespace CrazyDashCam.Shared.Database;

public class DbIntakeTemperature : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbIntakeTemperature()
    {
        
    }
    
    public DbIntakeTemperature(DateTimeOffset date, float value) : base(date, value)
    {
    }
}