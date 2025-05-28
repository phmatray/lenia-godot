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
    private PopulationGraph populationGraph;
    
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
        
        // Add animated background
        var animatedBg = new AnimatedBackground();
        AddChild(animatedBg);
        
        controlPanel = new Panel();
        controlPanel.AnchorLeft = 0;
        controlPanel.AnchorTop = 0;
        controlPanel.AnchorRight = 0;
        controlPanel.AnchorBottom = 1;
        controlPanel.CustomMinimumSize = new Vector2(400, 0);
        controlPanel.Size = new Vector2(400, 0);
        
        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.08f, 0.1f, 0.18f, 0.95f);
        panelStyle.BorderWidthRight = 3;
        panelStyle.BorderColor = new Color(0.2f, 0.4f, 0.8f, 0.6f);
        panelStyle.CornerRadiusTopRight = 10;
        panelStyle.CornerRadiusBottomRight = 10;
        panelStyle.ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        panelStyle.ShadowSize = 8;
        panelStyle.ShadowOffset = new Vector2(2, 2);
        controlPanel.AddThemeStyleboxOverride("panel", panelStyle);
        AddChild(controlPanel);
        
        container = new VBoxContainer();
        container.Position = new Vector2(20, 20);
        container.Size = new Vector2(360, 0);
        container.AddThemeConstantOverride("separation", 15);
        controlPanel.AddChild(container);
        
        var titleLabel = new Label();
        titleLabel.Text = "LENIA CONTROLS";
        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
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
        
        var visualLabel = new Label();
        visualLabel.Text = "VISUALS";
        visualLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        visualLabel.AddThemeFontSizeOverride("font_size", 18);
        visualLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(visualLabel);
        
        CreateColorSchemeSelector();
        
        CreatePerformanceSelector();
        
        container.AddChild(CreateSeparator());
        
        var interactionLabel = new Label();
        interactionLabel.Text = "INTERACTION";
        interactionLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        interactionLabel.AddThemeFontSizeOverride("font_size", 18);
        interactionLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(interactionLabel);
        
        CreateSlider("Brush Size", 1.0f, 10.0f, simulation.BrushSize, 
            value => simulation.BrushSize = value);
        
        CreateSlider("Brush Intensity", 0.1f, 3.0f, simulation.BrushIntensity, 
            value => simulation.BrushIntensity = value);
        
        CreateSlider("Simulation Speed", 0.0f, 3.0f, simulation.SimulationSpeed, 
            value => simulation.SimulationSpeed = value);
        
        // Add mouse instructions
        var instructionText = new RichTextLabel();
        instructionText.CustomMinimumSize = new Vector2(340, 80);
        instructionText.BbcodeEnabled = true;
        instructionText.AddThemeFontSizeOverride("normal_font_size", 12);
        instructionText.Text = "[center][color=#9999ff][b]Mouse Controls:[/b][/color]\n[color=#aaaacc]• Left Click: Paint organisms\n• Right Click: Erase\n• Drag to paint continuously[/color][/center]";
        container.AddChild(instructionText);
        
        container.AddChild(CreateSeparator());
        
        var patternsLabel = new Label();
        patternsLabel.Text = "PATTERNS";
        patternsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        patternsLabel.AddThemeFontSizeOverride("font_size", 18);
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
        controlsLabel.AddThemeFontSizeOverride("font_size", 18);
        controlsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(controlsLabel);
        
        pauseButton = CreateStyledButton("Pause");
        pauseButton.Pressed += OnPausePressed;
        container.AddChild(pauseButton);
        
        menuButton = CreateStyledButton("Back to Menu");
        menuButton.Pressed += OnMenuPressed;
        container.AddChild(menuButton);
        
        container.AddChild(CreateSeparator());
        
        var statsLabel = new Label();
        statsLabel.Text = "STATISTICS";
        statsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        statsLabel.AddThemeFontSizeOverride("font_size", 18);
        statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(statsLabel);
        
        // Add population graph
        populationGraph = new PopulationGraph();
        container.AddChild(populationGraph);
        
        fpsLabel = new Label();
        fpsLabel.Text = "FPS: 0";
        fpsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        fpsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(fpsLabel);
    }
    
    private HSeparator CreateSeparator()
    {
        var separator = new HSeparator();
        separator.AddThemeColorOverride("separator", new Color(0.2f, 0.4f, 0.8f, 0.5f));
        separator.CustomMinimumSize = new Vector2(0, 4);
        return separator;
    }
    
    private Button CreateStyledButton(string text)
    {
        var button = new Button();
        button.Text = text;
        button.CustomMinimumSize = new Vector2(340, 45);
        button.AddThemeFontSizeOverride("font_size", 16);
        
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.15f, 0.2f, 0.35f);
        buttonStyle.BorderWidthTop = 2;
        buttonStyle.BorderWidthBottom = 2;
        buttonStyle.BorderWidthLeft = 2;
        buttonStyle.BorderWidthRight = 2;
        buttonStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f);
        buttonStyle.CornerRadiusTopLeft = 8;
        buttonStyle.CornerRadiusTopRight = 8;
        buttonStyle.CornerRadiusBottomLeft = 8;
        buttonStyle.CornerRadiusBottomRight = 8;
        button.AddThemeStyleboxOverride("normal", buttonStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.2f, 0.3f, 0.5f);
        hoverStyle.BorderWidthTop = 2;
        hoverStyle.BorderWidthBottom = 2;
        hoverStyle.BorderWidthLeft = 2;
        hoverStyle.BorderWidthRight = 2;
        hoverStyle.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        hoverStyle.CornerRadiusTopLeft = 8;
        hoverStyle.CornerRadiusTopRight = 8;
        hoverStyle.CornerRadiusBottomLeft = 8;
        hoverStyle.CornerRadiusBottomRight = 8;
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        var pressedStyle = new StyleBoxFlat();
        pressedStyle.BgColor = new Color(0.1f, 0.15f, 0.25f);
        pressedStyle.BorderWidthTop = 2;
        pressedStyle.BorderWidthBottom = 2;
        pressedStyle.BorderWidthLeft = 2;
        pressedStyle.BorderWidthRight = 2;
        pressedStyle.BorderColor = new Color(0.5f, 0.7f, 1.0f);
        pressedStyle.CornerRadiusTopLeft = 8;
        pressedStyle.CornerRadiusTopRight = 8;
        pressedStyle.CornerRadiusBottomLeft = 8;
        pressedStyle.CornerRadiusBottomRight = 8;
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        
        return button;
    }
    
    private void CreateSlider(string name, float min, float max, float value, 
        System.Action<float> onChanged)
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        container.AddChild(vbox);
        
        var label = new Label();
        label.Text = name;
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(label);
        
        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);
        
        var slider = new HSlider();
        slider.MinValue = min;
        slider.MaxValue = max;
        slider.Value = value;
        slider.Step = (max - min) / 100.0f;
        slider.CustomMinimumSize = new Vector2(240, 30);
        hbox.AddChild(slider);
        
        var valueLabel = new Label();
        valueLabel.Text = value.ToString("F3");
        valueLabel.CustomMinimumSize = new Vector2(80, 0);
        valueLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        valueLabel.AddThemeFontSizeOverride("font_size", 14);
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
    
    private void CreateColorSchemeSelector()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        container.AddChild(vbox);
        
        var label = new Label();
        label.Text = "Color Scheme";
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(label);
        
        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);
        
        var optionButton = new OptionButton();
        optionButton.CustomMinimumSize = new Vector2(240, 35);
        optionButton.AddThemeFontSizeOverride("font_size", 14);
        
        // Add all color schemes
        var schemes = System.Enum.GetValues<ColorMapper.ColorScheme>();
        foreach (var scheme in schemes)
        {
            optionButton.AddItem(ColorMapper.GetSchemeName(scheme));
        }
        
        // Set current selection
        optionButton.Selected = (int)simulation.CurrentColorScheme;
        
        optionButton.ItemSelected += (long index) => {
            simulation.CurrentColorScheme = (ColorMapper.ColorScheme)index;
        };
        
        hbox.AddChild(optionButton);
    }
    
    private void CreatePerformanceSelector()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        container.AddChild(vbox);
        
        var label = new Label();
        label.Text = "Performance Mode";
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(label);
        
        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);
        
        var optionButton = new OptionButton();
        optionButton.CustomMinimumSize = new Vector2(240, 35);
        optionButton.AddThemeFontSizeOverride("font_size", 14);
        
        optionButton.AddItem("Fast (128x128)");
        optionButton.AddItem("Balanced (192x192)");
        optionButton.AddItem("Quality (256x256)");
        
        // Default to Fast mode
        optionButton.Selected = 0;
        
        optionButton.ItemSelected += (long index) => {
            switch (index)
            {
                case 0: // Fast
                    simulation.GridWidth = 128;
                    simulation.GridHeight = 128;
                    break;
                case 1: // Balanced
                    simulation.GridWidth = 192;
                    simulation.GridHeight = 192;
                    break;
                case 2: // Quality
                    simulation.GridWidth = 256;
                    simulation.GridHeight = 256;
                    break;
            }
            // Reinitialize simulation with new size
            simulation._Ready();
        };
        
        hbox.AddChild(optionButton);
    }
    
    public override void _Process(double delta)
    {
        fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
        
        // Update population graph every few frames for performance
        if (Engine.GetProcessFrames() % 10 == 0)
        {
            populationGraph?.UpdatePopulation(simulation.GetCurrentGrid(), simulation.GridWidth, simulation.GridHeight);
        }
    }
}