using System.Text.Json;
using CrazyDashCam.Configuration;
using CrazyDashCam.Database;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Microsoft.Extensions.Logging;
using OBD.NET.OBDData;

namespace CrazyDashCam;

public class DashCam : IDisposable
{
    private BluetoothClient? _bluetoothClient;
    private TripDbContext? _tripDbContext;
    private readonly List<CameraRecorder> _recorders = [];
    private TripEventAggregator? _eventAggregator;
    private ObdListener? _obdListener;
    private bool _recording = false;

    private readonly ILogger _logger;
    private readonly DashCamConfiguration _configuration;

    public DashCam(ILogger logger, DashCamConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        foreach (Camera device in configuration.Cameras)
        {
            CameraRecorder recorder = new CameraRecorder(logger, device);
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
    
    private void HandleEvent(DateTime date, object eventData)
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
            case CalculatedEngineLoad calculatedEngineLoad:
                _tripDbContext.EngineLoads.Add(new DbEngineLoad(date, calculatedEngineLoad.Load));
                break;
            case FuelTankLevelInput fuelTankLevelInput:
                _tripDbContext.FuelLevels.Add(new DbFuelLevel(date, fuelTankLevelInput.Level));
                break;
            case IntakeAirTemperature intakeAirTemperature:
                _tripDbContext.IntakeTemperatures.Add(new DbIntakeTemperature(date, intakeAirTemperature.Temperature));
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
    
    public async void StartRecording(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting recording");
        
        DateTime start = DateTime.Now;
        string folderName = start.ToString("yyyy-MM-dd_HH-mm-ss");
        string folderPath = Path.Combine(_configuration.VideoPath, folderName);
        Directory.CreateDirectory(folderPath);
        
        _tripDbContext = new TripDbContext(folderPath);
        _tripDbContext.ApplyMigrations();

        TripMetadata metadata = new TripMetadata
        {
            StartTime = start,
            VehicleName = _configuration.VehicleName
        };
        string metadataJson = CrazyJsonSerializer.Serialize(metadata);
        await File.WriteAllTextAsync(Path.Combine(folderPath, "metadata.json"), metadataJson, cancellationToken);
        
        foreach (var recorder in _recorders)
        {
            recorder.StartRecording(cancellationToken, folderPath, $"{recorder.Camera.Label}.{_configuration.FileFormat}");
        }
        
        _eventAggregator = new TripEventAggregator();
        _eventAggregator.Subscribe(HandleEvent);

        if (_configuration.UseObd)
        {
            if (_configuration.AutomaticallyConnectToObdBluetooth)
                await ConnectToRfCommBluetoothDevice(_configuration.Obd2BluetoothAddress);
            
            _obdListener = new ObdListener(_logger, _eventAggregator, _configuration.ObdSerialPort);
            _obdListener.StartListening(cancellationToken);
        }

        _recording = true;
        _logger.LogInformation("Started recording");
        
        cancellationToken.Register(StopRecording);
    }

    private void StopRecording()
    {
        _logger.LogInformation("Stopping recording");
        
        _obdListener?.Dispose();
        _eventAggregator?.Dispose();
        
        _tripDbContext?.SaveChanges();
        _tripDbContext?.Dispose();
        
        _recording = false;
        _logger.LogInformation("Stopped recording");
    }

    public void Dispose()
    {
        _bluetoothClient?.Dispose();
        _tripDbContext?.Dispose();
        _eventAggregator?.Dispose();
    }
}