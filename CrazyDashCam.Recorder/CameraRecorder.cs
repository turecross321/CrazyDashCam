using System.Diagnostics;
using System.Runtime.InteropServices;
using CrazyDashCam.Recorder.Configuration;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam.Recorder;

public class CameraRecorder(ILogger logger, Camera camera)
{
    public Camera Camera { get; } = camera;
    private Process? _process = null;
    public DateTimeOffset? StartDate { get; private set; }
    public string? FileName { get; private set; }

    private string GetSupportedFormat()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "v4l2";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "dshow";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "avfoundation";
        
        throw new Exception("Unsupported OS");
    }

    public void StartRecording(CancellationToken cancellationToken, string directory, string fileName)
    {
        string output = Path.Combine(directory, fileName);
        logger.LogInformation("Starting recording for {device} at {output}", Camera, output);

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $" -framerate {Camera.Fps}" + 
                        $" -f {GetSupportedFormat()}" +
                        $" -i {Camera.DeviceName}" +
                        $" -fps_mode vfr" + // Synchronizes video frames to maintain constant frame rate
                        $" -video_size {Camera.ResolutionWidth}x{Camera.ResolutionHeight}" +
                        $" -copyinkf" +
                        $" -preset fast" + // todo: make preset changable?
                        $" -b:v {Camera.VideoBitrate}" +
                        $" -g 10" +
                        $" -movflags +faststart" +
                        $" -vf format=yuv420p" +
                        $" \"{output}\"",
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
        
        // all output is treated as errors for some reason
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
            logger.LogWarning("Recording process is not running or has already exited.");
            return;
        }

        try
        {
            logger.LogInformation("Stopping recording for {device}", Camera);

            // Send a graceful termination signal
            _process.StandardInput.WriteLine("q");
            _process.StandardInput.Flush();
            
            // Wait for the process to exit
            _process.WaitForExit(5000); // Wait up to 5 seconds for clean termination

            if (!_process.HasExited)
            {
                logger.LogWarning("Recording process did not stop cleanly. Forcing termination...");
                _process.Kill(); // Force termination if necessary
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping recording process for {device}", Camera);
        }
        finally
        {
            _process.Dispose();
            _process = null;
        }
    }


    private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data?.StartsWith("frame=") ?? false)
        {
            StartDate ??= DateTimeOffset.Now;
        }
        
        if (e.Data?.Contains("error", StringComparison.InvariantCultureIgnoreCase) ?? false)
        {
            logger.LogError("{data}", e.Data);
        }
        else
        {
            logger.LogInformation("{data}", e.Data);
        }
    }

    public void Dispose()
    {
        StopRecording();
    }
}