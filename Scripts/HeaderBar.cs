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
    
    public HeaderBar()
    {
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
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
        
        // Capture the viewport
        var viewport = GetViewport();
        var image = viewport.GetTexture().GetImage();
        
        // Save the image
        var error = image.SavePng(filepath);
        if (error == Error.Ok)
        {
            GD.Print($"Screenshot saved to: {filepath}");
            ShowScreenshotNotification("Screenshot saved!");
        }
        else
        {
            GD.PrintErr($"Failed to save screenshot: {error}");
            ShowScreenshotNotification("Screenshot failed!");
        }
    }
    
    private void OpenGallery()
    {
        // Switch to gallery scene
        GetTree().ChangeSceneToFile("res://gallery.tscn");
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
}