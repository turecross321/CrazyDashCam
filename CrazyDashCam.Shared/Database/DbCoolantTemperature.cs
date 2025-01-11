namespace CrazyDashCam.Shared.Database;

public class DbCoolantTemperature : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbCoolantTemperature()
    {
        
    }
    
    public DbCoolantTemperature(DateTime date, float value) : base(date, value)
    {
    }
}