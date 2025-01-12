namespace CrazyDashCam.Shared.Database;

public class DbSpeed : DbValueWithTimestamp<int>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbSpeed()
    {
        
    }
    
    public DbSpeed(DateTimeOffset date, int value) : base(date, value)
    {
    }
}