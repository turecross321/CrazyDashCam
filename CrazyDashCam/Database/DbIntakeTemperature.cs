namespace CrazyDashCam.Database;

public class DbIntakeTemperature(DateTime date, float value) : DbValueWithTimestamp<float>(date, value)
{
    
}