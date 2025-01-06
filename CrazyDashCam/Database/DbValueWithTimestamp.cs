using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CrazyDashCam.Database;

public class DbValueWithTimestamp<T>(DateTime date, T value)
{
    [Key] public DateTime Timestamp { get; set; } = date;
    public T Value { get; set; } = value;
}