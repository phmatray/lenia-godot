using Godot;

public partial class RightSidebar : Panel
{
    private LeniaSimulation simulation;
    private PopulationGraph populationGraph;
    
    // Section references
    private VBoxContainer parametersContent;
    private VBoxContainer visualsContent;
    private VBoxContainer interactionContent;
    private VBoxContainer patternsContent;
    private VBoxContainer funFeaturesContent;
    private VBoxContainer statisticsContent;
    
    // Section headers
    private Button parametersHeader;
    private Button visualsHeader;
    private Button interactionHeader;
    private Button patternsHeader;
    private Button funFeaturesHeader;
    private Button statisticsHeader;
    
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
        var basePath = "MarginContainer/ScrollContainer/VBoxContainer";
        
        parametersHeader = GetNode<Button>($"{basePath}/ParametersSection/Header");
        parametersContent = GetNode<VBoxContainer>($"{basePath}/ParametersSection/Content");
        
        visualsHeader = GetNode<Button>($"{basePath}/VisualsSection/Header");
        visualsContent = GetNode<VBoxContainer>($"{basePath}/VisualsSection/Content");
        
        interactionHeader = GetNode<Button>($"{basePath}/InteractionSection/Header");
        interactionContent = GetNode<VBoxContainer>($"{basePath}/InteractionSection/Content");
        
        patternsHeader = GetNode<Button>($"{basePath}/PatternsSection/Header");
        patternsContent = GetNode<VBoxContainer>($"{basePath}/PatternsSection/Content");
        
        statisticsHeader = GetNode<Button>($"{basePath}/StatisticsSection/Header");
        statisticsContent = GetNode<VBoxContainer>($"{basePath}/StatisticsSection/Content");
        
        // Style section headers
        StyleSectionHeader(parametersHeader);
        StyleSectionHeader(visualsHeader);
        StyleSectionHeader(interactionHeader);
        StyleSectionHeader(patternsHeader);
        StyleSectionHeader(statisticsHeader);
        
        // Connect header toggles
        parametersHeader.Pressed += () => ToggleSection(parametersHeader, parametersContent);
        visualsHeader.Pressed += () => ToggleSection(visualsHeader, visualsContent);
        interactionHeader.Pressed += () => ToggleSection(interactionHeader, interactionContent);
        patternsHeader.Pressed += () => ToggleSection(patternsHeader, patternsContent);
        statisticsHeader.Pressed += () => ToggleSection(statisticsHeader, statisticsContent);
    }
    
    private void SetupSidebar()
    {
        // Create organized parameters
        CreateSimulationParameters();
        CreateVisualSettings();
        CreateInteractionSettings();
        CreatePatternsSection();
        CreateFunFeaturesSection();
        CreateStatistics();
    }
    
    private void CreateSimulationParameters()
    {
        CreateParameterControl(parametersContent, "Delta Time", 0.01f, 0.5f, simulation.DeltaTime, 0.1f,
            value => simulation.DeltaTime = value,
            "Controls the simulation time step");
            
        CreateParameterControl(parametersContent, "Kernel Radius", 5.0f, 30.0f, simulation.KernelRadius, 13.0f,
            value => {
                simulation.KernelRadius = value;
                simulation.CreateKernel();
            },
            "Size of the convolution kernel");
            
        CreateParameterControl(parametersContent, "Growth Mean", 0.0f, 0.3f, simulation.GrowthMean, 0.15f,
            value => simulation.GrowthMean = value,
            "Center of the growth function");
            
        CreateParameterControl(parametersContent, "Growth Sigma", 0.001f, 0.1f, simulation.GrowthSigma, 0.015f,
            value => simulation.GrowthSigma = value,
            "Width of the growth function");
    }
    
    private void CreateVisualSettings()
    {
        CreateColorSchemeSelector(visualsContent);
        CreatePerformanceSelector(visualsContent);
    }
    
    private void CreateInteractionSettings()
    {
        CreateParameterControl(interactionContent, "Brush Size", 1.0f, 10.0f, simulation.BrushSize, 3.0f,
            value => simulation.BrushSize = value,
            "Size of the painting brush");
            
        CreateParameterControl(interactionContent, "Brush Intensity", 0.1f, 3.0f, simulation.BrushIntensity, 1.0f,
            value => simulation.BrushIntensity = value,
            "Strength of the painting brush");
            
        // Add interaction instructions
        var instructionPanel = new Panel();
        var instructionStyle = new StyleBoxFlat();
        instructionStyle.BgColor = new Color(0.1f, 0.15f, 0.25f, 0.8f);
        instructionStyle.SetBorderWidthAll(1);
        instructionStyle.BorderColor = new Color(0.2f, 0.3f, 0.5f, 0.5f);
        instructionStyle.SetCornerRadiusAll(4);
        instructionPanel.AddThemeStyleboxOverride("panel", instructionStyle);
        instructionPanel.CustomMinimumSize = new Vector2(0, 60);
        
        var instructionContainer = new MarginContainer();
        instructionContainer.AddThemeConstantOverride("margin_left", 8);
        instructionContainer.AddThemeConstantOverride("margin_top", 6);
        instructionContainer.AddThemeConstantOverride("margin_right", 8);
        instructionContainer.AddThemeConstantOverride("margin_bottom", 6);
        instructionPanel.AddChild(instructionContainer);
        
        var instructionText = new RichTextLabel();
        instructionText.BbcodeEnabled = true;
        instructionText.FitContent = true;
        instructionText.AddThemeFontSizeOverride("normal_font_size", 10);
        instructionText.Text = "[center][color=#9999ff][b]Controls:[/b][/color]\n[color=#aaaacc]Left Click: Paint • Right Click: Erase\nSelect tools from left toolbar[/color][/center]";
        instructionContainer.AddChild(instructionText);
        
        interactionContent.AddChild(instructionPanel);
    }
    
    private void CreatePatternsSection()
    {
        // Create pattern grid with visual previews
        var patternGrid = new GridContainer();
        patternGrid.Columns = 2;
        patternGrid.AddThemeConstantOverride("h_separation", 10);
        patternGrid.AddThemeConstantOverride("v_separation", 10);
        patternsContent.AddChild(patternGrid);
        
        // Define patterns with their data
        var patterns = new[]
        {
            new { Name = "Orbium", Action = new System.Action(() => simulation.OrbiumPattern()), Data = GetOrbiumMiniature() },
            new { Name = "Random Dots", Action = new System.Action(() => simulation.RandomPattern()), Data = GetRandomMiniature() },
            new { Name = "Central Blob", Action = new System.Action(() => simulation.InitializePattern()), Data = GetBlobMiniature() },
            new { Name = "Ring", Action = new System.Action(() => CreateRingPattern()), Data = GetRingMiniature() },
            new { Name = "Cross", Action = new System.Action(() => CreateCrossPattern()), Data = GetCrossMiniature() },
            new { Name = "Clear Grid", Action = new System.Action(() => ClearPattern()), Data = GetClearMiniature() }
        };
        
        foreach (var pattern in patterns)
        {
            CreatePatternButton(patternGrid, pattern.Name, pattern.Action, pattern.Data);
        }
    }
    
    private void CreateStatistics()
    {
        populationGraph = new PopulationGraph();
        statisticsContent.AddChild(populationGraph);
        
        var statsPanel = new Panel();
        var statsStyle = new StyleBoxFlat();
        statsStyle.BgColor = new Color(0.1f, 0.12f, 0.18f, 0.8f);
        statsStyle.SetBorderWidthAll(1);
        statsStyle.BorderColor = new Color(0.2f, 0.3f, 0.5f, 0.3f);
        statsStyle.SetCornerRadiusAll(4);
        statsPanel.AddThemeStyleboxOverride("panel", statsStyle);
        statsPanel.CustomMinimumSize = new Vector2(0, 60);
        
        var statsContainer = new MarginContainer();
        statsContainer.AddThemeConstantOverride("margin_left", 8);
        statsContainer.AddThemeConstantOverride("margin_top", 6);
        statsContainer.AddThemeConstantOverride("margin_right", 8);
        statsContainer.AddThemeConstantOverride("margin_bottom", 6);
        statsPanel.AddChild(statsContainer);
        
        var statsGrid = new GridContainer();
        statsGrid.Columns = 2;
        statsGrid.AddThemeConstantOverride("h_separation", 10);
        statsGrid.AddThemeConstantOverride("v_separation", 6);
        statsContainer.AddChild(statsGrid);
        
        AddStatLabel(statsGrid, "Grid Size:", $"{simulation.GridWidth}×{simulation.GridHeight}");
        AddStatLabel(statsGrid, "Total Cells:", $"{simulation.GridWidth * simulation.GridHeight:N0}");
        
        statisticsContent.AddChild(statsPanel);
    }
    
    private void CreateParameterControl(VBoxContainer parent, string name, float min, float max, float value, float defaultValue,
        System.Action<float> onChanged, string tooltip = "")
    {
        var container = new Panel();
        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.12f, 0.15f, 0.22f, 0.6f);
        panelStyle.SetBorderWidthAll(1);
        panelStyle.BorderColor = new Color(0.2f, 0.3f, 0.4f, 0.3f);
        panelStyle.SetCornerRadiusAll(4);
        container.AddThemeStyleboxOverride("panel", panelStyle);
        container.CustomMinimumSize = new Vector2(0, 55);
        if (!string.IsNullOrEmpty(tooltip))
        {
            container.TooltipText = tooltip;
        }
        
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 8);
        margin.AddThemeConstantOverride("margin_top", 6);
        margin.AddThemeConstantOverride("margin_right", 8);
        margin.AddThemeConstantOverride("margin_bottom", 6);
        container.AddChild(margin);
        
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 6);
        margin.AddChild(vbox);
        
        // Label row
        var labelRow = new HBoxContainer();
        vbox.AddChild(labelRow);
        
        var label = new Label();
        label.Text = name;
        label.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.95f));
        label.AddThemeFontSizeOverride("font_size", 12);
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        labelRow.AddChild(label);
        
        var valueLabel = new Label();
        valueLabel.Text = value.ToString("F3");
        valueLabel.CustomMinimumSize = new Vector2(50, 0);
        valueLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 0.9f));
        valueLabel.AddThemeFontSizeOverride("font_size", 11);
        valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
        labelRow.AddChild(valueLabel);
        
        // Slider row
        var sliderRow = new HBoxContainer();
        vbox.AddChild(sliderRow);
        
        var slider = new HSlider();
        slider.MinValue = min;
        slider.MaxValue = max;
        slider.Value = value;
        slider.Step = (max - min) / 100.0f;
        slider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        sliderRow.AddChild(slider);
        
        var resetButton = new Button();
        resetButton.Text = "↺";
        resetButton.CustomMinimumSize = new Vector2(25, 20);
        resetButton.AddThemeFontSizeOverride("font_size", 10);
        resetButton.Flat = true;
        resetButton.TooltipText = $"Reset to {defaultValue:F3}";
        sliderRow.AddChild(resetButton);
        
        // Connect signals
        slider.ValueChanged += (double newValue) => {
            onChanged((float)newValue);
            valueLabel.Text = newValue.ToString("F3");
        };
        
        resetButton.Pressed += () => {
            slider.Value = defaultValue;
        };
        
        parent.AddChild(container);
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
        header.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.15f, 0.2f, 0.3f, 0.6f);
        hoverStyle.SetCornerRadiusAll(4);
        header.AddThemeStyleboxOverride("hover", hoverStyle);
    }
    
    private void CreateColorSchemeSelector(VBoxContainer parent)
    {
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 8);
        parent.AddChild(container);
        
        var label = new Label();
        label.Text = "Color Scheme";
        label.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.95f));
        label.AddThemeFontSizeOverride("font_size", 12);
        container.AddChild(label);
        
        var optionButton = new OptionButton();
        optionButton.CustomMinimumSize = new Vector2(0, 28);
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
        container.AddThemeConstantOverride("separation", 8);
        parent.AddChild(container);
        
        var label = new Label();
        label.Text = "Performance Mode";
        label.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.95f));
        label.AddThemeFontSizeOverride("font_size", 12);
        container.AddChild(label);
        
        var optionButton = new OptionButton();
        optionButton.CustomMinimumSize = new Vector2(0, 28);
        optionButton.AddThemeFontSizeOverride("font_size", 11);
        
        optionButton.AddItem("Fast (128×128)");
        optionButton.AddItem("Balanced (192×192)");
        optionButton.AddItem("Quality (256×256)");
        
        optionButton.Selected = 0;
        optionButton.ItemSelected += (long index) => {
            switch (index)
            {
                case 0:
                    simulation.ResizeGrid(128, 128);
                    break;
                case 1:
                    simulation.ResizeGrid(192, 192);
                    break;
                case 2:
                    simulation.ResizeGrid(256, 256);
                    break;
            }
        };
        
        container.AddChild(optionButton);
    }
    
    private void AddStatLabel(GridContainer grid, string label, string value)
    {
        var labelNode = new Label();
        labelNode.Text = label;
        labelNode.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
        labelNode.AddThemeFontSizeOverride("font_size", 11);
        grid.AddChild(labelNode);
        
        var valueNode = new Label();
        valueNode.Text = value;
        valueNode.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        valueNode.AddThemeFontSizeOverride("font_size", 11);
        grid.AddChild(valueNode);
    }
    
    private void ClearPattern()
    {
        var grid = simulation.GetCurrentGrid();
        for (int x = 0; x < simulation.GridWidth; x++)
        {
            for (int y = 0; y < simulation.GridHeight; y++)
            {
                grid[x, y] = 0.0f;
            }
        }
    }
    
    public override void _Process(double delta)
    {
        if (Engine.GetProcessFrames() % 10 == 0 && populationGraph != null && simulation != null)
        {
            populationGraph.UpdatePopulation(simulation.GetCurrentGrid(), simulation.GridWidth, simulation.GridHeight);
        }
    }
    
    private void CreatePatternButton(GridContainer parent, string name, System.Action action, float[,] miniatureData)
    {
        var button = new Panel();
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.12f, 0.15f, 0.22f, 0.8f);
        buttonStyle.SetBorderWidthAll(2);
        buttonStyle.BorderColor = new Color(0.2f, 0.3f, 0.5f, 0.4f);
        buttonStyle.SetCornerRadiusAll(6);
        button.AddThemeStyleboxOverride("panel", buttonStyle);
        button.CustomMinimumSize = new Vector2(120, 80);
        parent.AddChild(button);
        
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 6);
        button.AddChild(container);
        
        // Create miniature preview
        var preview = new PatternPreview();
        preview.SetPattern(miniatureData);
        preview.CustomMinimumSize = new Vector2(110, 50);
        container.AddChild(preview);
        
        // Create button label
        var label = new Button();
        label.Text = name;
        label.AddThemeFontSizeOverride("font_size", 10);
        label.Flat = true;
        label.AddThemeColorOverride("font_color", new Color(0.85f, 0.9f, 1.0f));
        label.CustomMinimumSize = new Vector2(0, 24);
        container.AddChild(label);
        
        // Connect click action
        label.Pressed += () => action();
        
        // Add hover effects
        button.MouseEntered += () => {
            buttonStyle.BorderColor = new Color(0.4f, 0.6f, 0.9f, 0.8f);
            button.AddThemeStyleboxOverride("panel", buttonStyle);
        };
        button.MouseExited += () => {
            buttonStyle.BorderColor = new Color(0.2f, 0.3f, 0.5f, 0.4f);
            button.AddThemeStyleboxOverride("panel", buttonStyle);
        };
    }
    
    private void CreateRingPattern()
    {
        var grid = simulation.GetCurrentGrid();
        var centerX = simulation.GridWidth / 2;
        var centerY = simulation.GridHeight / 2;
        var outerRadius = 25;
        var innerRadius = 15;
        
        for (int x = 0; x < simulation.GridWidth; x++)
        {
            for (int y = 0; y < simulation.GridHeight; y++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distance >= innerRadius && distance <= outerRadius)
                {
                    var intensity = 1.0f - Mathf.Abs(distance - (innerRadius + outerRadius) / 2) / ((outerRadius - innerRadius) / 2);
                    grid[x, y] = intensity * 0.8f;
                }
                else
                {
                    grid[x, y] = 0.0f;
                }
            }
        }
    }
    
    private void CreateCrossPattern()
    {
        var grid = simulation.GetCurrentGrid();
        var centerX = simulation.GridWidth / 2;
        var centerY = simulation.GridHeight / 2;
        var thickness = 8;
        var length = 30;
        
        for (int x = 0; x < simulation.GridWidth; x++)
        {
            for (int y = 0; y < simulation.GridHeight; y++)
            {
                grid[x, y] = 0.0f;
                
                // Horizontal bar
                if (Mathf.Abs(y - centerY) <= thickness && Mathf.Abs(x - centerX) <= length)
                {
                    var intensity = 1.0f - Mathf.Abs(y - centerY) / (float)thickness;
                    grid[x, y] = intensity * 0.7f;
                }
                
                // Vertical bar
                if (Mathf.Abs(x - centerX) <= thickness && Mathf.Abs(y - centerY) <= length)
                {
                    var intensity = 1.0f - Mathf.Abs(x - centerX) / (float)thickness;
                    grid[x, y] = Mathf.Max(grid[x, y], intensity * 0.7f);
                }
            }
        }
    }
    
    private float[,] GetOrbiumMiniature()
    {
        var mini = new float[16, 16];
        var orbium = new float[,] {
            {0.0f,0.0f,0.0f,0.0f,0.1f,0.2f,0.2f,0.1f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f},
            {0.0f,0.0f,0.1f,0.3f,0.5f,0.7f,0.7f,0.5f,0.3f,0.1f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f},
            {0.0f,0.1f,0.3f,0.6f,0.8f,0.9f,0.9f,0.8f,0.6f,0.3f,0.1f,0.0f,0.0f,0.0f,0.0f,0.0f},
            {0.0f,0.2f,0.5f,0.8f,0.9f,1.0f,1.0f,0.9f,0.8f,0.5f,0.2f,0.0f,0.0f,0.0f,0.0f,0.0f},
            {0.1f,0.3f,0.6f,0.9f,1.0f,1.0f,1.0f,1.0f,0.9f,0.6f,0.3f,0.1f,0.0f,0.0f,0.0f,0.0f},
            {0.0f,0.2f,0.5f,0.8f,0.9f,1.0f,1.0f,0.9f,0.8f,0.5f,0.2f,0.0f,0.0f,0.0f,0.0f,0.0f},
            {0.0f,0.1f,0.3f,0.6f,0.8f,0.9f,0.9f,0.8f,0.6f,0.3f,0.1f,0.0f,0.0f,0.0f,0.0f,0.0f},
            {0.0f,0.0f,0.1f,0.3f,0.5f,0.7f,0.7f,0.5f,0.3f,0.1f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f},
            {0.0f,0.0f,0.0f,0.0f,0.1f,0.2f,0.2f,0.1f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f}
        };
        
        for (int x = 0; x < 9 && x < 16; x++)
        {
            for (int y = 0; y < 16 && y < 16; y++)
            {
                mini[x + 3, y] = orbium[x, y];
            }
        }
        return mini;
    }
    
    private float[,] GetRandomMiniature()
    {
        var mini = new float[16, 16];
        var random = new System.Random(42); // Fixed seed for consistent preview
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                mini[x, y] = random.NextDouble() < 0.15 ? (float)random.NextDouble() * 0.8f : 0.0f;
            }
        }
        return mini;
    }
    
    private float[,] GetBlobMiniature()
    {
        var mini = new float[16, 16];
        var centerX = 8;
        var centerY = 8;
        var radius = 5;
        
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance <= radius)
                {
                    var intensity = 1.0f - (distance / radius);
                    mini[x, y] = intensity * 0.8f;
                }
            }
        }
        return mini;
    }
    
    private float[,] GetRingMiniature()
    {
        var mini = new float[16, 16];
        var centerX = 8;
        var centerY = 8;
        var outerRadius = 6;
        var innerRadius = 3;
        
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distance >= innerRadius && distance <= outerRadius)
                {
                    var intensity = 1.0f - Mathf.Abs(distance - (innerRadius + outerRadius) / 2) / ((outerRadius - innerRadius) / 2);
                    mini[x, y] = intensity * 0.8f;
                }
            }
        }
        return mini;
    }
    
    private float[,] GetCrossMiniature()
    {
        var mini = new float[16, 16];
        var centerX = 8;
        var centerY = 8;
        var thickness = 2;
        var length = 6;
        
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                // Horizontal bar
                if (Mathf.Abs(y - centerY) <= thickness && Mathf.Abs(x - centerX) <= length)
                {
                    var intensity = 1.0f - Mathf.Abs(y - centerY) / (float)thickness;
                    mini[x, y] = intensity * 0.7f;
                }
                
                // Vertical bar
                if (Mathf.Abs(x - centerX) <= thickness && Mathf.Abs(y - centerY) <= length)
                {
                    var intensity = 1.0f - Mathf.Abs(x - centerX) / (float)thickness;
                    mini[x, y] = Mathf.Max(mini[x, y], intensity * 0.7f);
                }
            }
        }
        return mini;
    }
    
    private float[,] GetClearMiniature()
    {
        var mini = new float[16, 16];
        // Add a subtle "X" pattern to indicate clear
        for (int i = 0; i < 16; i++)
        {
            mini[i, i] = 0.3f;
            mini[i, 15 - i] = 0.3f;
        }
        return mini;
    }
    
    private void CreateFunFeaturesSection()
    {
        // Since this section doesn't exist in the scene, create it programmatically
        var mainContainer = GetNode<VBoxContainer>("MarginContainer/ScrollContainer/VBoxContainer");
        
        // Create the section container
        var sectionContainer = new VBoxContainer();
        sectionContainer.Name = "FunFeaturesSection";
        
        // Create header
        funFeaturesHeader = new Button();
        funFeaturesHeader.Text = "▼ Fun Features";
        funFeaturesHeader.Flat = true;
        funFeaturesHeader.Alignment = HorizontalAlignment.Left;
        StyleSectionHeader(funFeaturesHeader);
        sectionContainer.AddChild(funFeaturesHeader);
        
        // Create content
        funFeaturesContent = new VBoxContainer();
        funFeaturesContent.AddThemeConstantOverride("separation", 8);
        sectionContainer.AddChild(funFeaturesContent);
        
        // Connect header toggle
        funFeaturesHeader.Pressed += () => ToggleSection(funFeaturesHeader, funFeaturesContent);
        
        // Insert before statistics section
        var statisticsIndex = mainContainer.GetChildCount() - 1; // Assuming statistics is last
        mainContainer.AddChild(sectionContainer);
        mainContainer.MoveChild(sectionContainer, statisticsIndex);
        
        // Add fun features controls
        CreateAudioControls();
        CreateParticleControls();
        CreateRecordingControls();
        CreateQuickAccessButtons();
    }
    
    private void CreateAudioControls()
    {
        var audioPanel = new Panel();
        var audioStyle = new StyleBoxFlat();
        audioStyle.BgColor = new Color(0.12f, 0.15f, 0.22f, 0.6f);
        audioStyle.SetBorderWidthAll(1);
        audioStyle.BorderColor = new Color(0.2f, 0.3f, 0.4f, 0.3f);
        audioStyle.SetCornerRadiusAll(4);
        audioPanel.AddThemeStyleboxOverride("panel", audioStyle);
        audioPanel.CustomMinimumSize = new Vector2(0, 80);
        
        var audioContainer = new VBoxContainer();
        audioContainer.AddThemeConstantOverride("separation", 8);
        audioPanel.AddChild(audioContainer);
        
        var audioLabel = new Label();
        audioLabel.Text = "🔊 Audio Settings";
        audioLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.95f));
        audioLabel.AddThemeFontSizeOverride("font_size", 12);
        audioContainer.AddChild(audioLabel);
        
        CreateParameterControl(audioContainer, "Audio Volume", 0.0f, 1.0f, 0.5f, 0.5f,
            value => GetMainUI()?.SetAudioVolume(value),
            "Master volume for audio feedback");
        
        funFeaturesContent.AddChild(audioPanel);
    }
    
    private void CreateParticleControls()
    {
        var particlePanel = new Panel();
        var particleStyle = new StyleBoxFlat();
        particleStyle.BgColor = new Color(0.12f, 0.15f, 0.22f, 0.6f);
        particleStyle.SetBorderWidthAll(1);
        particleStyle.BorderColor = new Color(0.2f, 0.3f, 0.4f, 0.3f);
        particleStyle.SetCornerRadiusAll(4);
        particlePanel.AddThemeStyleboxOverride("panel", particleStyle);
        particlePanel.CustomMinimumSize = new Vector2(0, 80);
        
        var particleContainer = new VBoxContainer();
        particleContainer.AddThemeConstantOverride("separation", 8);
        particlePanel.AddChild(particleContainer);
        
        var particleLabel = new Label();
        particleLabel.Text = "✨ Particle Effects";
        particleLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.95f));
        particleLabel.AddThemeFontSizeOverride("font_size", 12);
        particleContainer.AddChild(particleLabel);
        
        CreateParameterControl(particleContainer, "Particle Intensity", 0.0f, 2.0f, 0.7f, 0.7f,
            value => GetMainUI()?.SetParticleIntensity(value),
            "Number and brightness of particles");
        
        funFeaturesContent.AddChild(particlePanel);
    }
    
    private void CreateRecordingControls()
    {
        var recordPanel = new Panel();
        var recordStyle = new StyleBoxFlat();
        recordStyle.BgColor = new Color(0.12f, 0.15f, 0.22f, 0.6f);
        recordStyle.SetBorderWidthAll(1);
        recordStyle.BorderColor = new Color(0.2f, 0.3f, 0.4f, 0.3f);
        recordStyle.SetCornerRadiusAll(4);
        recordPanel.AddThemeStyleboxOverride("panel", recordStyle);
        recordPanel.CustomMinimumSize = new Vector2(0, 60);
        
        var recordContainer = new VBoxContainer();
        recordContainer.AddThemeConstantOverride("separation", 8);
        recordPanel.AddChild(recordContainer);
        
        var recordLabel = new Label();
        recordLabel.Text = "🎬 Time-lapse Recording";
        recordLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.95f));
        recordLabel.AddThemeFontSizeOverride("font_size", 12);
        recordContainer.AddChild(recordLabel);
        
        var infoLabel = new Label();
        infoLabel.Text = "Use the record button in the header bar to start/stop recording.";
        infoLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 0.9f));
        infoLabel.AddThemeFontSizeOverride("font_size", 10);
        infoLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        recordContainer.AddChild(infoLabel);
        
        funFeaturesContent.AddChild(recordPanel);
    }
    
    private void CreateQuickAccessButtons()
    {
        var buttonPanel = new Panel();
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.12f, 0.15f, 0.22f, 0.6f);
        buttonStyle.SetBorderWidthAll(1);
        buttonStyle.BorderColor = new Color(0.2f, 0.3f, 0.4f, 0.3f);
        buttonStyle.SetCornerRadiusAll(4);
        buttonPanel.AddThemeStyleboxOverride("panel", buttonStyle);
        buttonPanel.CustomMinimumSize = new Vector2(0, 120);
        
        var buttonContainer = new VBoxContainer();
        buttonContainer.AddThemeConstantOverride("separation", 8);
        buttonPanel.AddChild(buttonContainer);
        
        var buttonLabel = new Label();
        buttonLabel.Text = "🚀 Quick Access";
        buttonLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.95f));
        buttonLabel.AddThemeFontSizeOverride("font_size", 12);
        buttonContainer.AddChild(buttonLabel);
        
        var buttonGrid = new GridContainer();
        buttonGrid.Columns = 2;
        buttonGrid.AddThemeConstantOverride("h_separation", 8);
        buttonGrid.AddThemeConstantOverride("v_separation", 8);
        buttonContainer.AddChild(buttonGrid);
        
        // Pattern Library button
        var patternBtn = new Button();
        patternBtn.Text = "📚 Patterns";
        patternBtn.AddThemeFontSizeOverride("font_size", 10);
        patternBtn.Pressed += () => GetMainUI()?.ShowPatternLibrary();
        buttonGrid.AddChild(patternBtn);
        
        // Tutorials button
        var tutorialBtn = new Button();
        tutorialBtn.Text = "🎓 Tutorials";
        tutorialBtn.AddThemeFontSizeOverride("font_size", 10);
        tutorialBtn.Pressed += () => GetMainUI()?.ShowTutorials();
        buttonGrid.AddChild(tutorialBtn);
        
        // Challenges button
        var challengeBtn = new Button();
        challengeBtn.Text = "🏆 Challenges";
        challengeBtn.AddThemeFontSizeOverride("font_size", 10);
        challengeBtn.Pressed += () => GetMainUI()?.ShowChallenges();
        buttonGrid.AddChild(challengeBtn);
        
        // Clear button for particles
        var clearBtn = new Button();
        clearBtn.Text = "💨 Clear FX";
        clearBtn.AddThemeFontSizeOverride("font_size", 10);
        clearBtn.TooltipText = "Clear all particle effects";
        clearBtn.Pressed += () => {
            // Clear particles - would need access to particle system
            GD.Print("Clearing particle effects");
        };
        buttonGrid.AddChild(clearBtn);
        
        funFeaturesContent.AddChild(buttonPanel);
    }
    
    private LeniaMainUI GetMainUI()
    {
        // Navigate up the tree to find the main UI
        Node current = this;
        while (current != null && !(current is LeniaMainUI))
        {
            current = current.GetParent();
        }
        return current as LeniaMainUI;
    }
}