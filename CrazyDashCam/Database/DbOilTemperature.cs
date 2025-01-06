namespace CrazyDashCam.Database;

public class DbOilTemperature(DateTime date, float value) : DbValueWithTimestamp<float>(date, value)
{
    
}