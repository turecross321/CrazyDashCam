using System.ComponentModel.DataAnnotations;

namespace CrazyDashCam.Shared.Database;

public class DbValueWithTimestamp<T>
{
    /// <summary>
    /// For database migration
    /// </summary>
    protected DbValueWithTimestamp()
    {
        
    }

    protected DbValueWithTimestamp(DateTime date, T value)
    {
        Timestamp = date;
        Value = value;
    }

    [Key] public DateTime Timestamp { get; set; }
    public T Value { get; set; }
}