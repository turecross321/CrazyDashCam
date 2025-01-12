using CrazyDashCam.Shared.Database;

namespace CrazyDashCam.PlayerAPI.Models;

public record TripEventsResponse
{
    public required IEnumerable<DbAmbientAirTemperature> AmbAirTemp { get; set; }
    public required IEnumerable<DbCoolantTemperature> CoolTemp { get; set; }
    public required IEnumerable<DbEngineLoad> EngLoad { get; set; }
    public required IEnumerable<DbFuelLevel> FuelLvl { get; set; }
    public required IEnumerable<DbIntakeTemperature> InTemp { get; set; }
    public required IEnumerable<DbLocation> Loc { get; set; }
    public required IEnumerable<DbOilTemperature> OilTemp { get; set; }
    public required IEnumerable<DbRpm> Rpm { get; set; }
    public required IEnumerable<DbSpeed> Speed { get; set; }
    public required IEnumerable<DbThrottlePosition> ThrPos { get; set; }
    public required DateTimeOffset From { get; set; }
    public required DateTimeOffset To { get; set; }
}