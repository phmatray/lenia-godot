using Godot;

public partial class RightSidebar : Panel
{
    private LeniaSimulation simulation;
    private PopulationGraph populationGraph;
    
    // Section references
    private VBoxContainer parametersContent;
    private VBoxContainer visualsContent;
    private VBoxContainer patternsContent;
    private VBoxContainer statisticsContent;
    
    // Section headers
    private Button parametersHeader;
    private Button visualsHeader;
    private Button patternsHeader;
    private Button statisticsHeader;
    
    // Pattern buttons
    private Button resetButton;
    private Button randomButton;
    private Button orbiumButton;
    
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
        // Get references from scene
        parametersHeader = GetNode<Button>("ScrollContainer/VBoxContainer/ParametersSection/Header");
        parametersContent = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer/ParametersSection/Content");
        
        visualsHeader = GetNode<Button>("ScrollContainer/VBoxContainer/VisualsSection/Header");
        visualsContent = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer/VisualsSection/Content");
        
        patternsHeader = GetNode<Button>("ScrollContainer/VBoxContainer/PatternsSection/Header");
        patternsContent = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer/PatternsSection/Content");
        
        statisticsHeader = GetNode<Button>("ScrollContainer/VBoxContainer/StatisticsSection/Header");
        statisticsContent = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer/StatisticsSection/Content");
        
        // Get pattern buttons
        resetButton = GetNode<Button>("ScrollContainer/VBoxContainer/PatternsSection/Content/ResetButton");
        randomButton = GetNode<Button>("ScrollContainer/VBoxContainer/PatternsSection/Content/RandomButton");
        orbiumButton = GetNode<Button>("ScrollContainer/VBoxContainer/PatternsSection/Content/OrbiumButton");
        
        // Style the panel
        var sidebarStyle = new StyleBoxFlat();
        sidebarStyle.BgColor = new Color(0.1f, 0.12f, 0.2f, 0.95f);
        sidebarStyle.BorderWidthLeft = 2;
        sidebarStyle.BorderColor = new Color(0.2f, 0.4f, 0.8f, 0.4f);
        AddThemeStyleboxOverride("panel", sidebarStyle);
        
        // Style section headers
        StyleSectionHeader(parametersHeader);
        StyleSectionHeader(visualsHeader);
        StyleSectionHeader(patternsHeader);
        StyleSectionHeader(statisticsHeader);
        
        // Style buttons
        StyleButton(resetButton);
        StyleButton(randomButton);
        StyleButton(orbiumButton);
        
        // Connect header toggles
        parametersHeader.Pressed += () => ToggleSection(parametersHeader, parametersContent);
        visualsHeader.Pressed += () => ToggleSection(visualsHeader, visualsContent);
        patternsHeader.Pressed += () => ToggleSection(patternsHeader, patternsContent);
        statisticsHeader.Pressed += () => ToggleSection(statisticsHeader, statisticsContent);
    }
    
    private void SetupSidebar()
    {
        // Connect pattern buttons
        resetButton.Pressed += () => simulation.InitializePattern();
        randomButton.Pressed += () => simulation.RandomPattern();
        orbiumButton.Pressed += () => simulation.OrbiumPattern();
        
        // Create parameters
        CreateParameterSlider(parametersContent, "Delta Time", 0.01f, 0.5f, simulation.DeltaTime, 
            value => simulation.DeltaTime = value);
        CreateParameterSlider(parametersContent, "Kernel Radius", 5.0f, 30.0f, simulation.KernelRadius, 
            value => {
                simulation.KernelRadius = value;
                simulation.CreateKernel();
            });
        CreateParameterSlider(parametersContent, "Growth Mean", 0.0f, 0.3f, simulation.GrowthMean, 
            value => simulation.GrowthMean = value);
        CreateParameterSlider(parametersContent, "Growth Sigma", 0.001f, 0.1f, simulation.GrowthSigma, 
            value => simulation.GrowthSigma = value);
        CreateParameterSlider(parametersContent, "Brush Size", 1.0f, 10.0f, simulation.BrushSize, 
            value => simulation.BrushSize = value);
        CreateParameterSlider(parametersContent, "Brush Intensity", 0.1f, 3.0f, simulation.BrushIntensity, 
            value => simulation.BrushIntensity = value);
        
        // Create visuals
        CreateColorSchemeSelector(visualsContent);
        CreatePerformanceSelector(visualsContent);
        
        // Create statistics
        populationGraph = new PopulationGraph();
        statisticsContent.AddChild(populationGraph);
        
        var statsGrid = new GridContainer();
        statsGrid.Columns = 2;
        statsGrid.AddThemeConstantOverride("h_separation", 10);
        statsGrid.AddThemeConstantOverride("v_separation", 5);
        statisticsContent.AddChild(statsGrid);
        
        AddStatLabel(statsGrid, "Grid Size:", $"{simulation.GridWidth}x{simulation.GridHeight}");
        AddStatLabel(statsGrid, "Total Cells:", $"{simulation.GridWidth * simulation.GridHeight:N0}");
    }
    
    private void ToggleSection(Button header, VBoxContainer content)
    {
        content.Visible = !content.Visible;
        var text = header.Text;
        if (text.StartsWith("▼"))
        {
            header.Text = "▶" + text.Substring(1);
        }
        else if (text.StartsWith("▶"))
        {
            header.Text = "▼" + text.Substring(1);
        }
    }
    
    private void StyleSectionHeader(Button header)
    {
        var headerStyle = new StyleBoxFlat();
        headerStyle.BgColor = new Color(0.2f, 0.25f, 0.4f, 0.8f);
        headerStyle.SetBorderWidthAll(1);
        headerStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        headerStyle.SetCornerRadiusAll(6);
        header.AddThemeStyleboxOverride("normal", headerStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.25f, 0.3f, 0.5f, 0.9f);
        hoverStyle.SetBorderWidthAll(1);
        hoverStyle.BorderColor = new Color(0.4f, 0.6f, 0.9f, 0.6f);
        hoverStyle.SetCornerRadiusAll(6);
        header.AddThemeStyleboxOverride("hover", hoverStyle);
        
        header.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
    }
    
    private void StyleButton(Button button)
    {
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.15f, 0.2f, 0.35f);
        buttonStyle.SetBorderWidthAll(1);
        buttonStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f);
        buttonStyle.SetCornerRadiusAll(6);
        button.AddThemeStyleboxOverride("normal", buttonStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.2f, 0.3f, 0.5f);
        hoverStyle.SetBorderWidthAll(1);
        hoverStyle.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        hoverStyle.SetCornerRadiusAll(6);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
    }
    
    private void CreateParameterSlider(VBoxContainer parent, string name, float min, float max, float value, 
        System.Action<float> onChanged)
    {
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 5);
        parent.AddChild(container);
        
        var label = new Label();
        label.Text = name;
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 12);
        container.AddChild(label);
        
        var hbox = new HBoxContainer();
        container.AddChild(hbox);
        
        var slider = new HSlider();
        slider.MinValue = min;
        slider.MaxValue = max;
        slider.Value = value;
        slider.Step = (max - min) / 100.0f;
        slider.CustomMinimumSize = new Vector2(180, 20);
        slider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
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
    
    private void CreateColorSchemeSelector(VBoxContainer parent)
    {
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 5);
        parent.AddChild(container);
        
        var label = new Label();
        label.Text = "Color Scheme";
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 12);
        container.AddChild(label);
        
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
        
        container.AddChild(optionButton);
    }
    
    private void CreatePerformanceSelector(VBoxContainer parent)
    {
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 5);
        parent.AddChild(container);
        
        var label = new Label();
        label.Text = "Performance Mode";
        label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        label.AddThemeFontSizeOverride("font_size", 12);
        container.AddChild(label);
        
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
        
        container.AddChild(optionButton);
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
    
    public override void _Process(double delta)
    {
        if (Engine.GetProcessFrames() % 10 == 0 && populationGraph != null && simulation != null)
        {
            populationGraph.UpdatePopulation(simulation.GetCurrentGrid(), simulation.GridWidth, simulation.GridHeight);
        }
    }
}