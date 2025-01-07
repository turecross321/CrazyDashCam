using System.Text.Json;

namespace CrazyDashCam;

public static class CrazyJsonSerializer
{
    public static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, obj.GetType(), JsonSerializerOptions.Web);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web);
    }
}