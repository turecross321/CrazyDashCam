using System.Diagnostics;
using System.Runtime.InteropServices;
using CrazyDashCam.Recorder.Configuration;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder;

public class CameraRecorder : IDisposable
{
    public CameraConfiguration Camera { get; }
    private Process? _process = null;
    private readonly ILogger _logger;
    private readonly DashCam _dashCam;

    public CameraRecorder(DashCam dashCam, ILogger logger, CameraConfiguration camera, string videoEncoder, string audioEncoder)
    {
        _logger = logger;
        Camera = camera;
        _dashCam = dashCam;
        VideoEncoder = videoEncoder;
        AudioEncoder = audioEncoder;
    }

    private string VideoEncoder { get; }
    private string AudioEncoder { get; }
    public DateTimeOffset? StartDate { get; private set; }
    public string? FileName { get; private set; }

    private string GetVideoFormat()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "v4l2";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "dshow";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "avfoundation";
        
        throw new Exception("Unsupported OS");
    }
    
    private string GetAudioFormat()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "alsa"; // Most common for Linux
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "dshow";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "avfoundation";

        throw new Exception("Unsupported OS");
    }

    private string GetFfmpegArguments(CameraConfiguration camera, string videoEncoder, string audioEncoder, string output)
    {
        string arguments = "";
        string videoFormat = GetVideoFormat();

        arguments += $" -framerate {camera.Fps}" +
                     $" -f {videoFormat}" +
                     $" -rtbufsize {camera.BufferSizeMb}M";

        if (videoFormat == "dshow")
        {
            arguments += $" -i video=\"{camera.DeviceName}\"";
        }
        else
            arguments += $" -i {camera.DeviceName}";

        if (camera.RecordAudio)
        {
            string audioFormat = GetAudioFormat();
            
            // if the video uses dshow, we combine the audio to the video input with a colon
            if (videoFormat == "dshow" && audioFormat == videoFormat)
                arguments += $":audio=\"{camera.AudioDevice}\"";
            else
                arguments += $" -f {GetAudioFormat()}" +
                         $" -i {camera.AudioDevice}";
        }

        arguments +=
            $" -c:v {videoEncoder}" +
            $" -fps_mode vfr" +
            $" -video_size {camera.ResolutionWidth}x{camera.ResolutionHeight}" +
            $" -preset veryfast" +
            $" -b:v {camera.VideoBitrateKbps}k" +
            $" -g 10";

        if (camera.RecordAudio)
            arguments += $" -c:a aac" +
                         $" -b:a {camera.AudioBitrateKbps}k";

        arguments +=
            $" -movflags +faststart" +
            $" -vf format=yuv420p" +
            $" -async 1" + // Ensure audio and video are in sync
            $" -threads {camera.Threads}" +
            $" -y" +
            $" \"{output}\"";

        return arguments;
    }
    public void StartRecording(CancellationToken cancellationToken, string directory, string fileName)
    {
        string output = Path.Combine(directory, fileName);
        _logger.LogInformation("Starting recording for {device} at {output}", Camera, output);
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = GetFfmpegArguments(Camera, VideoEncoder, AudioEncoder, output),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _process = Process.Start(startInfo);
        
        if (_process == null)
        {
            throw new InvalidOperationException($"Failed to start recording with {Camera}. Is FFmpeg installed and added to PATH?");
        }
        
        _process.ErrorDataReceived += ProcessOnErrorDataReceived;
        
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        FileName = fileName;
        
        cancellationToken.Register(StopRecording);
    }
    private void StopRecording()
    {
        if (_process == null || _process.HasExited)
        {
            _logger.LogWarning("Recording process is not running or has already exited.");
            return;
        }

        try
        {
            _logger.LogInformation("Stopping recording for {device}", Camera);

            // Send a graceful termination signal
            _process.StandardInput.WriteLine("q");
            _process.StandardInput.Flush();
            
            // Wait for the process to exit
            _process.WaitForExit(5000); // Wait up to 5 seconds for clean termination

            if (!_process.HasExited)
            {
                _logger.LogWarning("Recording process did not stop cleanly. Forcing termination...");
                _process.Kill(); // Force termination if necessary
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording process for {device}", Camera);
        }
        finally
        {
            _process.Dispose();
            _process = null;
        }
    }


    private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
            return;
        
        if (StartDate == null && e.Data.StartsWith("frame="))
        {
            StartDate = DateTimeOffset.Now;
            _dashCam.InvokeRecordingActivity(new RecordingEventArgs(Camera.Label, true));
        }
        
        if (e.Data.Contains("Cannot open") || e.Data.Contains("Device or resource busy") || e.Data.Contains("error", StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogError("{data}", e.Data);
            _dashCam.InvokeWarning();
        }
        else
        {
            _logger.LogInformation("{data}", e.Data);
        }
    }

    public void Dispose()
    {
        _dashCam.InvokeRecordingActivity(new RecordingEventArgs(Camera.Label, false));
        _process?.Dispose();
    }
}