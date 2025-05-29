using Godot;

public partial class HeaderBar : Panel
{
    private LeniaSimulation simulation;
    private Button playPauseButton;
    private Button stepButton;
    private Button resetButton;
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
    }
    
    private void SetupHeaderBar()
    {
        // Connect signals
        playPauseButton.Pressed += OnPlayPausePressed;
        stepButton.Pressed += OnStepPressed;
        resetButton.Pressed += OnResetPressed;
        speedSlider.ValueChanged += OnSpeedChanged;
        
        // Set initial values
        speedSlider.Value = simulation.SimulationSpeed;
        speedLabel.Text = $"{simulation.SimulationSpeed:F1}x";
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
}