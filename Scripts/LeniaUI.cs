using Godot;

public partial class LeniaUI : Control
{
    private LeniaSimulation simulation;
    private Panel controlPanel;
    private ScrollContainer scrollContainer;
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
        GetViewport().SizeChanged += OnViewportSizeChanged;
    }
    
    private void OnViewportSizeChanged()
    {
        if (scrollContainer != null)
        {
            scrollContainer.Size = new Vector2(380, GetViewport().GetVisibleRect().Size.Y - 20);
        }
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
        
        // Create scroll container to handle overflow
        scrollContainer = new ScrollContainer();
        scrollContainer.Position = new Vector2(10, 10);
        scrollContainer.Size = new Vector2(380, GetViewport().GetVisibleRect().Size.Y - 20);
        scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.Auto;
        controlPanel.AddChild(scrollContainer);
        
        container = new VBoxContainer();
        container.CustomMinimumSize = new Vector2(360, 0);
        container.AddThemeConstantOverride("separation", 12);
        scrollContainer.AddChild(container);
        
        var titleLabel = new Label();
        titleLabel.Text = "LENIA CONTROLS";
        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        titleLabel.AddThemeFontSizeOverride("font_size", 20);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(titleLabel);
        
        container.AddChild(CreateSeparator());
        
        // Parameters Section
        var parametersSection = new CollapsibleSection("PARAMETERS", true);
        CreateSliderInSection(parametersSection.Content, "Delta Time", 0.01f, 0.5f, simulation.DeltaTime, 
            value => simulation.DeltaTime = value);
        CreateSliderInSection(parametersSection.Content, "Kernel Radius", 5.0f, 30.0f, simulation.KernelRadius, 
            value => {
                simulation.KernelRadius = value;
                simulation.CreateKernel();
            });
        CreateSliderInSection(parametersSection.Content, "Growth Mean", 0.0f, 0.3f, simulation.GrowthMean, 
            value => simulation.GrowthMean = value);
        CreateSliderInSection(parametersSection.Content, "Growth Sigma", 0.001f, 0.1f, simulation.GrowthSigma, 
            value => simulation.GrowthSigma = value);
        container.AddChild(parametersSection);
        
        // Visuals Section
        var visualsSection = new CollapsibleSection("VISUALS", true);
        CreateColorSchemeSelectorInSection(visualsSection.Content);
        CreatePerformanceSelectorInSection(visualsSection.Content);
        container.AddChild(visualsSection);
        
        // Interaction Section
        var interactionSection = new CollapsibleSection("INTERACTION", true);
        CreateSliderInSection(interactionSection.Content, "Brush Size", 1.0f, 10.0f, simulation.BrushSize, 
            value => simulation.BrushSize = value);
        CreateSliderInSection(interactionSection.Content, "Brush Intensity", 0.1f, 3.0f, simulation.BrushIntensity, 
            value => simulation.BrushIntensity = value);
        CreateSliderInSection(interactionSection.Content, "Simulation Speed", 0.0f, 3.0f, simulation.SimulationSpeed, 
            value => simulation.SimulationSpeed = value);
        
        var instructionText = new RichTextLabel();
        instructionText.CustomMinimumSize = new Vector2(320, 50);
        instructionText.BbcodeEnabled = true;
        instructionText.AddThemeFontSizeOverride("normal_font_size", 10);
        instructionText.Text = "[center][color=#9999ff][b]Mouse:[/b][/color] [color=#aaaacc]Left: Paint • Right: Erase[/color][/center]";
        interactionSection.Content.AddChild(instructionText);
        container.AddChild(interactionSection);
        
        // Patterns Section
        var patternsSection = new CollapsibleSection("PATTERNS", false);
        var resetButton = CreateStyledButton("Reset Pattern");
        resetButton.Pressed += () => simulation.InitializePattern();
        patternsSection.Content.AddChild(resetButton);
        
        var randomButton = CreateStyledButton("Random Pattern");
        randomButton.Pressed += () => simulation.RandomPattern();
        patternsSection.Content.AddChild(randomButton);
        
        var orbiumButton = CreateStyledButton("Orbium Pattern");
        orbiumButton.Pressed += () => simulation.OrbiumPattern();
        patternsSection.Content.AddChild(orbiumButton);
        container.AddChild(patternsSection);
        
        // Controls Section
        var controlsSection = new CollapsibleSection("CONTROLS", true);
        pauseButton = CreateStyledButton("Pause");
        pauseButton.Pressed += OnPausePressed;
        controlsSection.Content.AddChild(pauseButton);
        
        menuButton = CreateStyledButton("Back to Menu");
        menuButton.Pressed += OnMenuPressed;
        controlsSection.Content.AddChild(menuButton);
        container.AddChild(controlsSection);
        
        // Statistics Section
        var statsSection = new CollapsibleSection("STATISTICS", false);
        populationGraph = new PopulationGraph();
        statsSection.Content.AddChild(populationGraph);
        
        fpsLabel = new Label();
        fpsLabel.Text = "FPS: 0";
        fpsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        fpsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statsSection.Content.AddChild(fpsLabel);
        container.AddChild(statsSection);
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
        button.CustomMinimumSize = new Vector2(340, 38);
        button.AddThemeFontSizeOverride("font_size", 14);
        
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
    
    private void CreateSliderInSection(VBoxContainer sectionContainer, string name, float min, float max, float value, 
        System.Action<float> onChanged)
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 5);
        sectionContainer.AddChild(vbox);
        
        var label = new Label();
        label.Text = name;
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 13);
        vbox.AddChild(label);
        
        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);
        
        var slider = new HSlider();
        slider.MinValue = min;
        slider.MaxValue = max;
        slider.Value = value;
        slider.Step = (max - min) / 100.0f;
        slider.CustomMinimumSize = new Vector2(200, 25);
        hbox.AddChild(slider);
        
        var valueLabel = new Label();
        valueLabel.Text = value.ToString("F3");
        valueLabel.CustomMinimumSize = new Vector2(60, 0);
        valueLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        valueLabel.AddThemeFontSizeOverride("font_size", 12);
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
    
    private void CreateColorSchemeSelectorInSection(VBoxContainer sectionContainer)
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 5);
        sectionContainer.AddChild(vbox);
        
        var label = new Label();
        label.Text = "Color Scheme";
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 13);
        vbox.AddChild(label);
        
        var optionButton = new OptionButton();
        optionButton.CustomMinimumSize = new Vector2(300, 30);
        optionButton.AddThemeFontSizeOverride("font_size", 12);
        
        var schemes = System.Enum.GetValues<ColorMapper.ColorScheme>();
        foreach (var scheme in schemes)
        {
            optionButton.AddItem(ColorMapper.GetSchemeName(scheme));
        }
        
        optionButton.Selected = (int)simulation.CurrentColorScheme;
        optionButton.ItemSelected += (long index) => {
            simulation.CurrentColorScheme = (ColorMapper.ColorScheme)index;
        };
        
        vbox.AddChild(optionButton);
    }
    
    private void CreatePerformanceSelectorInSection(VBoxContainer sectionContainer)
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 5);
        sectionContainer.AddChild(vbox);
        
        var label = new Label();
        label.Text = "Performance Mode";
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 13);
        vbox.AddChild(label);
        
        var optionButton = new OptionButton();
        optionButton.CustomMinimumSize = new Vector2(300, 30);
        optionButton.AddThemeFontSizeOverride("font_size", 12);
        
        optionButton.AddItem("Fast (128x128)");
        optionButton.AddItem("Balanced (192x192)");
        optionButton.AddItem("Quality (256x256)");
        
        optionButton.Selected = 0;
        optionButton.ItemSelected += (long index) => {
            switch (index)
            {
                case 0:
                    simulation.GridWidth = 128;
                    simulation.GridHeight = 128;
                    break;
                case 1:
                    simulation.GridWidth = 192;
                    simulation.GridHeight = 192;
                    break;
                case 2:
                    simulation.GridWidth = 256;
                    simulation.GridHeight = 256;
                    break;
            }
            simulation._Ready();
        };
        
        vbox.AddChild(optionButton);
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