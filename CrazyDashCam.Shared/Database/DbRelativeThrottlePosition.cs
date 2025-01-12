namespace CrazyDashCam.Shared.Database;

public class DbRelativeThrottlePosition : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbRelativeThrottlePosition()
    {
        
    }
    
    public DbRelativeThrottlePosition(DateTimeOffset date, float value) : base(date, value)
    {
    }
}