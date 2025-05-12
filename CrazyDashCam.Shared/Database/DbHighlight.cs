using System.ComponentModel.DataAnnotations;

namespace CrazyDashCam.Shared.Database;

public class DbHighlight : IHasTimestamp
{
    [Key] public required DateTimeOffset Date { get; set; }
}