using System.Text.Json.Serialization;

namespace CrazyDashCam.Recorder.Controllers;

public enum DashCamControllerType
{
    [JsonStringEnumMemberName("cli")]
    Cli,
    [JsonStringEnumMemberName("gpio")]
    Gpio
}