namespace CrazyDashCam.Database;

public class DbOilTemperature : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbOilTemperature()
    {
        
    }
    
    public DbOilTemperature(DateTime date, float value) : base(date, value)
    {
    }
}