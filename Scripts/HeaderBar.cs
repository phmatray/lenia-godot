using Godot;

public partial class HeaderBar : Panel
{
    private LeniaSimulation simulation;
    private Button playPauseButton;
    private Button stepButton;
    private Button resetButton;
    private Button screenshotButton;
    private Button galleryButton;
    private HSlider speedSlider;
    private Label speedLabel;
    private Label fpsLabel;
    private bool isPaused = false;
    
    // New feature buttons
    private Button patternLibraryButton;
    private Button tutorialButton;
    private Button challengeButton;
    private Button recordButton;
    private Button audioToggleButton;
    private Button particleToggleButton;
    private LeniaMainUI mainUI;
    
    public HeaderBar()
    {
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
        mainUI = GetParent().GetParent() as LeniaMainUI; // Get reference to main UI
        SetupHeaderBar();
    }
    
    public override void _Ready()
    {
        // Get references to scene nodes
        playPauseButton = GetNode<Button>("HBoxContainer/PlayPauseButton");
        stepButton = GetNode<Button>("HBoxContainer/StepButton");
        resetButton = GetNode<Button>("HBoxContainer/ResetButton");
        screenshotButton = GetNode<Button>("HBoxContainer/ScreenshotButton");
        galleryButton = GetNode<Button>("HBoxContainer/GalleryButton");
        speedSlider = GetNode<HSlider>("HBoxContainer/SpeedContainer/SpeedSlider");
        speedLabel = GetNode<Label>("HBoxContainer/SpeedContainer/SpeedValue");
        fpsLabel = GetNode<Label>("HBoxContainer/FPSLabel");
        
        // Style the panel
        var headerStyle = new StyleBoxFlat();
        headerStyle.BgColor = new Color(0.12f, 0.15f, 0.22f, 0.95f);
        headerStyle.BorderWidthBottom = 2;
        headerStyle.BorderColor = new Color(0.2f, 0.4f, 0.8f, 0.4f);
        AddThemeStyleboxOverride("panel", headerStyle);
        
        // Style buttons
        StyleButton(playPauseButton);
        StyleButton(stepButton);
        StyleButton(resetButton);
        StyleButton(screenshotButton);
        StyleButton(galleryButton);
        
        // Create and add new feature buttons
        CreateNewFeatureButtons();
    }
    
    private void SetupHeaderBar()
    {
        // Connect signals
        playPauseButton.Pressed += OnPlayPausePressed;
        stepButton.Pressed += OnStepPressed;
        resetButton.Pressed += OnResetPressed;
        screenshotButton.Pressed += OnScreenshotPressed;
        galleryButton.Pressed += OnGalleryPressed;
        speedSlider.ValueChanged += OnSpeedChanged;
        
        // Set initial values
        speedSlider.Value = simulation.SimulationSpeed;
        speedLabel.Text = $"{simulation.SimulationSpeed:F1}x";
        
        // Set initial play/pause state based on simulation
        isPaused = simulation.IsPaused;
        UpdatePlayPauseButton();
    }
    
    private void StyleButton(Button button)
    {
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.2f, 0.3f, 0.5f, 0.8f);
        buttonStyle.SetBorderWidthAll(1);
        buttonStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f);
        buttonStyle.SetCornerRadiusAll(6);
        button.AddThemeStyleboxOverride("normal", buttonStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.3f, 0.4f, 0.6f, 0.9f);
        hoverStyle.SetBorderWidthAll(1);
        hoverStyle.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        hoverStyle.SetCornerRadiusAll(6);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
    }
    
    private void OnPlayPausePressed()
    {
        isPaused = !isPaused;
        simulation.SetPaused(isPaused);
        UpdatePlayPauseButton();
    }
    
    private void UpdatePlayPauseButton()
    {
        playPauseButton.Text = isPaused ? "▶" : "⏸";
        playPauseButton.TooltipText = isPaused ? "Resume simulation" : "Pause simulation";
    }
    
    private void OnStepPressed()
    {
        if (!isPaused)
        {
            OnPlayPausePressed(); // Pause first
        }
        simulation.StepOneFrame();
    }
    
    private void OnResetPressed()
    {
        simulation.InitializePattern();
    }
    
    private void OnSpeedChanged(double value)
    {
        simulation.SimulationSpeed = (float)value;
        speedLabel.Text = $"{value:F1}x";
        
        // Update color based on speed
        if (value == 0)
        {
            speedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.4f, 0.4f)); // Red for paused
        }
        else if (value > 1.5f)
        {
            speedLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.8f, 0.4f)); // Orange for fast
        }
        else
        {
            speedLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 1.0f)); // Blue for normal
        }
    }
    
    public override void _Process(double delta)
    {
        if (fpsLabel != null)
        {
            fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
            
            // Update FPS color based on performance
            var fps = Engine.GetFramesPerSecond();
            if (fps >= 50)
            {
                fpsLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 0.6f)); // Green
            }
            else if (fps >= 30)
            {
                fpsLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.4f)); // Yellow
            }
            else
            {
                fpsLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.4f, 0.4f)); // Red
            }
        }
    }
    
    private void OnScreenshotPressed()
    {
        TakeScreenshot();
    }
    
    private void OnGalleryPressed()
    {
        GD.Print("Gallery button pressed - opening gallery");
        OpenGallery();
    }
    
    private void TakeScreenshot()
    {
        // Create screenshots directory if it doesn't exist
        var screenshotsDir = "user://screenshots/";
        if (!DirAccess.DirExistsAbsolute(screenshotsDir))
        {
            DirAccess.MakeDirRecursiveAbsolute(screenshotsDir);
        }
        
        // Generate filename with timestamp
        var timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var filename = $"lenia_screenshot_{timestamp}.png";
        var filepath = screenshotsDir + filename;
        var metadataPath = screenshotsDir + $"lenia_screenshot_{timestamp}.json";
        
        // Get canvas content from simulation
        var canvasImage = simulation.GetCanvasScreenshot();
        if (canvasImage == null)
        {
            GD.PrintErr("Failed to get canvas screenshot from simulation");
            ShowScreenshotNotification("Screenshot failed!");
            return;
        }
        
        // Save the canvas image
        var error = canvasImage.SavePng(filepath);
        if (error == Error.Ok)
        {
            // Save metadata
            SaveScreenshotMetadata(metadataPath);
            
            GD.Print($"Screenshot saved to: {filepath}");
            ShowScreenshotNotification("Screenshot saved!");
        }
        else
        {
            GD.PrintErr($"Failed to save screenshot: {error}");
            ShowScreenshotNotification("Screenshot failed!");
        }
    }
    
    private void SaveScreenshotMetadata(string metadataPath)
    {
        var metadata = new Godot.Collections.Dictionary();
        metadata["timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        metadata["grid_size"] = $"{simulation.GridWidth}x{simulation.GridHeight}";
        metadata["delta_time"] = simulation.DeltaTime;
        metadata["kernel_radius"] = simulation.KernelRadius;
        metadata["growth_mean"] = simulation.GrowthMean;
        metadata["growth_sigma"] = simulation.GrowthSigma;
        metadata["simulation_speed"] = simulation.SimulationSpeed;
        metadata["color_scheme"] = simulation.CurrentColorScheme.ToString();
        metadata["brush_size"] = simulation.BrushSize;
        metadata["brush_intensity"] = simulation.BrushIntensity;
        
        // Calculate population
        var grid = simulation.GetCurrentGrid();
        float totalPopulation = 0;
        int totalCells = simulation.GridWidth * simulation.GridHeight;
        
        for (int x = 0; x < simulation.GridWidth; x++)
        {
            for (int y = 0; y < simulation.GridHeight; y++)
            {
                totalPopulation += grid[x, y];
            }
        }
        
        var populationPercentage = (totalPopulation / totalCells) * 100;
        metadata["population_percent"] = populationPercentage;
        metadata["total_population"] = totalPopulation;
        
        var jsonString = Json.Stringify(metadata);
        var file = FileAccess.Open(metadataPath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(jsonString);
            file.Close();
        }
    }
    
    private void OpenGallery()
    {
        GD.Print("OpenGallery called - changing scene to gallery.tscn");
        
        // Check if gallery scene exists first
        if (!FileAccess.FileExists("res://gallery.tscn"))
        {
            GD.PrintErr("Gallery scene file does not exist!");
            return;
        }
        
        // Switch to gallery scene
        var result = GetTree().ChangeSceneToFile("res://gallery.tscn");
        GD.Print($"ChangeSceneToFile result: {result}");
        
        if (result != Error.Ok)
        {
            GD.PrintErr($"Failed to change scene to gallery: {result}");
        }
    }
    
    private void ShowScreenshotNotification(string message)
    {
        // Create a temporary notification label
        var notification = new Label();
        notification.Text = message;
        notification.AddThemeColorOverride("font_color", new Color(0.8f, 1.0f, 0.8f));
        notification.AddThemeFontSizeOverride("font_size", 14);
        notification.Position = new Vector2(GetViewport().GetVisibleRect().Size.X - 200, 60);
        notification.Size = new Vector2(180, 30);
        notification.HorizontalAlignment = HorizontalAlignment.Center;
        notification.VerticalAlignment = VerticalAlignment.Center;
        
        GetViewport().AddChild(notification);
        
        // Remove after 2 seconds
        var timer = GetTree().CreateTimer(2.0);
        timer.Timeout += () => notification.QueueFree();
    }
    
    private void CreateNewFeatureButtons()
    {
        var container = GetNode<HBoxContainer>("HBoxContainer");
        
        // Add separator
        var separator = new VSeparator();
        separator.AddThemeStyleboxOverride("separator", new StyleBoxFlat());
        container.AddChild(separator);
        
        // Pattern Library button
        patternLibraryButton = new Button();
        patternLibraryButton.Text = "📚";
        patternLibraryButton.TooltipText = "Pattern Library";
        patternLibraryButton.CustomMinimumSize = new Vector2(35, 0);
        patternLibraryButton.Pressed += () => mainUI?.ShowPatternLibrary();
        StyleButton(patternLibraryButton);
        container.AddChild(patternLibraryButton);
        
        // Tutorial button
        tutorialButton = new Button();
        tutorialButton.Text = "🎓";
        tutorialButton.TooltipText = "Tutorials";
        tutorialButton.CustomMinimumSize = new Vector2(35, 0);
        tutorialButton.Pressed += () => mainUI?.ShowTutorials();
        StyleButton(tutorialButton);
        container.AddChild(tutorialButton);
        
        // Challenge button
        challengeButton = new Button();
        challengeButton.Text = "🏆";
        challengeButton.TooltipText = "Challenges";
        challengeButton.CustomMinimumSize = new Vector2(35, 0);
        challengeButton.Pressed += () => mainUI?.ShowChallenges();
        StyleButton(challengeButton);
        container.AddChild(challengeButton);
        
        // Record button
        recordButton = new Button();
        recordButton.Text = "🎬";
        recordButton.TooltipText = "Start/Stop Time-lapse Recording";
        recordButton.CustomMinimumSize = new Vector2(35, 0);
        recordButton.Pressed += OnRecordPressed;
        StyleButton(recordButton);
        container.AddChild(recordButton);
        
        // Another separator
        var separator2 = new VSeparator();
        separator2.AddThemeStyleboxOverride("separator", new StyleBoxFlat());
        container.AddChild(separator2);
        
        // Audio toggle button
        audioToggleButton = new Button();
        audioToggleButton.Text = "🔊";
        audioToggleButton.TooltipText = "Toggle Audio";
        audioToggleButton.CustomMinimumSize = new Vector2(35, 0);
        audioToggleButton.ToggleMode = true;
        audioToggleButton.ButtonPressed = true; // Audio on by default
        audioToggleButton.Toggled += OnAudioToggled;
        StyleToggleButton(audioToggleButton);
        container.AddChild(audioToggleButton);
        
        // Particle toggle button
        particleToggleButton = new Button();
        particleToggleButton.Text = "✨";
        particleToggleButton.TooltipText = "Toggle Particle Effects";
        particleToggleButton.CustomMinimumSize = new Vector2(35, 0);
        particleToggleButton.ToggleMode = true;
        particleToggleButton.ButtonPressed = true; // Particles on by default
        particleToggleButton.Toggled += OnParticleToggled;
        StyleToggleButton(particleToggleButton);
        container.AddChild(particleToggleButton);
    }
    
    private void StyleToggleButton(Button button)
    {
        StyleButton(button);
        
        // Additional styling for toggle state
        var pressedStyle = new StyleBoxFlat();
        pressedStyle.BgColor = new Color(0.3f, 0.5f, 0.7f, 0.9f);
        pressedStyle.SetBorderWidthAll(1);
        pressedStyle.BorderColor = new Color(0.5f, 0.7f, 1.0f);
        pressedStyle.SetCornerRadiusAll(6);
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
    }
    
    private bool isRecording = false;
    
    private void OnRecordPressed()
    {
        if (isRecording)
        {
            mainUI?.StopTimelapseRecording();
            recordButton.Text = "🎬";
            recordButton.TooltipText = "Start Time-lapse Recording";
            isRecording = false;
        }
        else
        {
            mainUI?.StartTimelapseRecording();
            recordButton.Text = "⏹";
            recordButton.TooltipText = "Stop Time-lapse Recording";
            isRecording = true;
        }
    }
    
    private void OnAudioToggled(bool enabled)
    {
        mainUI?.ToggleAudio(enabled);
        audioToggleButton.Text = enabled ? "🔊" : "🔇";
        audioToggleButton.TooltipText = enabled ? "Mute Audio" : "Enable Audio";
    }
    
    private void OnParticleToggled(bool enabled)
    {
        mainUI?.ToggleParticles(enabled);
        particleToggleButton.Text = enabled ? "✨" : "💨";
        particleToggleButton.TooltipText = enabled ? "Disable Particles" : "Enable Particles";
    }
}