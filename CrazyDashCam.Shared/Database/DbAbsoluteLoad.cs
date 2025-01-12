namespace CrazyDashCam.Shared.Database;

public class DbAbsoluteLoad : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbAbsoluteLoad()
    {
        
    }
    
    public DbAbsoluteLoad(DateTimeOffset date, float value) : base(date, value)
    {
    }
}