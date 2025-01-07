using System.Diagnostics;
using CrazyDashCam.Configuration;
using Microsoft.Extensions.Logging;

namespace CrazyDashCam;

public class CameraRecorder(ILogger logger, Camera camera)
{
    public Camera Camera { get; } = camera;
    private Process? _process = null;

    public void StartRecording(CancellationToken cancellationToken, string directory, string fileName)
    {
        string output = Path.Combine(directory, fileName);
        logger.LogInformation("Starting recording for {device} at {output}", Camera, output);

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $" -framerate {Camera.Fps}" + 
                        //$" -f dshow" + // todo: required for windows to work
                        $" -i {Camera.DeviceName}" +
                        $" -fps_mode vfr" + // Synchronizes video frames to maintain constant frame rate
                        $" -copyinkf" +
                        $" -preset fast" + // todo: make preset changable?
                        $" -b:v {Camera.VideoBitrate}" +
                        $" -g 10" +
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
            throw new InvalidOperationException("Failed to start the recording process.");
        }
        
        // all output is treated as errors for some reason
        _process.ErrorDataReceived += ProcessOnErrorDataReceived;
        
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        
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
        logger.LogInformation("{data}", e.Data);

        if (e.Data?.Contains("error", StringComparison.InvariantCultureIgnoreCase) ?? false)
            throw new Exception(e.Data);
    }

    public void Dispose()
    {
        StopRecording();
    }
}