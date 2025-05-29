using Godot;

public partial class StatusBar : Panel
{
    private LeniaSimulation simulation;
    private Label fpsLabel;
    private Label cellCountLabel;
    private Label avgDensityLabel;
    private Label peakDensityLabel;
    private Label simSpeedLabel;
    
    public StatusBar()
    {
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
        SetupStatusBar();
    }
    
    public override void _Ready()
    {
        // Get references to scene nodes
        fpsLabel = GetNode<Label>("HBoxContainer/FPSLabel");
        cellCountLabel = GetNode<Label>("HBoxContainer/CellCountLabel");
        avgDensityLabel = GetNode<Label>("HBoxContainer/AvgDensityLabel");
        peakDensityLabel = GetNode<Label>("HBoxContainer/PeakDensityLabel");
        simSpeedLabel = GetNode<Label>("HBoxContainer/SimSpeedLabel");
        
        // Style the panel
        var statusStyle = new StyleBoxFlat();
        statusStyle.BgColor = new Color(0.08f, 0.1f, 0.15f, 0.95f);
        statusStyle.BorderWidthTop = 2;
        statusStyle.BorderColor = new Color(0.2f, 0.4f, 0.8f, 0.4f);
        AddThemeStyleboxOverride("panel", statusStyle);
    }
    
    private void SetupStatusBar()
    {
        // Initial update
        UpdateStatistics();
    }
    
    public override void _Process(double delta)
    {
        UpdateStatistics();
    }
    
    private void UpdateStatistics()
    {
        if (simulation == null) return;
        
        // Update FPS
        var fps = Engine.GetFramesPerSecond();
        fpsLabel.Text = $"FPS: {fps}";
        
        if (fps >= 50)
        {
            fpsLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 0.6f));
        }
        else if (fps >= 30)
        {
            fpsLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.4f));
        }
        else
        {
            fpsLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.4f, 0.4f));
        }
        
        // Update simulation speed
        simSpeedLabel.Text = $"Speed: {simulation.SimulationSpeed:F1}x";
        
        if (simulation.SimulationSpeed == 0)
        {
            simSpeedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.4f, 0.4f));
        }
        else if (simulation.SimulationSpeed > 1.5f)
        {
            simSpeedLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.8f, 0.4f));
        }
        else
        {
            simSpeedLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        }
        
        // Update grid statistics every few frames for performance
        if (Engine.GetProcessFrames() % 5 == 0)
        {
            var grid = simulation.GetCurrentGrid();
            int activeCells = 0;
            float totalDensity = 0;
            float peakDensity = 0;
            
            for (int x = 0; x < simulation.GridWidth; x++)
            {
                for (int y = 0; y < simulation.GridHeight; y++)
                {
                    float value = grid[x, y];
                    if (value > 0.01f)
                    {
                        activeCells++;
                        totalDensity += value;
                        if (value > peakDensity)
                        {
                            peakDensity = value;
                        }
                    }
                }
            }
            
            float avgDensity = activeCells > 0 ? totalDensity / activeCells : 0;
            
            cellCountLabel.Text = $"Active Cells: {activeCells:N0}";
            avgDensityLabel.Text = $"Avg Density: {avgDensity:F3}";
            peakDensityLabel.Text = $"Peak: {peakDensity:F3}";
            
            if (activeCells > simulation.GridWidth * simulation.GridHeight * 0.5f)
            {
                cellCountLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.4f));
            }
            else
            {
                cellCountLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            }
        }
    }
}