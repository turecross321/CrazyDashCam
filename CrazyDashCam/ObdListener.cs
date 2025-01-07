using Microsoft.Extensions.Logging;
using OBD.NET.Communication;
using OBD.NET.Devices;
using OBD.NET.Events;
using OBD.NET.Logging;
using OBD.NET.OBDData;

namespace CrazyDashCam;

public class ObdListener : IDisposable
{
    private readonly SerialConnection _connection;
    private readonly ELM327 _dev;
    private readonly TripEventAggregator _eventAggregator;
    private readonly ILogger _logger;

    private Timer? _timer;
    private bool _isListening;

    public ObdListener(ILogger logger, TripEventAggregator eventAggregator, string obdPort)
    {
        _logger = logger;
        _eventAggregator = eventAggregator;

        if (string.IsNullOrEmpty(obdPort))
        {
            logger.LogInformation("OBD port hasn't been specified.");
            obdPort = GetObdPort();
        }

        _connection = new SerialConnection(obdPort);
        _dev = new ELM327(_connection, new OBDConsoleLogger(OBDLogLevel.Debug));

        _dev.SubscribeDataReceived<AmbientAirTemperature>(ObdEventHandler);
        _dev.SubscribeDataReceived<EngineCoolantTemperature>(ObdEventHandler);
        _dev.SubscribeDataReceived<CalculatedEngineLoad>(ObdEventHandler);
        _dev.SubscribeDataReceived<FuelTankLevelInput>(ObdEventHandler);
        _dev.SubscribeDataReceived<IntakeAirTemperature>(ObdEventHandler);
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

    public async Task StartListeningAsync()
    {
        if (_isListening)
            return;

        _logger.LogInformation("Starting OBD listener...");

        _isListening = true;

        Task[] tasks =
        {
            PeriodicRequest<AmbientAirTemperature>(TimeSpan.FromSeconds(10)),
            PeriodicRequest<EngineCoolantTemperature>(TimeSpan.FromSeconds(10)),
            PeriodicRequest<CalculatedEngineLoad>(TimeSpan.FromSeconds(5)),
            PeriodicRequest<FuelTankLevelInput>(TimeSpan.FromSeconds(60)),
            PeriodicRequest<IntakeAirTemperature>(TimeSpan.FromSeconds(10)),
            PeriodicRequest<EngineOilTemperature>(TimeSpan.FromSeconds(10)),
            PeriodicRequest<EngineRPM>(TimeSpan.FromSeconds(1)),
            PeriodicRequest<VehicleSpeed>(TimeSpan.FromSeconds(1)),
            PeriodicRequest<ThrottlePosition>(TimeSpan.FromSeconds(1))
        };
        
        await Task.WhenAll(tasks);
    }

    private async Task PeriodicRequest<T>(TimeSpan interval) where T : class, IOBDData, new()
    {
        while (_isListening)
        {
            await Task.Delay(interval); // Delay between requests for this specific data type
            // Request data
            _dev.RequestData<T>();
        }
    }
    
    private void ObdEventHandler<T>(object sender, DataReceivedEventArgs<T> args) where T : IOBDData
    {
        _eventAggregator.Publish(args.Timestamp, args.Data);
    }

    public void Dispose()
    {
        _isListening = false;
        _timer?.Dispose();
        _dev.Dispose();
        _connection.Dispose();
    }
}