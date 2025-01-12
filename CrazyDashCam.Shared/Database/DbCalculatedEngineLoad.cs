namespace CrazyDashCam.Shared.Database;

public class DbCalculatedEngineLoad : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbCalculatedEngineLoad()
    {
        
    }
    
    public DbCalculatedEngineLoad(DateTimeOffset date, float value) : base(date, value)
    {
    }
}