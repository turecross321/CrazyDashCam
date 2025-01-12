namespace CrazyDashCam.Shared.Database;

public class DbAmbientAirTemperature : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbAmbientAirTemperature()
    {
        
    }
    
    public DbAmbientAirTemperature(DateTimeOffset date, float value) : base(date, value)
    {
    }
}