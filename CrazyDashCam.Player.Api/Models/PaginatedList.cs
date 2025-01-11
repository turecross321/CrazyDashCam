namespace CrazyDashCam.PlayerAPI.Models;

public record PaginatedList<T>
{
    public required IEnumerable<T> Items { get; set; }
    public required int Count { get; set; }
}