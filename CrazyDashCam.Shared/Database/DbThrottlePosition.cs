namespace CrazyDashCam.Shared.Database;

public class DbThrottlePosition : DbValueWithTimestamp<float>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbThrottlePosition()
    {
        
    }
    
    public DbThrottlePosition(DateTime date, float value) : base(date, value)
    {
    }
}