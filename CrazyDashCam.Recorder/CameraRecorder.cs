using System.Diagnostics;
using System.Runtime.InteropServices;
using CrazyDashCam.Recorder.Configuration;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder;

public class CameraRecorder(
    DashCam dashCam,
    ILogger logger,
    CameraConfiguration camera,
    string videoEncoder,
    string audioEncoder)
    : IDisposable
{
    public CameraConfiguration Camera { get; } = camera;
    public bool Recording { get; private set; } = false;
    private Process? _process = null;

    private string VideoEncoder { get; } = videoEncoder;
    private string AudioEncoder { get; } = audioEncoder;
    public DateTimeOffset? StartDate { get; private set; }
    public string? FileName { get; private set; }

    private CancellationToken? _recordingCancellationToken;

    private static string GetVideoFormat()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "v4l2";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "dshow";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "avfoundation";
        
        throw new Exception("Unsupported OS");
    }
    
    private static string GetAudioFormat()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "alsa"; // Most common for Linux
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "dshow";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "avfoundation";

        throw new Exception("Unsupported OS");
    }

    private static string GetFfmpegArguments(CameraConfiguration camera, string videoEncoder, string audioEncoder, string output)
    {
        string arguments = "";
        string videoFormat = GetVideoFormat();

        arguments += $" -framerate {camera.Fps}" +
                     $" -f {videoFormat}" +
                     $" -rtbufsize {camera.BufferSizeMb}M" +
                     $" -video_size {camera.ResolutionWidth}x{camera.ResolutionHeight}";

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
            $" -preset veryfast" +
            $" -b:v {camera.VideoBitrateKbps}k" +
            $" -g 10" +
            $" -fps_mode vfr";

        if (camera.RecordAudio)
            arguments += $" -c:a {audioEncoder}" +
                         $" -b:a {camera.AudioBitrateKbps}k";

        arguments +=
            $" -vf format=yuv420p" +
            $" -movflags +faststart" +
            $" -async 1" + // Ensure audio and video are in sync
            $" -threads {camera.Threads}" +
            $" -y" +
            $" \"{output}\"";

        return arguments;
    }
    public void StartRecording(string directory, string fileName, CancellationToken cancellationToken)
    {
        string output = Path.Combine(directory, fileName);
        FileName = fileName;
        logger.LogInformation("Starting recording for {device} at {output}", Camera, output);
        
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
        
        _process.ErrorDataReceived += ProcessOnDataReceived;
        _process.OutputDataReceived += ProcessOnDataReceived;
        _process.Exited += ProcessOnExited;

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        _recordingCancellationToken = cancellationToken;
        _recordingCancellationToken.Value.Register(StopRecording);
    }
    
    private void ProcessOnExited(object? sender, EventArgs e)
    {
        logger.LogInformation("{camera} process exited", Camera.Label);
    }

    private void StopRecording()
    {
        Recording = false;
        
        if (_process == null || _process.HasExited)
        {
            logger.LogWarning("Recording process is not running or has already exited.");
            dashCam.InvokeWarning();
        }

        try
        {
            logger.LogInformation("Stopping recording for {device}", Camera);

            // Send a graceful termination signal
            _process!.StandardInput.WriteLine("q");
            _process.StandardInput.Flush();
            
            // Wait for the process to exit
            _process.WaitForExit(5000); // Wait up to 5 seconds for clean termination

            if (!_process.HasExited)
            {
                logger.LogWarning("Recording process did not stop cleanly. Forcing termination...");
                dashCam.InvokeWarning();
                _process.Kill(); // Force termination if necessary
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping recording process for {device}", Camera);
        }
        finally
        {
            _process?.Dispose();
            _process = null;
            dashCam.InvokeRecordingActivity(this);
            StartDate = null;
        }
    }
    
    private void ProcessOnDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
            return;
        
        if (!_recordingCancellationToken!.Value.IsCancellationRequested && !Recording && e.Data.StartsWith("frame="))
        {
            Recording = true;
            StartDate = DateTimeOffset.Now;
            dashCam.InvokeRecordingActivity(this);
        }
        
        if (e.Data.Contains("Cannot open") || e.Data.Contains("Device or resource busy") || e.Data.Contains("error", StringComparison.InvariantCultureIgnoreCase))
        {
            logger.LogError("{data}", e.Data);
            dashCam.InvokeWarning();
        }
        else
        {
            logger.LogDebug("{data}", e.Data);
        }
    }
    
    public void Dispose()
    {
        if (_process != null)
            StopRecording();
        
        _process?.Dispose();
    }
}