namespace CrazyDashCam.Shared.Database;

public class DbIntakeTemperature : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbIntakeTemperature()
    {
        
    }
    
    public DbIntakeTemperature(DateTime date, float value) : base(date, value)
    {
    }
}