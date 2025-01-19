using System.Device.Gpio;
using CrazyDashCam.Recorder.Configuration;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder;

public class DashCamGpioController : IDisposable
{
    private readonly GpioController _gpioController;
    private readonly DashCam _cam;
    private readonly ILogger _logger;
    private readonly DashCamConfiguration _configuration;
    private CancellationTokenSource _cancellationTokenSource;

    private Dictionary<string, int> _cameraGpioNumbers = new Dictionary<string, int>();
    
    public DashCamGpioController(ILogger logger, DashCam cam, DashCamConfiguration configuration)
    {
        _gpioController = new GpioController();
        _cam = cam;
        _logger = logger;
        _configuration = configuration;
        _cancellationTokenSource = new CancellationTokenSource();

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
        
        _cam.Warning += CamOnWarning;
        _cam.ObdActivity += CamOnObdActivity;
        _cam.RecordingActivity += CamOnRecordingActivity;
        
        _gpioController.Write(_configuration.GpioPins.RunningLedPin, true);
    }

    private void WriteAllLeds(bool value)
    {
        _logger.LogInformation("Writing all LEDs {value}", value);
        
        _gpioController.Write(_configuration.GpioPins.WarningLedPin, value);
        _gpioController.Write(_configuration.GpioPins.RunningLedPin, value);
        _gpioController.Write(_configuration.GpioPins.ObdLedPin, value);

        foreach (int pin in _cameraGpioNumbers.Values)
        {
            _gpioController.Write(pin, value);
        }
    }
    
    private void CamOnRecordingActivity(object? sender, RecordingEventArgs recordingEventArgs)
    {
        int gpioPin = _cameraGpioNumbers[recordingEventArgs.Label];
        _gpioController.Write(gpioPin, recordingEventArgs.Recording);
    }

    private void CamOnObdActivity(object? sender, bool value)
    {
        _gpioController.Write(_configuration.GpioPins.ObdLedPin, value);
    }

    private void CamOnWarning(object? sender, bool value)
    {
        _gpioController.Write(_configuration.GpioPins.WarningLedPin, value);
    }

    private void OnStopRecording(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        _cancellationTokenSource.Cancel();
    }

    private void OnStartRecording(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        if (_cam.IsRecording())
            return;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _cam.StartRecording(_cancellationTokenSource.Token);
    }
    
    public void Dispose()
    {
        _logger.LogInformation("Disposing " + nameof(DashCamGpioController));
        
        _cam.Warning -= CamOnWarning;
        _cam.ObdActivity -= CamOnObdActivity;
        _cam.RecordingActivity -= CamOnRecordingActivity;

        WriteAllLeds(false);
        
        foreach (KeyValuePair<string, int> camera in _cameraGpioNumbers)
        {
            _gpioController.Write(camera.Value, false);
        }
        
        _gpioController.Dispose();
    }
}