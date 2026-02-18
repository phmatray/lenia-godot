using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using GodotFileAccess = Godot.FileAccess;

public partial class TimelapseRecorder : Node
{
    [Signal]
    public delegate void RecordingStartedEventHandler();
    
    [Signal]
    public delegate void RecordingStoppedEventHandler(string videoPath);
    
    [Signal]
    public delegate void RecordingProgressEventHandler(int currentFrame, int totalFrames);
    
    [Export] public bool IsRecording = false;
    [Export] public int TargetFPS = 30;
    [Export] public int MaxRecordingMinutes = 5;
    [Export] public float PlaybackSpeed = 1.0f;
    [Export] public Vector2I RecordingResolution = new Vector2I(512, 512);
    
    public enum RecordingQuality
    {
        Fast,      // 15 FPS, lower resolution
        Balanced,  // 30 FPS, medium resolution  
        High,      // 60 FPS, high resolution
        Custom     // User defined
    }
    
    private class RecordingFrame
    {
        public Image FrameImage;
        public float Timestamp;
        public Dictionary<string, object> Metadata;
    }
    
    private List<RecordingFrame> recordedFrames;
    private LeniaSimulation simulation;
    private float recordingStartTime;
    private float lastCaptureTime;
    private float captureInterval;
    private int maxFrames;
    private RecordingQuality currentQuality;
    private string currentRecordingPath;
    
    // Video export settings
    private bool isExporting = false;
    
    public override void _Ready()
    {
        recordedFrames = new List<RecordingFrame>();
        SetRecordingQuality(RecordingQuality.Balanced);
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
    }
    
    public void SetRecordingQuality(RecordingQuality quality)
    {
        currentQuality = quality;
        
        switch (quality)
        {
            case RecordingQuality.Fast:
                TargetFPS = 15;
                RecordingResolution = new Vector2I(256, 256);
                break;
            case RecordingQuality.Balanced:
                TargetFPS = 30;
                RecordingResolution = new Vector2I(512, 512);
                break;
            case RecordingQuality.High:
                TargetFPS = 60;
                RecordingResolution = new Vector2I(1024, 1024);
                break;
            case RecordingQuality.Custom:
                // Keep current settings
                break;
        }
        
        captureInterval = 1.0f / TargetFPS;
        maxFrames = TargetFPS * 60 * MaxRecordingMinutes; // FPS * seconds * minutes
    }
    
    public override void _Process(double delta)
    {
        if (IsRecording && simulation != null)
        {
            float currentTime = (float)Time.GetUnixTimeFromSystem();
            
            if (currentTime - lastCaptureTime >= captureInterval)
            {
                CaptureFrame();
                lastCaptureTime = currentTime;
            }
            
            // Check if we've reached the maximum recording time
            if (recordedFrames.Count >= maxFrames)
            {
                StopRecording();
            }
        }
    }
    
    public void StartRecording()
    {
        if (IsRecording || simulation == null) return;
        
        // Clear previous recording
        ClearRecording();
        
        IsRecording = true;
        recordingStartTime = (float)Time.GetUnixTimeFromSystem();
        lastCaptureTime = recordingStartTime;
        
        // Create recording directory
        var timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        currentRecordingPath = $"user://recordings/timelapse_{timestamp}/";
        DirAccess.MakeDirRecursiveAbsolute(currentRecordingPath);
        
        GD.Print($"Started time-lapse recording: {currentRecordingPath}");
        EmitSignal(SignalName.RecordingStarted);
    }
    
    public void StopRecording()
    {
        if (!IsRecording) return;
        
        IsRecording = false;
        float recordingDuration = (float)Time.GetUnixTimeFromSystem() - recordingStartTime;
        
        GD.Print($"Stopped recording. Duration: {recordingDuration:F1}s, Frames: {recordedFrames.Count}");
        
        // Save recording metadata
        SaveRecordingMetadata(recordingDuration);
        
        // Export video in background
        if (recordedFrames.Count > 0)
        {
            ExportTimelapseVideo();
        }
        
        EmitSignal(SignalName.RecordingStopped, currentRecordingPath);
    }
    
    public void PauseRecording()
    {
        IsRecording = false;
    }
    
    public void ResumeRecording()
    {
        if (recordedFrames.Count > 0)
        {
            IsRecording = true;
            lastCaptureTime = (float)Time.GetUnixTimeFromSystem();
        }
    }
    
    private void CaptureFrame()
    {
        if (simulation == null) return;
        
        // Get the current simulation image
        var originalImage = simulation.GetCanvasScreenshot();
        if (originalImage == null) return;
        
        // Resize if needed
        var frameImage = originalImage;
        if (originalImage.GetSize() != RecordingResolution)
        {
            frameImage = originalImage.Duplicate() as Image;
            frameImage.Resize(RecordingResolution.X, RecordingResolution.Y, Image.Interpolation.Lanczos);
        }
        
        // Create recording frame with metadata
        var recordingFrame = new RecordingFrame
        {
            FrameImage = frameImage,
            Timestamp = (float)Time.GetUnixTimeFromSystem() - recordingStartTime,
            Metadata = new Dictionary<string, object>
            {
                ["frame_number"] = recordedFrames.Count,
                ["delta_time"] = simulation.DeltaTime,
                ["kernel_radius"] = simulation.KernelRadius,
                ["growth_mean"] = simulation.GrowthMean,
                ["growth_sigma"] = simulation.GrowthSigma,
                ["simulation_speed"] = simulation.SimulationSpeed,
                ["color_scheme"] = simulation.CurrentColorScheme.ToString(),
                ["population"] = CalculatePopulation()
            }
        };
        
        recordedFrames.Add(recordingFrame);
        
        // Save frame to disk for large recordings
        if (recordedFrames.Count % 100 == 0) // Every 100 frames
        {
            SaveFramesToDisk();
        }
        
        EmitSignal(SignalName.RecordingProgress, recordedFrames.Count, maxFrames);
    }
    
    private float CalculatePopulation()
    {
        if (simulation == null) return 0.0f;
        
        var grid = simulation.GetCurrentGrid();
        float totalPopulation = 0.0f;
        int totalCells = simulation.GridWidth * simulation.GridHeight;
        
        for (int x = 0; x < simulation.GridWidth; x++)
        {
            for (int y = 0; y < simulation.GridHeight; y++)
            {
                totalPopulation += grid[x, y];
            }
        }
        
        return totalPopulation / totalCells;
    }
    
    private void SaveFramesToDisk()
    {
        if (string.IsNullOrEmpty(currentRecordingPath)) return;
        
        var framesPath = currentRecordingPath + "frames/";
        DirAccess.MakeDirRecursiveAbsolute(framesPath);
        
        // Save recent frames (keep last 50 in memory)
        int startIndex = Math.Max(0, recordedFrames.Count - 100);
        for (int i = startIndex; i < recordedFrames.Count - 50; i++)
        {
            var frame = recordedFrames[i];
            var framePath = framesPath + $"frame_{i:D6}.png";
            frame.FrameImage.SavePng(framePath);
            
            // Free memory
            frame.FrameImage = null;
        }
    }
    
    private void SaveRecordingMetadata(float duration)
    {
        if (string.IsNullOrEmpty(currentRecordingPath)) return;
        
        var metadata = new Godot.Collections.Dictionary
        {
            ["recording_info"] = new Godot.Collections.Dictionary
            {
                ["duration_seconds"] = duration,
                ["total_frames"] = recordedFrames.Count,
                ["target_fps"] = TargetFPS,
                ["resolution"] = $"{RecordingResolution.X}x{RecordingResolution.Y}",
                ["quality"] = currentQuality.ToString(),
                ["timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            },
            ["simulation_settings"] = new Godot.Collections.Dictionary
            {
                ["initial_delta_time"] = simulation?.DeltaTime ?? 0.1f,
                ["initial_kernel_radius"] = simulation?.KernelRadius ?? 13.0f,
                ["initial_growth_mean"] = simulation?.GrowthMean ?? 0.15f,
                ["initial_growth_sigma"] = simulation?.GrowthSigma ?? 0.015f,
                ["grid_size"] = $"{simulation?.GridWidth ?? 128}x{simulation?.GridHeight ?? 128}"
            }
        };
        
        var jsonString = Json.Stringify(metadata);
        var metadataPath = currentRecordingPath + "metadata.json";
        var file = GodotFileAccess.Open(metadataPath, GodotFileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(jsonString);
            file.Close();
        }
    }
    
    private void ExportTimelapseVideo()
    {
        if (isExporting || recordedFrames.Count == 0) return;
        
        isExporting = true;
        
        // Create video using FFmpeg or similar (simplified version)
        // In a real implementation, you'd use FFmpeg or another video encoder
        ExportAsImageSequence();
        CreateVideoPreview();
        
        isExporting = false;
    }
    
    private void ExportAsImageSequence()
    {
        var sequencePath = currentRecordingPath + "sequence/";
        DirAccess.MakeDirRecursiveAbsolute(sequencePath);
        
        for (int i = 0; i < recordedFrames.Count; i++)
        {
            var frame = recordedFrames[i];
            if (frame.FrameImage != null)
            {
                var framePath = sequencePath + $"frame_{i:D6}.png";
                frame.FrameImage.SavePng(framePath);
            }
            
            // Update progress
            if (i % 10 == 0)
            {
                CallDeferred(nameof(UpdateExportProgress), i, recordedFrames.Count);
            }
        }
        
        // Create FFmpeg command file for easy video creation
        CreateFFmpegCommandFile(sequencePath);
    }
    
    private void CreateFFmpegCommandFile(string sequencePath)
    {
        var commandPath = currentRecordingPath + "create_video.txt";
        var outputVideoPath = currentRecordingPath + "timelapse.mp4";
        
        // FFmpeg command to create video from image sequence
        var ffmpegCommand = $"ffmpeg -r {TargetFPS} -i \"{sequencePath}frame_%06d.png\" " +
                          $"-c:v libx264 -pix_fmt yuv420p -crf 18 " +
                          $"\"{outputVideoPath}\"";
        
        var file = GodotFileAccess.Open(commandPath, GodotFileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString($"# Run this command to create the video:\n{ffmpegCommand}\n\n");
            file.StoreString($"# Alternative with different settings:\n");
            file.StoreString($"ffmpeg -r {TargetFPS} -i \"{sequencePath}frame_%06d.png\" ");
            file.StoreString($"-c:v libx264 -preset slow -crf 22 \"{outputVideoPath}\"\n");
            file.Close();
        }
    }
    
    private void CreateVideoPreview()
    {
        // Create a preview by sampling key frames
        var previewPath = currentRecordingPath + "preview/";
        DirAccess.MakeDirRecursiveAbsolute(previewPath);
        
        int previewFrameCount = 20; // Create 20 preview frames
        int frameStep = Math.Max(1, recordedFrames.Count / previewFrameCount);
        
        for (int i = 0; i < previewFrameCount && i * frameStep < recordedFrames.Count; i++)
        {
            int frameIndex = i * frameStep;
            var frame = recordedFrames[frameIndex];
            if (frame.FrameImage != null)
            {
                var previewFramePath = previewPath + $"preview_{i:D2}.png";
                frame.FrameImage.SavePng(previewFramePath);
            }
        }
        
        CreatePreviewHTML();
    }
    
    private void CreatePreviewHTML()
    {
        var htmlPath = currentRecordingPath + "preview.html";
        var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>Lenia Time-lapse Preview</title>
    <style>
        body { font-family: Arial, sans-serif; text-align: center; background: #1a1a2e; color: white; }
        .preview-container { margin: 20px auto; max-width: 800px; }
        .frame { margin: 10px; display: inline-block; }
        .frame img { width: 150px; height: 150px; border: 2px solid #16213e; border-radius: 8px; }
        .info { background: #16213e; padding: 20px; border-radius: 10px; margin: 20px; }
        .play-button { background: #0f3460; color: white; padding: 15px 30px; border: none; border-radius: 25px; font-size: 16px; cursor: pointer; }
        .play-button:hover { background: #16537e; }
    </style>
</head>
<body>
    <h1>🌟 Lenia Time-lapse Recording</h1>
    <div class='info'>
        <h3>Recording Information</h3>
        <p><strong>Frames:</strong> " + recordedFrames.Count + @"</p>
        <p><strong>Duration:</strong> " + (recordedFrames.Count / (float)TargetFPS).ToString("F1") + @" seconds</p>
        <p><strong>Resolution:</strong> " + RecordingResolution.X + "x" + RecordingResolution.Y + @"</p>
        <p><strong>FPS:</strong> " + TargetFPS + @"</p>
    </div>
    <div class='preview-container'>
        <h3>Preview Frames</h3>";

        // Add preview frames
        for (int i = 0; i < 20; i++)
        {
            htmlContent += $"<div class='frame'><img src='preview/preview_{i:D2}.png' alt='Frame {i}'></div>";
        }
        
        htmlContent += @"
    </div>
    <div class='info'>
        <h3>Create Video</h3>
        <p>To create a video file, run the command in <strong>create_video.txt</strong> using FFmpeg.</p>
        <button class='play-button' onclick='alert(""Use FFmpeg with the command file to create the video!"")'>
            🎬 Create Video
        </button>
    </div>
</body>
</html>";
        
        var file = GodotFileAccess.Open(htmlPath, GodotFileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(htmlContent);
            file.Close();
        }
    }
    
    private void UpdateExportProgress(int current, int total)
    {
        // This would be called to update UI progress
        var percentage = (current / (float)total) * 100;
        GD.Print($"Export progress: {percentage:F1}%");
    }
    
    public void ClearRecording()
    {
        if (IsRecording)
        {
            StopRecording();
        }
        
        recordedFrames.Clear();
        currentRecordingPath = "";
    }
    
    public int GetRecordedFrameCount()
    {
        return recordedFrames.Count;
    }
    
    public float GetRecordingDuration()
    {
        return recordedFrames.Count / (float)TargetFPS;
    }
    
    public bool CanStartRecording()
    {
        return simulation != null && !IsRecording && !isExporting;
    }
    
    public string GetCurrentRecordingPath()
    {
        return currentRecordingPath;
    }
    
    public void SetCustomSettings(int fps, Vector2I resolution, int maxMinutes)
    {
        if (IsRecording) return; // Can't change settings during recording
        
        currentQuality = RecordingQuality.Custom;
        TargetFPS = Mathf.Clamp(fps, 5, 120);
        RecordingResolution = new Vector2I(
            Mathf.Clamp(resolution.X, 64, 2048),
            Mathf.Clamp(resolution.Y, 64, 2048)
        );
        MaxRecordingMinutes = Mathf.Clamp(maxMinutes, 1, 30);
        
        captureInterval = 1.0f / TargetFPS;
        maxFrames = TargetFPS * 60 * MaxRecordingMinutes;
    }
    
    // Playback functionality for reviewing recordings
    public void PlaybackRecording(string recordingPath, Control displayTarget)
    {
        // Implementation for playing back recorded time-lapses
        // This would load frames and play them back in sequence
        CallDeferred(nameof(StartPlayback), recordingPath, displayTarget);
    }
    
    private void StartPlayback(string recordingPath, Control displayTarget)
    {
        // Load and playback the recording
        // This would be a separate feature for reviewing recordings
        GD.Print($"Starting playback of: {recordingPath}");
    }
}