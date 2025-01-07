using System.ComponentModel.DataAnnotations;

namespace CrazyDashCam.Database;

public class DbLocation
{
    [Key] public DateTime Timestamp { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}