using Godot;

public partial class PatternPreview : Control
{
    private float[,] patternData;
    
    public void SetPattern(float[,] data)
    {
        patternData = data;
        QueueRedraw();
    }
    
    public override void _Draw()
    {
        if (patternData == null) return;
        
        var rect = GetRect();
        var cellWidth = rect.Size.X / patternData.GetLength(0);
        var cellHeight = rect.Size.Y / patternData.GetLength(1);
        
        // Draw background
        DrawRect(new Rect2(Vector2.Zero, rect.Size), new Color(0.02f, 0.02f, 0.05f));
        
        // Draw pattern
        for (int x = 0; x < patternData.GetLength(0); x++)
        {
            for (int y = 0; y < patternData.GetLength(1); y++)
            {
                var value = patternData[x, y];
                if (value > 0.01f)
                {
                    var color = ColorMapper.MapValue(value, ColorMapper.ColorScheme.Plasma);
                    var cellRect = new Rect2(
                        x * cellWidth, 
                        y * cellHeight, 
                        cellWidth, 
                        cellHeight
                    );
                    DrawRect(cellRect, color);
                }
            }
        }
        
        // Draw border
        DrawRect(new Rect2(Vector2.Zero, rect.Size), new Color(0.3f, 0.4f, 0.6f, 0.6f), false, 1);
    }
}