using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PopulationGraph : Control
{
    private List<float> populationHistory = new List<float>();
    private const int MaxHistorySize = 100;
    private Label populationLabel;
    private Color graphColor = new Color(0.3f, 0.8f, 0.6f);
    
    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(340, 90);
        
        populationLabel = new Label();
        populationLabel.Text = "Population: 0.0%";
        populationLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        populationLabel.AddThemeFontSizeOverride("font_size", 14);
        populationLabel.Position = new Vector2(10, 5);
        AddChild(populationLabel);
    }
    
    public void UpdatePopulation(float[,] grid, int width, int height)
    {
        float totalPopulation = 0;
        int totalCells = width * height;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                totalPopulation += grid[x, y];
            }
        }
        
        float populationPercentage = (totalPopulation / totalCells) * 100;
        
        populationHistory.Add(populationPercentage);
        if (populationHistory.Count > MaxHistorySize)
        {
            populationHistory.RemoveAt(0);
        }
        
        populationLabel.Text = $"Population: {populationPercentage:F1}%";
        QueueRedraw();
    }
    
    public override void _Draw()
    {
        if (populationHistory.Count < 2) return;
        
        var rect = GetRect();
        var graphRect = new Rect2(10, 30, rect.Size.X - 20, rect.Size.Y - 40);
        
        // Draw background
        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(0.05f, 0.05f, 0.1f, 0.8f);
        bgStyle.BorderWidthTop = 1;
        bgStyle.BorderWidthBottom = 1;
        bgStyle.BorderWidthLeft = 1;
        bgStyle.BorderWidthRight = 1;
        bgStyle.BorderColor = new Color(0.2f, 0.2f, 0.3f);
        bgStyle.CornerRadiusTopLeft = 4;
        bgStyle.CornerRadiusTopRight = 4;
        bgStyle.CornerRadiusBottomLeft = 4;
        bgStyle.CornerRadiusBottomRight = 4;
        bgStyle.Draw(GetCanvasItem(), graphRect);
        
        // Find min/max for scaling
        float minVal = populationHistory.Min();
        float maxVal = populationHistory.Max();
        if (maxVal - minVal < 0.1f) // Ensure some range
        {
            minVal = Mathf.Max(0, minVal - 0.05f);
            maxVal = minVal + 0.1f;
        }
        
        // Draw grid lines
        for (int i = 0; i <= 4; i++)
        {
            float y = graphRect.Position.Y + (i / 4.0f) * graphRect.Size.Y;
            DrawLine(
                new Vector2(graphRect.Position.X, y),
                new Vector2(graphRect.Position.X + graphRect.Size.X, y),
                new Color(0.2f, 0.2f, 0.3f, 0.5f),
                1
            );
        }
        
        // Draw population curve
        var points = new Vector2[populationHistory.Count];
        for (int i = 0; i < populationHistory.Count; i++)
        {
            float x = graphRect.Position.X + (i / (float)(MaxHistorySize - 1)) * graphRect.Size.X;
            float normalizedValue = (populationHistory[i] - minVal) / (maxVal - minVal);
            float y = graphRect.Position.Y + graphRect.Size.Y - (normalizedValue * graphRect.Size.Y);
            points[i] = new Vector2(x, y);
        }
        
        // Draw the line graph
        for (int i = 1; i < points.Length; i++)
        {
            DrawLine(points[i - 1], points[i], graphColor, 2);
        }
        
        // Draw points
        foreach (var point in points)
        {
            DrawCircle(point, 2, graphColor);
        }
        
        // Draw current value indicator
        if (points.Length > 0)
        {
            var lastPoint = points[points.Length - 1];
            DrawCircle(lastPoint, 4, new Color(1.0f, 1.0f, 1.0f));
            DrawCircle(lastPoint, 3, graphColor);
        }
    }
}