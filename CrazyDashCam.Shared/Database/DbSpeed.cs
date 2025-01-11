namespace CrazyDashCam.Shared.Database;

public class DbSpeed : DbValueWithTimestamp<int>
{
    /// <summary>
    /// For database migration
    /// </summary>
    public DbSpeed()
    {
        
    }
    
    public DbSpeed(DateTime date, int value) : base(date, value)
    {
    }
}