using Godot;

public partial class LeniaMainUI : Control
{
    private LeniaSimulation simulation;
    private HeaderBar headerBar;
    private LeftToolbar leftToolbar;
    private SimulationCanvas simulationCanvas;
    private RightSidebar rightSidebar;
    private StatusBar statusBar;
    
    public override void _Ready()
    {
        GD.Print("LeniaMainUI _Ready called");
        
        // Get the simulation node
        simulation = GetNode<LeniaSimulation>("/root/Main/LeniaSimulation");
        if (simulation == null)
        {
            GD.PrintErr("Failed to get LeniaSimulation node!");
            return;
        }
        
        GD.Print("Successfully got LeniaSimulation node");
        
        // Get UI components from the scene
        headerBar = GetNode<HeaderBar>("VBoxContainer/HeaderBar");
        leftToolbar = GetNode<LeftToolbar>("VBoxContainer/MiddleSection/LeftToolbar");
        simulationCanvas = GetNode<SimulationCanvas>("VBoxContainer/MiddleSection/SimulationCanvas");
        rightSidebar = GetNode<RightSidebar>("VBoxContainer/MiddleSection/RightSidebar");
        statusBar = GetNode<StatusBar>("VBoxContainer/StatusBar");
        
        // Initialize components with simulation reference
        headerBar.Initialize(simulation);
        simulationCanvas.Initialize(simulation);
        rightSidebar.Initialize(simulation);
        statusBar.Initialize(simulation);
        
        // Connect signals
        leftToolbar.ToolChanged += OnToolChanged;
    }
    
    private void OnToolChanged(LeftToolbar.Tool tool)
    {
        simulationCanvas.SetCurrentTool(tool);
    }
}