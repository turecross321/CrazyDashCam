namespace CrazyDashCam.Database;

public class DbAmbientAirTemperature(DateTime date, float value) : DbValueWithTimestamp<float>(date, value)
{
    
}