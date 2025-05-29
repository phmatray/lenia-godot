using Godot;

public partial class RightSidebar : Panel
{
    private LeniaSimulation simulation;
    private ScrollContainer scrollContainer;
    private VBoxContainer container;
    private PopulationGraph populationGraph;
    
    public RightSidebar()
    {
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
        SetupSidebar();
    }
    
    public override void _Ready()
    {
        // Setup is done in Initialize() after simulation reference is set
    }
    
    private void SetupSidebar()
    {
        var sidebarStyle = new StyleBoxFlat();
        sidebarStyle.BgColor = new Color(0.1f, 0.12f, 0.2f, 0.95f);
        sidebarStyle.BorderWidthLeft = 2;
        sidebarStyle.BorderColor = new Color(0.2f, 0.4f, 0.8f, 0.4f);
        AddThemeStyleboxOverride("panel", sidebarStyle);
        
        scrollContainer = new ScrollContainer();
        scrollContainer.AnchorLeft = 0;
        scrollContainer.AnchorTop = 0;
        scrollContainer.AnchorRight = 1;
        scrollContainer.AnchorBottom = 1;
        scrollContainer.OffsetLeft = 10;
        scrollContainer.OffsetTop = 10;
        scrollContainer.OffsetRight = -10;
        scrollContainer.OffsetBottom = -10;
        scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.Auto;
        AddChild(scrollContainer);
        
        container = new VBoxContainer();
        container.CustomMinimumSize = new Vector2(260, 0);
        container.AddThemeConstantOverride("separation", 15);
        scrollContainer.AddChild(container);
        
        CreateParametersSection();
        CreateVisualsSection();
        CreatePatternsSection();
        CreateStatisticsSection();
        
        GetViewport().SizeChanged += OnViewportSizeChanged;
    }
    
    private void OnViewportSizeChanged()
    {
        // No longer needed - anchors handle resizing automatically
    }
    
    private void CreateParametersSection()
    {
        var section = new CollapsibleSection("PARAMETERS", true);
        
        CreateSliderInSection(section.Content, "Delta Time", 0.01f, 0.5f, simulation.DeltaTime, 
            value => simulation.DeltaTime = value);
            
        CreateSliderInSection(section.Content, "Kernel Radius", 5.0f, 30.0f, simulation.KernelRadius, 
            value => {
                simulation.KernelRadius = value;
                simulation.CreateKernel();
            });
            
        CreateSliderInSection(section.Content, "Growth Mean", 0.0f, 0.3f, simulation.GrowthMean, 
            value => simulation.GrowthMean = value);
            
        CreateSliderInSection(section.Content, "Growth Sigma", 0.001f, 0.1f, simulation.GrowthSigma, 
            value => simulation.GrowthSigma = value);
            
        CreateSliderInSection(section.Content, "Brush Size", 1.0f, 10.0f, simulation.BrushSize, 
            value => simulation.BrushSize = value);
            
        CreateSliderInSection(section.Content, "Brush Intensity", 0.1f, 3.0f, simulation.BrushIntensity, 
            value => simulation.BrushIntensity = value);
            
        container.AddChild(section);
    }
    
    private void CreateVisualsSection()
    {
        var section = new CollapsibleSection("VISUALS", true);
        
        CreateColorSchemeSelectorInSection(section.Content);
        CreatePerformanceSelectorInSection(section.Content);
        
        container.AddChild(section);
    }
    
    private void CreatePatternsSection()
    {
        var section = new CollapsibleSection("PATTERNS", false);
        
        var resetButton = CreateStyledButton("Reset Pattern");
        resetButton.Pressed += () => simulation.InitializePattern();
        section.Content.AddChild(resetButton);
        
        var randomButton = CreateStyledButton("Random Pattern");
        randomButton.Pressed += () => simulation.RandomPattern();
        section.Content.AddChild(randomButton);
        
        var orbiumButton = CreateStyledButton("Orbium Pattern");
        orbiumButton.Pressed += () => simulation.OrbiumPattern();
        section.Content.AddChild(orbiumButton);
        
        container.AddChild(section);
    }
    
    private void CreateStatisticsSection()
    {
        var section = new CollapsibleSection("STATISTICS", true);
        
        populationGraph = new PopulationGraph();
        section.Content.AddChild(populationGraph);
        
        var statsGrid = new GridContainer();
        statsGrid.Columns = 2;
        statsGrid.AddThemeConstantOverride("h_separation", 10);
        statsGrid.AddThemeConstantOverride("v_separation", 5);
        section.Content.AddChild(statsGrid);
        
        AddStatLabel(statsGrid, "Grid Size:", $"{simulation.GridWidth}x{simulation.GridHeight}");
        AddStatLabel(statsGrid, "Total Cells:", $"{simulation.GridWidth * simulation.GridHeight:N0}");
        
        container.AddChild(section);
    }
    
    private void AddStatLabel(GridContainer grid, string label, string value)
    {
        var labelNode = new Label();
        labelNode.Text = label;
        labelNode.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
        labelNode.AddThemeFontSizeOverride("font_size", 12);
        grid.AddChild(labelNode);
        
        var valueNode = new Label();
        valueNode.Text = value;
        valueNode.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        valueNode.AddThemeFontSizeOverride("font_size", 12);
        grid.AddChild(valueNode);
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
        valueLabel.CustomMinimumSize = new Vector2(50, 0);
        valueLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        valueLabel.AddThemeFontSizeOverride("font_size", 11);
        hbox.AddChild(valueLabel);
        
        slider.ValueChanged += (double newValue) => {
            onChanged((float)newValue);
            valueLabel.Text = newValue.ToString("F3");
        };
    }
    
    private void CreateColorSchemeSelectorInSection(VBoxContainer sectionContainer)
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 5);
        sectionContainer.AddChild(vbox);
        
        var label = new Label();
        label.Text = "Color Scheme";
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 12);
        vbox.AddChild(label);
        
        var optionButton = new OptionButton();
        optionButton.CustomMinimumSize = new Vector2(240, 28);
        optionButton.AddThemeFontSizeOverride("font_size", 11);
        
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
        label.AddThemeFontSizeOverride("font_size", 12);
        vbox.AddChild(label);
        
        var optionButton = new OptionButton();
        optionButton.CustomMinimumSize = new Vector2(240, 28);
        optionButton.AddThemeFontSizeOverride("font_size", 11);
        
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
    
    private Button CreateStyledButton(string text)
    {
        var button = new Button();
        button.Text = text;
        button.CustomMinimumSize = new Vector2(240, 32);
        button.AddThemeFontSizeOverride("font_size", 12);
        
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.15f, 0.2f, 0.35f);
        buttonStyle.BorderWidthTop = 1;
        buttonStyle.BorderWidthBottom = 1;
        buttonStyle.BorderWidthLeft = 1;
        buttonStyle.BorderWidthRight = 1;
        buttonStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f);
        buttonStyle.CornerRadiusTopLeft = 6;
        buttonStyle.CornerRadiusTopRight = 6;
        buttonStyle.CornerRadiusBottomLeft = 6;
        buttonStyle.CornerRadiusBottomRight = 6;
        button.AddThemeStyleboxOverride("normal", buttonStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.2f, 0.3f, 0.5f);
        hoverStyle.BorderWidthTop = 1;
        hoverStyle.BorderWidthBottom = 1;
        hoverStyle.BorderWidthLeft = 1;
        hoverStyle.BorderWidthRight = 1;
        hoverStyle.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        hoverStyle.CornerRadiusTopLeft = 6;
        hoverStyle.CornerRadiusTopRight = 6;
        hoverStyle.CornerRadiusBottomLeft = 6;
        hoverStyle.CornerRadiusBottomRight = 6;
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        
        return button;
    }
    
    public override void _Process(double delta)
    {
        if (Engine.GetProcessFrames() % 10 == 0)
        {
            populationGraph?.UpdatePopulation(simulation.GetCurrentGrid(), simulation.GridWidth, simulation.GridHeight);
        }
    }
}