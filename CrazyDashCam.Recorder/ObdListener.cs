using CrazyDashCam.Shared.Database;
using Microsoft.Extensions.Logging;
using OBD.NET.Communication;
using OBD.NET.Devices;
using OBD.NET.Events;
using OBD.NET.Logging;
using OBD.NET.OBDData;

namespace CrazyDashCam.Recorder;

public class ObdListener : IDisposable
{
    private readonly SerialConnection _connection;
    private readonly ELM327 _dev;
    private readonly ILogger _logger;
    private readonly DashCam _dashCam;

    public ObdListener(ILogger logger, DashCam dashCam, string obdPort)
    {
        _logger = logger;
        _dashCam = dashCam;
        
        if (string.IsNullOrEmpty(obdPort))
        {
            logger.LogInformation("OBD port hasn't been specified.");
            obdPort = GetObdPort();
        }
        
        _connection = new SerialConnection(obdPort);
        _dev = new ELM327(_connection, new OBDConsoleLogger(OBDLogLevel.Debug));

        _dev.SubscribeDataReceived<AmbientAirTemperature>(ObdEventHandler);
        _dev.SubscribeDataReceived<EngineCoolantTemperature>(ObdEventHandler);
        _dev.SubscribeDataReceived<FuelTankLevelInput>(ObdEventHandler);
        _dev.SubscribeDataReceived<EngineOilTemperature>(ObdEventHandler);
        _dev.SubscribeDataReceived<EngineRPM>(ObdEventHandler);
        _dev.SubscribeDataReceived<VehicleSpeed>(ObdEventHandler);
        _dev.SubscribeDataReceived<ThrottlePosition>(ObdEventHandler);

        _dev.Initialize();
    }

    private string GetObdPort()
    {
        _logger.LogInformation("Attempting to find OBD port");
        
        IEnumerable<string> availablePorts = SerialConnection.GetAvailablePorts();

        string result = "";
        
        foreach (string port in availablePorts)
        {
            try
            {
                SerialConnection con = new SerialConnection(port);
                con.Connect();
                result = port;
            }
            catch (Exception e)
            {
                continue;
            }
        }

        _logger.LogInformation("Found {result}", result);
        
        return result;
    }
    
    public void StartListening(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting OBD listener...");

        // todo: make this configurable
        _ = PeriodicRequest<AmbientAirTemperature>(TimeSpan.FromMinutes(5), cancellationToken);
        _ = PeriodicRequest<FuelTankLevelInput>(TimeSpan.FromMinutes(5), cancellationToken);
        _ = PeriodicRequest<EngineCoolantTemperature>(TimeSpan.FromSeconds(10), cancellationToken);
        _ = PeriodicRequest<EngineOilTemperature>(TimeSpan.FromSeconds(10), cancellationToken);
        _ = PeriodicRequest<EngineRPM>(TimeSpan.FromSeconds(2), cancellationToken);
        _ = PeriodicRequest<VehicleSpeed>(TimeSpan.FromSeconds(1), cancellationToken);
        _ = PeriodicRequest<ThrottlePosition>(TimeSpan.FromSeconds(1), cancellationToken);
    }

    private async Task PeriodicRequest<T>(TimeSpan interval, CancellationToken cancellationToken) where T : class, IOBDData, new()
    {
        await Task.Delay(interval, cancellationToken);
        
        while (cancellationToken.IsCancellationRequested == false)
        {
            _dev.RequestData<T>();
            await Task.Delay(interval, cancellationToken);
        }
    }
    
    private void ObdEventHandler<T>(object sender, DataReceivedEventArgs<T> args) where T : IOBDData
    {
        IHasTimestamp dbEvent = args.Data switch
        {
            AmbientAirTemperature ambientAirTemperature => new DbAmbientAirTemperature(args.Timestamp,
                ambientAirTemperature.Temperature),
            EngineCoolantTemperature engineCoolantTemperature => new DbCoolantTemperature(args.Timestamp,
                engineCoolantTemperature.Temperature),
            FuelTankLevelInput fuelTankLevelInput => new DbFuelLevel(args.Timestamp, fuelTankLevelInput.Level),
            EngineOilTemperature engineOilTemperature => new DbOilTemperature(args.Timestamp,
                engineOilTemperature.Temperature),
            EngineRPM engineRpm => new DbRpm(args.Timestamp, engineRpm.Rpm),
            VehicleSpeed speed => new DbSpeed(args.Timestamp, speed.Speed),
            ThrottlePosition throttlePosition => new DbThrottlePosition(args.Timestamp, throttlePosition.Position),
            _ => throw new ArgumentNullException(nameof(args))
        };

        _dashCam.AddTripData(dbEvent);
    }

    public void Dispose()
    {
        _dev.Dispose();
        _connection.Dispose();
    }
}