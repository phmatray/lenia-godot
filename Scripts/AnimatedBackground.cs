using Godot;

public partial class AnimatedBackground : Control
{
    private float time = 0.0f;
    private Color[] particleColors = {
        new Color(0.2f, 0.4f, 0.8f, 0.3f),
        new Color(0.4f, 0.6f, 1.0f, 0.2f),
        new Color(0.1f, 0.3f, 0.6f, 0.4f)
    };
    
    public override void _Ready()
    {
        AnchorLeft = 0;
        AnchorTop = 0;
        AnchorRight = 1;
        AnchorBottom = 1;
        ZIndex = -1; // Behind everything
    }
    
    public override void _Process(double delta)
    {
        time += (float)delta;
        QueueRedraw();
    }
    
    public override void _Draw()
    {
        var rect = GetRect();
        
        // Draw subtle animated particles
        for (int i = 0; i < 20; i++)
        {
            float phase = time * 0.5f + i * 0.3f;
            float x = (Mathf.Sin(phase * 0.7f) * 0.3f + 0.5f) * rect.Size.X;
            float y = (Mathf.Cos(phase * 0.5f) * 0.3f + 0.5f) * rect.Size.Y;
            float size = (Mathf.Sin(phase * 1.2f) * 0.5f + 1.0f) * 3.0f;
            
            var color = particleColors[i % particleColors.Length];
            color.A *= (Mathf.Sin(phase * 2.0f) * 0.3f + 0.7f);
            
            DrawCircle(new Vector2(x, y), size, color);
        }
        
        // Draw subtle grid pattern
        var gridColor = new Color(0.1f, 0.2f, 0.4f, 0.1f);
        for (int x = 0; x < rect.Size.X; x += 50)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, rect.Size.Y), gridColor, 1);
        }
        for (int y = 0; y < rect.Size.Y; y += 50)
        {
            DrawLine(new Vector2(0, y), new Vector2(rect.Size.X, y), gridColor, 1);
        }
    }
}