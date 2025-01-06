namespace CrazyDashCam.Database;

public class DbFuelLevel : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbFuelLevel()
    {
        
    }
    
    public DbFuelLevel(DateTime date, float value) : base(date, value)
    {
    }
}