using System.Device.Gpio;
using CrazyDashCam.Recorder.Configuration;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder.Controllers;

public class GpioDashCamController : DashCamController, IDisposable
{
    private readonly GpioController _gpioController;
    private readonly DashCamConfiguration _configuration;

    private readonly Dictionary<string, int> _cameraGpioNumbers = new Dictionary<string, int>();
    
    public GpioDashCamController(ILogger logger, DashCam cam, DashCamConfiguration configuration) : base(logger, cam)
    {
        _gpioController = new GpioController();
        _configuration = configuration;

        _gpioController.OpenPin(_configuration.GpioPins.WarningLedPin, PinMode.Output);
        _gpioController.OpenPin(_configuration.GpioPins.RunningLedPin, PinMode.Output);
        _gpioController.OpenPin(_configuration.GpioPins.ObdLedPin, PinMode.Output);
        foreach (CameraConfiguration camera in _configuration.Cameras)
        {
            if (!camera.GpioPin.HasValue)
                continue;
                
            _cameraGpioNumbers.Add(camera.Label, camera.GpioPin.Value);
            _gpioController.OpenPin(camera.GpioPin.Value, PinMode.Output);
        }

        WriteAllLeds(false);
        
        _gpioController.OpenPin(_configuration.GpioPins.StartRecordingButtonPin, PinMode.InputPullUp);
        _gpioController.RegisterCallbackForPinValueChangedEvent(_configuration.GpioPins.StartRecordingButtonPin, PinEventTypes.Falling, OnStartRecording);
        
        _gpioController.OpenPin(_configuration.GpioPins.StopRecordingButtonPin, PinMode.InputPullUp);
        _gpioController.RegisterCallbackForPinValueChangedEvent(_configuration.GpioPins.StopRecordingButtonPin, PinEventTypes.Falling, OnStopRecording);

        _gpioController.OpenPin(_configuration.GpioPins.AddHighlightPin, PinMode.InputPullUp);
        _gpioController.RegisterCallbackForPinValueChangedEvent(_configuration.GpioPins.StopRecordingButtonPin, PinEventTypes.Falling, OnAddHighlight);
        
        _gpioController.Write(_configuration.GpioPins.RunningLedPin, true);
    }
    
    private void WriteAllLeds(bool value)
    {
        Logger.LogInformation("Writing all LEDs {value}", value);
        
        _gpioController.Write(_configuration.GpioPins.WarningLedPin, value);
        _gpioController.Write(_configuration.GpioPins.RunningLedPin, value);
        _gpioController.Write(_configuration.GpioPins.ObdLedPin, value);

        foreach (int pin in _cameraGpioNumbers.Values)
        {
            _gpioController.Write(pin, value);
        }
    }


    protected override void CamOnRecordingActivity(object? sender, CameraRecorder recorder)
    {
        int gpioPin = _cameraGpioNumbers[recorder.CameraConfig.Label];
        _gpioController.Write(gpioPin, recorder.Recording);
    }

    protected override void CamOnObdActivity(object? sender, bool value)
    {
        _gpioController.Write(_configuration.GpioPins.ObdLedPin, value);
    }

    protected override void CamOnWarning(object? sender, bool value)
    {
        _gpioController.Write(_configuration.GpioPins.WarningLedPin, value);
    }

    private void OnStopRecording(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        StopRecording();
    }

    private void OnStartRecording(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        StartRecording();
    }

    private void OnAddHighlight(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        AddHighlight();
    }

    
    public override void Dispose()
    {
        Logger.LogInformation("Disposing " + nameof(GpioDashCamController));

        WriteAllLeds(false);
        
        foreach (KeyValuePair<string, int> camera in _cameraGpioNumbers)
        {
            _gpioController.Write(camera.Value, false);
        }
        
        _gpioController.Dispose();
        
        GC.SuppressFinalize(this);
    }
}