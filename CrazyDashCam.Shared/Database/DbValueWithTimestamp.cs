using System.ComponentModel.DataAnnotations;

namespace CrazyDashCam.Shared.Database;

public class DbValueWithTimestamp<T> : IHasTimestamp
{
    /// <summary>
    /// For database migration
    /// </summary>
    protected DbValueWithTimestamp()
    {
        
    }

    protected DbValueWithTimestamp(DateTimeOffset date, T value)
    {
        Date = date;
        Value = value;
    }

    [Key] public DateTimeOffset Date { get; set; }
    public T Value { get; set; }
}