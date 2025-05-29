using Godot;

public partial class SimulationCanvas : Control
{
    private LeniaSimulation simulation;
    private LeftToolbar.Tool currentTool = LeftToolbar.Tool.Paint;
    private Sprite2D displaySprite;
    private bool isMouseDown = false;
    private Vector2 lastMousePosition;
    private ToolOverlay toolOverlay;
    
    public SimulationCanvas()
    {
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
        
        // Get the display sprite from simulation and add it as a child
        if (simulation != null)
        {
            displaySprite = simulation.GetDisplaySprite();
            if (displaySprite != null)
            {
                AddChild(displaySprite);
                
                // Create tool overlay that renders on top
                toolOverlay = new ToolOverlay();
                toolOverlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                toolOverlay.Initialize(simulation, displaySprite);
                AddChild(toolOverlay);
                
                // Connect to grid resize signal
                simulation.GridResized += OnGridResized;
                
                // Defer the display update to ensure the layout is settled
                CallDeferred(nameof(UpdateSimulationDisplay));
            }
            else
            {
                GD.PrintErr("SimulationCanvas: Display sprite is null!");
            }
        }
    }
    
    private void SetupCanvas()
    {
        MouseFilter = MouseFilterEnum.Pass;
        
        var canvasStyle = new StyleBoxFlat();
        canvasStyle.BgColor = new Color(0.02f, 0.02f, 0.03f);
        canvasStyle.SetBorderWidthAll(1);
        canvasStyle.BorderColor = new Color(0.2f, 0.3f, 0.5f, 0.3f);
        canvasStyle.SetCornerRadiusAll(4);
        canvasStyle.SetContentMarginAll(5);
        AddThemeStyleboxOverride("panel", canvasStyle);
        
        ClipContents = true;
    }
    
    public void SetCurrentTool(LeftToolbar.Tool tool)
    {
        currentTool = tool;
        if (toolOverlay != null)
        {
            toolOverlay.SetTool(tool);
        }
        
        switch (tool)
        {
            case LeftToolbar.Tool.Paint:
            case LeftToolbar.Tool.Erase:
                MouseDefaultCursorShape = CursorShape.Cross;
                break;
            case LeftToolbar.Tool.Move:
                MouseDefaultCursorShape = CursorShape.Move;
                break;
            case LeftToolbar.Tool.Pattern:
                MouseDefaultCursorShape = CursorShape.PointingHand;
                break;
            default:
                MouseDefaultCursorShape = CursorShape.Arrow;
                break;
        }
    }
    
    public override void _Ready()
    {
        GetViewport().SizeChanged += UpdateSimulationDisplay;
        SetupCanvas();
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                isMouseDown = mouseButton.Pressed;
                if (isMouseDown)
                {
                    lastMousePosition = mouseButton.Position;
                    HandleToolAction(mouseButton.Position);
                }
            }
            else if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                isMouseDown = mouseButton.Pressed;
                if (isMouseDown && currentTool == LeftToolbar.Tool.Paint)
                {
                    HandleErase(mouseButton.Position);
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            if (isMouseDown)
            {
                switch (currentTool)
                {
                    case LeftToolbar.Tool.Paint:
                        if (Input.IsMouseButtonPressed(MouseButton.Left))
                            HandlePaint(mouseMotion.Position);
                        else if (Input.IsMouseButtonPressed(MouseButton.Right))
                            HandleErase(mouseMotion.Position);
                        break;
                    case LeftToolbar.Tool.Erase:
                        HandleErase(mouseMotion.Position);
                        break;
                    case LeftToolbar.Tool.Move:
                        HandleMove(mouseMotion.Position);
                        break;
                }
                lastMousePosition = mouseMotion.Position;
            }
        }
    }
    
    private void HandleToolAction(Vector2 position)
    {
        switch (currentTool)
        {
            case LeftToolbar.Tool.Paint:
                HandlePaint(position);
                break;
            case LeftToolbar.Tool.Erase:
                HandleErase(position);
                break;
            case LeftToolbar.Tool.Pattern:
                HandlePatternPlace(position);
                break;
        }
    }
    
    private void HandlePaint(Vector2 position)
    {
        var gridPos = ScreenToGrid(position);
        if (gridPos.HasValue)
        {
            PaintBrush(gridPos.Value.X, gridPos.Value.Y, simulation.BrushSize, simulation.BrushIntensity);
        }
    }
    
    private void HandleErase(Vector2 position)
    {
        var gridPos = ScreenToGrid(position);
        if (gridPos.HasValue)
        {
            PaintBrush(gridPos.Value.X, gridPos.Value.Y, simulation.BrushSize, -simulation.BrushIntensity);
        }
    }
    
    private void HandleMove(Vector2 position)
    {
        if (displaySprite != null)
        {
            var delta = position - lastMousePosition;
            displaySprite.Position += delta;
        }
    }
    
    private void HandlePatternPlace(Vector2 position)
    {
        var gridPos = ScreenToGrid(position);
        if (gridPos.HasValue)
        {
            PlaceRandomPattern(gridPos.Value.X, gridPos.Value.Y);
        }
    }
    
    private Vector2I? ScreenToGrid(Vector2 screenPos)
    {
        if (displaySprite == null) return null;
        
        var localPos = screenPos - displaySprite.Position;
        var gridX = (int)(localPos.X / displaySprite.Scale.X);
        var gridY = (int)(localPos.Y / displaySprite.Scale.Y);
        
        if (gridX >= 0 && gridX < simulation.GridWidth && gridY >= 0 && gridY < simulation.GridHeight)
        {
            return new Vector2I(gridX, gridY);
        }
        
        return null;
    }
    
    private void PaintBrush(int centerX, int centerY, float radius, float intensity)
    {
        // Use the simulation's paint method
        simulation.PaintBrush(centerX, centerY, radius, intensity);
    }
    
    private void PlaceRandomPattern(int centerX, int centerY)
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        
        int size = 10;
        for (int dx = -size; dx <= size; dx++)
        {
            for (int dy = -size; dy <= size; dy++)
            {
                int x = centerX + dx;
                int y = centerY + dy;
                
                if (x >= 0 && x < simulation.GridWidth && y >= 0 && y < simulation.GridHeight)
                {
                    if (dx * dx + dy * dy <= size * size)
                    {
                        simulation.SetGridValue(x, y, rng.Randf() > 0.5f ? rng.Randf() : 0.0f);
                    }
                }
            }
        }
    }
    
    private void UpdateSimulationDisplay()
    {
        if (displaySprite == null || simulation == null) return;
        
        var availableSize = Size;
        if (availableSize.X <= 0 || availableSize.Y <= 0) return;
        
        // Add padding to prevent the simulation from touching the edges
        var padding = 10.0f;
        var effectiveWidth = availableSize.X - (padding * 2);
        var effectiveHeight = availableSize.Y - (padding * 2);
        
        var scale = Mathf.Min(effectiveWidth / simulation.GridWidth, effectiveHeight / simulation.GridHeight);
        displaySprite.Scale = new Vector2(scale, scale);
        
        var centeredX = padding + (effectiveWidth - simulation.GridWidth * scale) / 2;
        var centeredY = padding + (effectiveHeight - simulation.GridHeight * scale) / 2;
        displaySprite.Position = new Vector2(centeredX, centeredY);
    }
    
    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            UpdateSimulationDisplay();
        }
    }
    
    private void OnGridResized(int width, int height)
    {
        // Get new display sprite after resize
        if (simulation != null)
        {
            // Remove old sprite
            if (displaySprite != null)
            {
                displaySprite.QueueFree();
            }
            
            // Get new sprite with updated texture
            displaySprite = simulation.GetDisplaySprite();
            if (displaySprite != null)
            {
                AddChild(displaySprite);
                MoveChild(displaySprite, 0); // Ensure it's behind the overlay
                
                // Update overlay with new sprite
                if (toolOverlay != null)
                {
                    toolOverlay.Initialize(simulation, displaySprite);
                }
                
                // Update display
                CallDeferred(nameof(UpdateSimulationDisplay));
            }
        }
    }
}