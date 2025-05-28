using Godot;

public partial class LeniaUI : Control
{
    private LeniaSimulation simulation;
    private Panel controlPanel;
    private VBoxContainer container;
    private Label fpsLabel;
    private Button menuButton;
    private Button pauseButton;
    private bool isPaused = false;
    
    public override void _Ready()
    {
        simulation = GetNode<LeniaSimulation>("../LeniaSimulation");
        SetupUI();
    }
    
    private void SetupUI()
    {
        AnchorLeft = 0;
        AnchorTop = 0;
        AnchorRight = 1;
        AnchorBottom = 1;
        
        controlPanel = new Panel();
        controlPanel.AnchorLeft = 0;
        controlPanel.AnchorTop = 0;
        controlPanel.AnchorRight = 0;
        controlPanel.AnchorBottom = 1;
        controlPanel.CustomMinimumSize = new Vector2(300, 0);
        controlPanel.Size = new Vector2(300, GetViewport().GetVisibleRect().Size.Y);
        
        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        panelStyle.BorderWidthRight = 2;
        panelStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        controlPanel.AddThemeStyleboxOverride("panel", panelStyle);
        AddChild(controlPanel);
        
        container = new VBoxContainer();
        container.Position = new Vector2(15, 15);
        container.Size = new Vector2(270, 0);
        container.AddThemeConstantOverride("separation", 10);
        controlPanel.AddChild(container);
        
        var titleLabel = new Label();
        titleLabel.Text = "LENIA CONTROLS";
        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(titleLabel);
        
        container.AddChild(CreateSeparator());
        
        CreateSlider("Delta Time", 0.01f, 0.5f, simulation.DeltaTime, 
            value => simulation.DeltaTime = value);
        
        CreateSlider("Kernel Radius", 5.0f, 30.0f, simulation.KernelRadius, 
            value => {
                simulation.KernelRadius = value;
                simulation.CreateKernel();
            });
        
        CreateSlider("Growth Mean", 0.0f, 0.3f, simulation.GrowthMean, 
            value => simulation.GrowthMean = value);
        
        CreateSlider("Growth Sigma", 0.001f, 0.1f, simulation.GrowthSigma, 
            value => simulation.GrowthSigma = value);
        
        container.AddChild(CreateSeparator());
        
        var patternsLabel = new Label();
        patternsLabel.Text = "PATTERNS";
        patternsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        patternsLabel.AddThemeFontSizeOverride("font_size", 14);
        patternsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(patternsLabel);
        
        var resetButton = CreateStyledButton("Reset Pattern");
        resetButton.Pressed += () => simulation.InitializePattern();
        container.AddChild(resetButton);
        
        var randomButton = CreateStyledButton("Random Pattern");
        randomButton.Pressed += () => simulation.RandomPattern();
        container.AddChild(randomButton);
        
        var orbiumButton = CreateStyledButton("Orbium Pattern");
        orbiumButton.Pressed += () => simulation.OrbiumPattern();
        container.AddChild(orbiumButton);
        
        container.AddChild(CreateSeparator());
        
        var controlsLabel = new Label();
        controlsLabel.Text = "CONTROLS";
        controlsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        controlsLabel.AddThemeFontSizeOverride("font_size", 14);
        controlsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(controlsLabel);
        
        pauseButton = CreateStyledButton("Pause");
        pauseButton.Pressed += OnPausePressed;
        container.AddChild(pauseButton);
        
        menuButton = CreateStyledButton("Back to Menu");
        menuButton.Pressed += OnMenuPressed;
        container.AddChild(menuButton);
        
        container.AddChild(CreateSeparator());
        
        fpsLabel = new Label();
        fpsLabel.Text = "FPS: 0";
        fpsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        fpsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(fpsLabel);
    }
    
    private HSeparator CreateSeparator()
    {
        var separator = new HSeparator();
        separator.AddThemeColorOverride("separator", new Color(0.3f, 0.3f, 0.4f));
        return separator;
    }
    
    private Button CreateStyledButton(string text)
    {
        var button = new Button();
        button.Text = text;
        button.CustomMinimumSize = new Vector2(250, 35);
        
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.2f, 0.2f, 0.3f);
        buttonStyle.BorderWidthTop = 1;
        buttonStyle.BorderWidthBottom = 1;
        buttonStyle.BorderWidthLeft = 1;
        buttonStyle.BorderWidthRight = 1;
        buttonStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f);
        buttonStyle.CornerRadiusTopLeft = 4;
        buttonStyle.CornerRadiusTopRight = 4;
        buttonStyle.CornerRadiusBottomLeft = 4;
        buttonStyle.CornerRadiusBottomRight = 4;
        button.AddThemeStyleboxOverride("normal", buttonStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.3f, 0.3f, 0.4f);
        hoverStyle.BorderWidthTop = 1;
        hoverStyle.BorderWidthBottom = 1;
        hoverStyle.BorderWidthLeft = 1;
        hoverStyle.BorderWidthRight = 1;
        hoverStyle.BorderColor = new Color(0.5f, 0.5f, 0.6f);
        hoverStyle.CornerRadiusTopLeft = 4;
        hoverStyle.CornerRadiusTopRight = 4;
        hoverStyle.CornerRadiusBottomLeft = 4;
        hoverStyle.CornerRadiusBottomRight = 4;
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        
        return button;
    }
    
    private void CreateSlider(string name, float min, float max, float value, 
        System.Action<float> onChanged)
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 5);
        container.AddChild(vbox);
        
        var label = new Label();
        label.Text = name;
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 12);
        vbox.AddChild(label);
        
        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);
        
        var slider = new HSlider();
        slider.MinValue = min;
        slider.MaxValue = max;
        slider.Value = value;
        slider.Step = (max - min) / 100.0f;
        slider.CustomMinimumSize = new Vector2(180, 20);
        hbox.AddChild(slider);
        
        var valueLabel = new Label();
        valueLabel.Text = value.ToString("F3");
        valueLabel.CustomMinimumSize = new Vector2(60, 0);
        valueLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        valueLabel.AddThemeFontSizeOverride("font_size", 11);
        hbox.AddChild(valueLabel);
        
        slider.ValueChanged += (double newValue) => {
            onChanged((float)newValue);
            valueLabel.Text = newValue.ToString("F3");
        };
    }
    
    private void OnPausePressed()
    {
        isPaused = !isPaused;
        simulation.SetProcess(!isPaused);
        pauseButton.Text = isPaused ? "Resume" : "Pause";
    }
    
    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://menu.tscn");
    }
    
    public override void _Process(double delta)
    {
        fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }
}