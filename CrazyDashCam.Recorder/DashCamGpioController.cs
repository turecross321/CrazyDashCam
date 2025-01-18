using System.Device.Gpio;
using CrazyDashCam.Recorder.Configuration;

namespace CrazyDashCam.Recorder;

public class DashCamGpioController : IDisposable
{
    private readonly GpioController _gpioController;
    private readonly DashCam _cam;
    private readonly DashCamConfiguration _configuration;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private Dictionary<string, int> _cameraGpioNumbers = new Dictionary<string, int>();
    
    public DashCamGpioController(DashCam cam, DashCamConfiguration configuration)
    {
        _gpioController = new GpioController();
        _cam = cam;
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

        _gpioController.OpenPin(_configuration.GpioPins.StartRecordingButtonPin, PinMode.Input);
        _gpioController.RegisterCallbackForPinValueChangedEvent(_configuration.GpioPins.StartRecordingButtonPin, PinEventTypes.Rising, OnStartRecording);
        
        _gpioController.OpenPin(_configuration.GpioPins.StopRecordingButtonPin, PinMode.Input);
        _gpioController.RegisterCallbackForPinValueChangedEvent(_configuration.GpioPins.StartRecordingButtonPin, PinEventTypes.Rising, OnStopRecording);
        
        _cam.Warning += CamOnWarning;
        _cam.ObdActivity += CamOnObdActivity;
        _cam.RecordingActivity += CamOnRecordingActivity;
        
        _gpioController.Write(_configuration.GpioPins.RunningLedPin, true);
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
        _cam.StartRecording(_cancellationTokenSource.Token);
    }
    
    public void Dispose()
    {
        _cam.Warning -= CamOnWarning;
        _cam.ObdActivity -= CamOnObdActivity;
        _cam.RecordingActivity -= CamOnRecordingActivity;
        
        _gpioController.Dispose();
    }
}