using Godot;

public partial class ToolOverlay : Control
{
    private LeniaSimulation simulation;
    private LeftToolbar.Tool currentTool = LeftToolbar.Tool.Paint;
    private Sprite2D displaySprite;
    
    public void Initialize(LeniaSimulation sim, Sprite2D sprite)
    {
        simulation = sim;
        displaySprite = sprite;
        MouseFilter = MouseFilterEnum.Ignore; // Let mouse events pass through
    }
    
    public void SetTool(LeftToolbar.Tool tool)
    {
        currentTool = tool;
        QueueRedraw();
    }
    
    public override void _Draw()
    {
        if (currentTool == LeftToolbar.Tool.Paint || currentTool == LeftToolbar.Tool.Erase)
        {
            var mousePos = GetGlobalMousePosition() - GlobalPosition;
            var gridPos = ScreenToGrid(mousePos);
            
            if (gridPos.HasValue && displaySprite != null)
            {
                var brushScreenRadius = simulation.BrushSize * displaySprite.Scale.X;
                var brushCenter = new Vector2(
                    gridPos.Value.X * displaySprite.Scale.X + displaySprite.Position.X,
                    gridPos.Value.Y * displaySprite.Scale.Y + displaySprite.Position.Y
                );
                
                var color = currentTool == LeftToolbar.Tool.Paint ? 
                    new Color(0.3f, 0.8f, 0.3f, 0.5f) : 
                    new Color(0.8f, 0.3f, 0.3f, 0.5f);
                    
                DrawCircle(brushCenter, brushScreenRadius, color);
                DrawArc(brushCenter, brushScreenRadius, 0, Mathf.Tau, 32, color * 1.5f, 2.0f);
            }
        }
    }
    
    public override void _Process(double delta)
    {
        if (currentTool == LeftToolbar.Tool.Paint || currentTool == LeftToolbar.Tool.Erase)
        {
            QueueRedraw();
        }
    }
    
    private Vector2I? ScreenToGrid(Vector2 screenPos)
    {
        if (displaySprite == null) return null;
        
        var spriteRect = new Rect2(
            displaySprite.Position,
            displaySprite.Texture.GetSize() * displaySprite.Scale
        );
        
        if (!spriteRect.HasPoint(screenPos))
            return null;
            
        var relativePos = screenPos - displaySprite.Position;
        var normalizedPos = relativePos / displaySprite.Scale;
        
        var gridX = (int)normalizedPos.X;
        var gridY = (int)normalizedPos.Y;
        
        if (gridX >= 0 && gridX < simulation.GridWidth && gridY >= 0 && gridY < simulation.GridHeight)
        {
            return new Vector2I(gridX, gridY);
        }
        
        return null;
    }
}