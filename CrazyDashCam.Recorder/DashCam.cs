using System.Diagnostics;
using CrazyDashCam.Recorder.Configuration;
using CrazyDashCam.Shared;
using CrazyDashCam.Shared.Database;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder;

public class DashCam : IDisposable
{
    private BluetoothClient? _bluetoothClient;
    private TripDbContext? _tripDbContext;
    private readonly List<CameraRecorder> _recorders = [];
    private ObdListener? _obdListener;
    
    private TripMetadata? _tripMetadata;
    private string? _tripDirectory;
    private bool _recording = false;
    public bool IsRecording() => _recording;

    private readonly ILogger _logger;
    private readonly DashCamConfiguration _configuration;
    private CancellationTokenSource? _cancellationTokenSource;

    public event EventHandler<bool>? Warning; 
    public event EventHandler<bool>? ObdActivity; 
    public event EventHandler<CameraRecorder>? RecordingActivity; 

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
    
    public void AddTripData(IHasTimestamp eventData)
    {
        if (_tripDbContext == null)
        {
            throw new NullReferenceException("_tripDbContext is null");
        }
        
        switch (eventData)
        {
            case DbAmbientAirTemperature ambientAirTemperature:
                _tripDbContext.AmbientAirTemperatures.Add(ambientAirTemperature);
                break;
            case DbCoolantTemperature engineCoolantTemperature:
                _tripDbContext.CoolantTemperatures.Add(engineCoolantTemperature);
                break;
            case DbFuelLevel fuelTankLevelInput:
                _tripDbContext.FuelLevels.Add(fuelTankLevelInput);
                break;
            case DbOilTemperature engineOilTemperature:
                _tripDbContext.OilTemperatures.Add(engineOilTemperature);
                break;
            case DbRpm engineRpm:
                _tripDbContext.Rpms.Add(engineRpm);
                break;
            case DbSpeed speed:
                _tripDbContext.Speeds.Add(speed);
                break;
            case DbThrottlePosition throttlePosition:
                _tripDbContext.ThrottlePositions.Add(throttlePosition);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(eventData), "Tried to add invalid database entry");
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
    
    public async void StartRecording()
    {
        if (IsRecording())
        {
            _logger.LogWarning("Attempted to start recording while one was already active");
            return;
        }
        
        _logger.LogInformation("Starting recording");
        _recording = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        DateTimeOffset start = DateTimeOffset.Now;
        string folderName = start.ToString("yyyy-MM-dd_HH-mm-ss");
        _tripDirectory = Path.Combine(_configuration.VideoPath, folderName);
        Directory.CreateDirectory(_tripDirectory);
        
        _tripDbContext = new TripDbContext(_tripDirectory);
        _tripDbContext.ApplyMigrations();
        
        foreach (var recorder in _recorders)
        {
            string fileName = $"{recorder.Camera.Label.ToValidFileName()}.mp4";
            recorder.StartRecording(_tripDirectory, fileName, _cancellationTokenSource.Token);
        }
        
        _tripMetadata = new TripMetadata
        {
            StartDate = start,
            VehicleName = _configuration.VehicleName,
        };
        
        SaveMetadata();

        if (_configuration.UseObd)
        {
            try
            {
                if (_configuration.AutomaticallyConnectToObdBluetooth)
                    await ConnectToRfCommBluetoothDevice(_configuration.Obd2BluetoothAddress);
                
                _obdListener = new ObdListener(_logger, this, _configuration.ObdSerialPort);
                _obdListener.StartListening(_cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                _logger.LogError("OBD ERROR {message}", e.Message);
                InvokeWarning();
            }
        }
        
        _logger.LogInformation("Started recording");
    }

    public async void StopRecording()
    {
        if (!IsRecording())
        {
            _logger.LogWarning("Attempted to stop recording while one wasn't active");
            return;
        }
        
        _logger.LogInformation("Stopping recording");
        _recording = false;
        await _cancellationTokenSource!.CancelAsync();
        
        DateTimeOffset end = DateTimeOffset.Now;
        Debug.Assert(_tripMetadata != null, nameof(_tripMetadata) + " != null");
        _tripMetadata.EndDate = end;
        
        SaveMetadata();
        
        _obdListener?.Dispose();

        if (_tripDbContext != null)
        {
            await _tripDbContext.SaveChangesAsync();
            await _tripDbContext.Database.CloseConnectionAsync();
            await _tripDbContext.DisposeAsync();
        }
        else
        {
            _logger.LogError("_tripDbContext is null?");
        }
        
        _tripMetadata = null;
        _tripDirectory = null;
        
        _logger.LogInformation("Stopped recording");
    }

    private void AddRecorderToMetadata(CameraRecorder recorder)
    {
        _tripMetadata!.Videos.Add(new TripMetadataVideo(recorder.Camera.Label, 
            recorder.FileName!, recorder.StartDate, recorder.Camera.MuteAutomaticallyOnPlayback));

        if (_tripMetadata.Videos.Count == _recorders.Count) 
            _tripMetadata.AllVideosStartedDate = _tripMetadata.Videos.MaxBy(t => t.StartDate)?.StartDate;
        
        SaveMetadata();
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
    public void InvokeRecordingActivity(CameraRecorder recorder)
    {
        RecordingActivity?.Invoke(this, recorder);

        // If the recorder is recording and hasn't been added to the metadata, add it.
        if (recorder.Recording && 
            _tripMetadata!.Videos.FirstOrDefault(v => v.Label == recorder.Camera.Label) == null)
            AddRecorderToMetadata(recorder);
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
    }
}