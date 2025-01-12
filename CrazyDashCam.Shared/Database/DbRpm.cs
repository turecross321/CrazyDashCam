namespace CrazyDashCam.Shared.Database;

public class DbRpm : DbValueWithTimestamp<int>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbRpm()
    {
        
    }
    
    public DbRpm(DateTimeOffset date, int value) : base(date, value)
    {
    }
}