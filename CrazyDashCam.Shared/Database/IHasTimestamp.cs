namespace CrazyDashCam.Shared.Database;

public interface IHasTimestamp
{
    public DateTimeOffset Date { get; set; }
}