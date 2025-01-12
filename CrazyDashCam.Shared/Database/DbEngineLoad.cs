namespace CrazyDashCam.Shared.Database;

public class DbEngineLoad : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbEngineLoad()
    {
        
    }
    
    public DbEngineLoad(DateTimeOffset date, float value) : base(date, value)
    {
    }
}