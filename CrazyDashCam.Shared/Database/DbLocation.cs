using System.ComponentModel.DataAnnotations;

namespace CrazyDashCam.Shared.Database;

public class DbLocation : IHasTimestamp
{
    [Key] public DateTimeOffset Date { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}