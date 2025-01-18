using System.Diagnostics;
using CrazyDashCam.Recorder.Configuration;
using CrazyDashCam.Shared;
using CrazyDashCam.Shared.Database;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OBD.NET.OBDData;

namespace CrazyDashCam.Recorder;

public class DashCam : IDisposable
{
    private BluetoothClient? _bluetoothClient;
    private TripDbContext? _tripDbContext;
    private readonly List<CameraRecorder> _recorders = [];
    private TripEventAggregator? _eventAggregator;
    private ObdListener? _obdListener;
    
    private TripMetadata? _tripMetadata;
    private string? _tripDirectory;
    private bool _recording = false;
    public bool IsRecording() => _recording;

    private readonly ILogger _logger;
    private readonly DashCamConfiguration _configuration;

    public event EventHandler<bool>? Warning; 
    public event EventHandler<bool>? ObdActivity; 
    public event EventHandler<RecordingEventArgs>? RecordingActivity; 

    public DashCam(ILogger logger, DashCamConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        foreach (CameraConfiguration device in configuration.Cameras)
        {
            CameraRecorder recorder = new(this, logger, device, configuration.VideoEncoder, configuration.AudioEncoder);
            _recorders.Add(recorder);
        }
    }

    private Task ConnectToRfCommBluetoothDevice(string deviceAddress)
    {
        _logger.LogInformation("Connecting to RfCommBluetoothDevice");
        
        _bluetoothClient = new BluetoothClient();
        
        BluetoothAddress address = BluetoothAddress.Parse(deviceAddress);
        BluetoothEndPoint endpoint = new BluetoothEndPoint(address, BluetoothService.SerialPort);
        
        _bluetoothClient.Connect(endpoint);
        
        return Task.CompletedTask;
    }
    
    private void HandleEvent(DateTimeOffset date, object eventData)
    {
        if (_tripDbContext == null)
        {
            throw new NullReferenceException("_tripDbContext is null");
        }
        
        switch (eventData)
        {
            case AmbientAirTemperature ambientAirTemperature:
                _tripDbContext.AmbientAirTemperatures.Add(new DbAmbientAirTemperature(date, ambientAirTemperature.Temperature));
                break;
            case EngineCoolantTemperature engineCoolantTemperature:
                _tripDbContext.CoolantTemperatures.Add(new DbCoolantTemperature(date, engineCoolantTemperature.Temperature));
                break;
            case FuelTankLevelInput fuelTankLevelInput:
                _tripDbContext.FuelLevels.Add(new DbFuelLevel(date, fuelTankLevelInput.Level));
                break;
            case EngineOilTemperature engineOilTemperature:
                _tripDbContext.OilTemperatures.Add(new DbOilTemperature(date, engineOilTemperature.Temperature));
                break;
            case EngineRPM engineRpm:
                _tripDbContext.Rpms.Add(new DbRpm(date, engineRpm.Rpm));
                break;
            case VehicleSpeed speed:
                _tripDbContext.Speeds.Add(new DbSpeed(date, speed.Speed));
                break;
            case ThrottlePosition throttlePosition:
                _tripDbContext.ThrottlePositions.Add(new DbThrottlePosition(date, throttlePosition.Position));
                break;
        }
    }

    private void SaveMetadata()
    {
        _logger.LogInformation("Saving metadata");

        Debug.Assert(_tripMetadata != null, nameof(_tripMetadata) + " != null");
        string metadataJson = CrazyJsonSerializer.Serialize(_tripMetadata);
        Debug.Assert(_tripDirectory != null, nameof(_tripDirectory) + " != null");
        File.WriteAllTextAsync(Path.Combine(_tripDirectory, "metadata.json"), metadataJson);
    }
    
    public async void StartRecording(CancellationToken cancellationToken)
    {
        if (IsRecording())
        {
            _logger.LogWarning("Attempted to start recording while one was already active");
            return;
        }
        
        _logger.LogInformation("Starting recording");
        _recording = true;
        
        DateTimeOffset start = DateTimeOffset.Now;
        string folderName = start.ToString("yyyy-MM-dd_HH-mm-ss");
        _tripDirectory = Path.Combine(_configuration.VideoPath, folderName);
        Directory.CreateDirectory(_tripDirectory);
        
        _tripDbContext = new TripDbContext(_tripDirectory);
        _tripDbContext.ApplyMigrations();
        
        foreach (var recorder in _recorders)
        {
            string fileName = $"{recorder.Camera.Label.ToValidFileName()}.mp4";
            recorder.StartRecording(_tripDirectory, fileName);
        }
        
        _tripMetadata = new TripMetadata
        {
            StartDate = start,
            VehicleName = _configuration.VehicleName,
        };
        
        SaveMetadata();
        
        _eventAggregator = new TripEventAggregator();
        _eventAggregator.Subscribe(HandleEvent);

        if (_configuration.UseObd)
        {
            if (_configuration.AutomaticallyConnectToObdBluetooth)
                await ConnectToRfCommBluetoothDevice(_configuration.Obd2BluetoothAddress);
            
            _obdListener = new ObdListener(_logger, _eventAggregator, _configuration.ObdSerialPort);
            _obdListener.StartListening(cancellationToken);
        }
        
        cancellationToken.Register(StopRecording);
        
        _logger.LogInformation("Started recording");
    }

    private async void StopRecording()
    {
        _logger.LogInformation("Stopping recording");
        _recording = false;
        
        DateTimeOffset end = DateTimeOffset.Now;
        Debug.Assert(_tripMetadata != null, nameof(_tripMetadata) + " != null");
        _tripMetadata.EndDate = end;
        List<TripMetadataVideo> metadataVideos = new List<TripMetadataVideo>();

        DateTimeOffset? lastStartDate = null;
        foreach (CameraRecorder recorder in _recorders)
        {
            if (lastStartDate == null || recorder.StartDate > lastStartDate)
                lastStartDate = recorder.StartDate;
            
            metadataVideos.Add(new TripMetadataVideo(recorder.Camera.Label, 
                recorder.FileName!, recorder.StartDate, recorder.Camera.MuteAutomaticallyOnPlayback));
            
            await recorder.StopRecording();
        }

        _tripMetadata.AllVideosStartedDate = lastStartDate;
        _tripMetadata.Videos = metadataVideos.ToArray();
        
        SaveMetadata();
        
        _obdListener?.Dispose();
        _eventAggregator?.Dispose();

        if (_tripDbContext != null)
        {
            await _tripDbContext.SaveChangesAsync();
            await _tripDbContext.Database.CloseConnectionAsync();
            await _tripDbContext.DisposeAsync();
        }
        
        _tripMetadata = null;
        _tripDirectory = null;
        
        _logger.LogInformation("Stopped recording");
    }

    /// <summary>
    /// Signal to listeners that something has gone wrong
    /// </summary>
    public void InvokeWarning()
    {
        Warning?.Invoke(this, true);
    }
    
    /// <summary>
    /// Signal to listeners about video recording updates
    /// </summary>
    public void InvokeRecordingActivity(RecordingEventArgs args)
    {
        RecordingActivity?.Invoke(this, args);
    }
    
    /// <summary>
    /// Signal to listeners about OBD updates
    /// </summary>
    public void InvokeObdActivity(bool value)
    {
        ObdActivity?.Invoke(this, value);
    }

    public void Dispose()
    {
        if (_recording)
            StopRecording();
        
        _bluetoothClient?.Dispose();
        _tripDbContext?.Dispose();
        _eventAggregator?.Dispose();
    }
}