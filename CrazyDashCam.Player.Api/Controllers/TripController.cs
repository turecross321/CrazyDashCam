using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CrazyDashCam.PlayerAPI.Models;
using CrazyDashCam.PlayerAPI.Services;
using CrazyDashCam.Shared;
using CrazyDashCam.Shared.Database;
using Microsoft.AspNetCore.Mvc;

namespace CrazyDashCam.PlayerAPI.Controllers;

public class TripController(TripStorageService tripStorage) : ControllerBase
{
    
    [HttpGet("/trips")]
    public async Task<IActionResult> GetTrips([FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        List<StoredTrip> trips = await tripStorage.GetTrips();
        return Ok(new PaginatedList<TripResponse>
        {
            Items = trips.Skip(skip).Take(take).Select(TripResponse.FromStoredTrip),
            Count = trips.Count
        });
    }

    [HttpGet("/trips/{directoryName}")]
    public async Task<IActionResult> GetTripInformation([FromRoute] string directoryName)
    {
        StoredTrip? trip = await tripStorage.GetTrip(directoryName);
        if (trip == null)
            return NotFound();

        return Ok(TripResponse.FromStoredTrip(trip));
    }

    [HttpGet("/trips/{directoryName}/videos/{videoLabel}/stream")]
    public async Task<IActionResult> StreamTripVideo([FromRoute] string directoryName, [FromRoute] string videoLabel)
    {
        StoredTrip? trip = await tripStorage.GetTrip(directoryName);
        if (trip == null)
            return NotFound();
        
        TripMetadataVideo? video = trip.MetaData.Videos.FirstOrDefault(v => v.Label == videoLabel);
        if (video == null)
            return NotFound();
        
        string path = Path.Join(trip.Path, video.FileName);
        
        if (!System.IO.File.Exists(path))
            return NotFound();
        
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        return File(stream, "video/mp4", enableRangeProcessing: true);
    }

    [HttpGet("/trips/{directoryName}/videos/{videoLabel}")]
    public async Task<IActionResult> DownloadTripVideo([FromRoute] string directoryName, [FromRoute] string videoLabel)
    {
        StoredTrip? trip = await tripStorage.GetTrip(directoryName);
        if (trip == null)
            return NotFound();
        
        TripMetadataVideo? video = trip.MetaData.Videos.FirstOrDefault(v => v.Label == videoLabel);
        if (video == null)
            return NotFound();
        
        string path = Path.Join(trip.Path, video.FileName);
        
        if (!System.IO.File.Exists(path))
            return NotFound();
        
        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(path);
        return File(fileBytes, "application/octet-stream", video.FileName);
    }
    
    [HttpGet("/trips/{directoryName}/events")]
    public async Task GetTripEvents([FromRoute] string directoryName)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
        
        StoredTrip? trip = await tripStorage.GetTrip(directoryName);
        if (trip == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
        
        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await using TripDbContext tripDb = new TripDbContext(trip.Path);
        tripDb.ApplyMigrations();
        
        byte[] buffer = new byte[1024 * 4];
        
        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                break;
            }

            if (result.MessageType != WebSocketMessageType.Text) 
                continue;
            
            string receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
            string json = JsonSerializer.Deserialize<string>(receivedText)!; // todo: fix this shitty bodge
            TripEventsRequest? clientRequest = CrazyJsonSerializer.Deserialize<TripEventsRequest>(json);
            
            if (clientRequest == null)
                continue;

            TripEventsResponse response = new TripEventsResponse
            {
                AmbAirTemp = tripDb.AmbientAirTemperatures.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                CoolTemp = tripDb.CoolantTemperatures.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                CalcLoad = tripDb.CalculatedEngineLoads.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                AbsLoad = tripDb.AbsoluteLoads.FilterByTimestamp(clientRequest.From, clientRequest.To),
                FuelLvl = tripDb.FuelLevels.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                InTemp = tripDb.IntakeTemperatures.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                Loc = tripDb.Locations.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                OilTemp = tripDb.OilTemperatures.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                Rpm = tripDb.Rpms.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                Speed = tripDb.Speeds.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                ThrPos = tripDb.ThrottlePositions.FilterByTimestamp(clientRequest.From,
                    clientRequest.To),
                From = clientRequest.From,
                To = clientRequest.To,
            };
            
            // todo: do *some* security measure to make sure this isnt completely abused

            // Send events as JSON
            var payload = CrazyJsonSerializer.SerializeCompact(response);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            await webSocket.SendAsync(new ArraySegment<byte>(payloadBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}