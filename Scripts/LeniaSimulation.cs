using Godot;
using System;
using System.Threading.Tasks;

public partial class LeniaSimulation : Node2D
{
    [Signal]
    public delegate void GridResizedEventHandler(int width, int height);
    
    [Export] public int GridWidth = 128;
    [Export] public int GridHeight = 128;
    [Export] public float DeltaTime = 0.1f;
    [Export] public float KernelRadius = 13.0f;
    [Export] public float GrowthMean = 0.15f;
    [Export] public float GrowthSigma = 0.015f;
    [Export] public ColorMapper.ColorScheme CurrentColorScheme = ColorMapper.ColorScheme.Heat;
    [Export] public float BrushSize = 3.0f;
    [Export] public float BrushIntensity = 1.0f;
    [Export] public float SimulationSpeed = 1.0f;
    [Export] public bool IsPaused = false;
    
    private float[,] currentGrid;
    private float[,] nextGrid;
    private float[,] kernel;
    private (int x, int y, float weight)[] kernelOffsets;
    private Image gridImage;
    private ImageTexture gridTexture;
    private Sprite2D displaySprite;
    private byte[] pixelData;
    
    public override void _Ready()
    {
        InitializeGrids();
        CreateKernel();
        SetupDisplay();
        InitializePattern();
    }
    
    private void InitializeGrids()
    {
        currentGrid = new float[GridWidth, GridHeight];
        nextGrid = new float[GridWidth, GridHeight];
        pixelData = new byte[GridWidth * GridHeight * 3]; // RGB format
    }
    
    public void CreateKernel()
    {
        int kernelSize = (int)(KernelRadius * 2) + 1;
        kernel = new float[kernelSize, kernelSize];
        var offsetsList = new System.Collections.Generic.List<(int, int, float)>();
        float kernelSum = 0;
        
        for (int i = 0; i < kernelSize; i++)
        {
            for (int j = 0; j < kernelSize; j++)
            {
                float x = i - KernelRadius;
                float y = j - KernelRadius;
                float distance = Mathf.Sqrt(x * x + y * y) / KernelRadius;
                
                if (distance <= 1.0f)
                {
                    kernel[i, j] = Mathf.Exp(4.0f * (1.0f - Mathf.Pow(distance, 2)));
                    kernelSum += kernel[i, j];
                }
            }
        }
        
        // Normalize and create offset cache
        for (int i = 0; i < kernelSize; i++)
        {
            for (int j = 0; j < kernelSize; j++)
            {
                kernel[i, j] /= kernelSum;
                if (kernel[i, j] > 0.001f) // Only cache significant weights
                {
                    offsetsList.Add((i - (int)KernelRadius, j - (int)KernelRadius, kernel[i, j]));
                }
            }
        }
        
        kernelOffsets = offsetsList.ToArray();
    }
    
    private void SetupDisplay()
    {
        gridImage = Image.CreateEmpty(GridWidth, GridHeight, false, Image.Format.Rgb8);
        gridTexture = ImageTexture.CreateFromImage(gridImage);
        
        displaySprite = new Sprite2D();
        displaySprite.Texture = gridTexture;
        displaySprite.Centered = false;
        // Don't add as child - SimulationCanvas will handle positioning
    }
    
    public void InitializePattern()
    {
        var rng = new RandomNumberGenerator();
        rng.Seed = 42;
        
        int centerX = GridWidth / 2;
        int centerY = GridHeight / 2;
        int radius = 20;
        
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        currentGrid[x, y] = rng.Randf();
                    }
                }
            }
        }
    }
    
    public override void _Process(double delta)
    {
        // Apply simulation speed multiplier
        if (!IsPaused && SimulationSpeed > 0)
        {
            UpdateSimulation();
        }
        UpdateDisplay();
    }
    
    public void SetPaused(bool paused)
    {
        IsPaused = paused;
    }
    
    public void ResizeGrid(int newWidth, int newHeight)
    {
        if (newWidth == GridWidth && newHeight == GridHeight)
            return;
            
        GridWidth = newWidth;
        GridHeight = newHeight;
        
        // Reinitialize grids with new size
        InitializeGrids();
        CreateKernel();
        SetupDisplay();
        InitializePattern();
        
        // Notify any listening components about the resize
        EmitSignal(SignalName.GridResized, newWidth, newHeight);
    }
    
    public void PaintBrush(int centerX, int centerY, float radius, float intensity)
    {
        int intRadius = (int)radius;
        for (int dx = -intRadius; dx <= intRadius; dx++)
        {
            for (int dy = -intRadius; dy <= intRadius; dy++)
            {
                int x = centerX + dx;
                int y = centerY + dy;
                
                if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
                {
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance <= radius)
                    {
                        float falloff = 1.0f - (distance / radius);
                        float paintAmount = intensity * falloff * 0.1f; // Gentle painting
                        currentGrid[x, y] = Mathf.Clamp(currentGrid[x, y] + paintAmount, 0.0f, 1.0f);
                    }
                }
            }
        }
    }
    
    private void UpdateSimulation()
    {
        // Use parallel processing for better performance
        Parallel.For(0, GridWidth, x =>
        {
            for (int y = 0; y < GridHeight; y++)
            {
                float convolution = 0;
                
                // Use cached kernel offsets (much faster)
                foreach (var (ox, oy, weight) in kernelOffsets)
                {
                    int nx = (x + ox + GridWidth) % GridWidth;
                    int ny = (y + oy + GridHeight) % GridHeight;
                    convolution += currentGrid[nx, ny] * weight;
                }
                
                float growth = GrowthFunction(convolution);
                nextGrid[x, y] = Mathf.Clamp(currentGrid[x, y] + DeltaTime * growth, 0.0f, 1.0f);
            }
        });
        
        // Swap grids efficiently
        (currentGrid, nextGrid) = (nextGrid, currentGrid);
    }
    
    private float GrowthFunction(float u)
    {
        return 2.0f * Mathf.Exp(-Mathf.Pow((u - GrowthMean) / GrowthSigma, 2) / 2.0f) - 1.0f;
    }
    
    private void UpdateDisplay()
    {
        // Update pixel data array with parallel processing
        Parallel.For(0, GridHeight, y =>
        {
            for (int x = 0; x < GridWidth; x++)
            {
                var color = ColorMapper.MapValue(currentGrid[x, y], CurrentColorScheme);
                int index = (y * GridWidth + x) * 3;
                pixelData[index] = (byte)(color.R * 255);     // Red
                pixelData[index + 1] = (byte)(color.G * 255); // Green
                pixelData[index + 2] = (byte)(color.B * 255); // Blue
            }
        });
        
        // Create new image from byte array (much faster than individual SetPixel calls)
        gridImage = Image.CreateFromData(GridWidth, GridHeight, false, Image.Format.Rgb8, pixelData);
        gridTexture.Update(gridImage);
    }
    
    public void RandomPattern()
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                currentGrid[x, y] = rng.Randf() > 0.5f ? rng.Randf() : 0.0f;
            }
        }
    }
    
    public void OrbiumPattern()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                currentGrid[x, y] = 0.0f;
            }
        }
        
        float[,] orbium = {
            {0,0,0,0,0,0,0.1f,0.14f,0.1f,0,0,0.03f,0.03f,0,0,0.3f,0,0,0,0},
            {0,0,0,0,0,0.08f,0.24f,0.3f,0.3f,0.18f,0.14f,0.15f,0.16f,0.15f,0.09f,0.2f,0,0,0,0},
            {0,0,0,0,0,0.15f,0.34f,0.44f,0.46f,0.38f,0.18f,0.14f,0.11f,0.13f,0.19f,0.18f,0.45f,0,0,0},
            {0,0,0,0,0.06f,0.13f,0.39f,0.5f,0.5f,0.37f,0.06f,0,0,0,0.02f,0.16f,0.68f,0,0,0},
            {0,0,0,0.11f,0.17f,0.17f,0.33f,0.4f,0.38f,0.28f,0.14f,0,0,0,0,0,0.18f,0.42f,0,0},
            {0,0,0.09f,0.18f,0.13f,0.06f,0.08f,0.26f,0.32f,0.32f,0.27f,0,0,0,0,0,0,0.82f,0,0},
            {0.27f,0,0.16f,0.12f,0,0,0,0.25f,0.38f,0.44f,0.45f,0.34f,0,0,0,0,0,0.22f,0.17f,0},
            {0,0.07f,0.2f,0.02f,0,0,0,0.31f,0.48f,0.57f,0.6f,0.57f,0,0,0,0,0,0,0.49f,0},
            {0,0.59f,0.19f,0,0,0,0,0.2f,0.57f,0.69f,0.76f,0.76f,0.49f,0,0,0,0,0,0.36f,0},
            {0,0.58f,0.19f,0,0,0,0,0,0.67f,0.83f,0.9f,0.92f,0.87f,0.12f,0,0,0,0,0.22f,0.07f},
            {0,0.5f,0.19f,0,0,0,0,0,0.76f,0.92f,0.99f,0.99f,0.94f,0.76f,0,0,0,0,0.18f,0.11f},
            {0,0.6f,0.19f,0,0,0,0,0,0.75f,0.94f,0.99f,0.99f,0.95f,0.89f,0.49f,0,0,0,0.15f,0.1f},
            {0,0.58f,0.19f,0,0,0,0,0,0.7f,0.93f,0.99f,0.99f,0.97f,0.94f,0.76f,0,0,0,0.13f,0.09f},
            {0,0.5f,0.19f,0,0,0,0,0,0.69f,0.88f,0.98f,0.99f,0.99f,0.97f,0.77f,0.15f,0,0,0.11f,0.08f},
            {0,0.24f,0.13f,0,0,0,0,0.13f,0.7f,0.85f,0.96f,0.99f,0.99f,0.98f,0.85f,0.25f,0,0,0.1f,0.07f},
            {0,0,0.07f,0.05f,0,0,0,0.7f,0.82f,0.88f,0.95f,0.99f,0.99f,0.98f,0.87f,0.35f,0,0.07f,0.09f,0.05f},
            {0,0,0,0.14f,0.09f,0.05f,0.14f,0.8f,0.85f,0.9f,0.95f,0.99f,0.99f,0.95f,0.76f,0.34f,0.18f,0.09f,0.05f,0},
            {0,0,0,0,0.09f,0.32f,0.48f,0.71f,0.79f,0.84f,0.89f,0.94f,0.91f,0.78f,0.45f,0.29f,0.14f,0.05f,0,0},
            {0,0,0,0,0,0.32f,0.58f,0.71f,0.72f,0.71f,0.76f,0.77f,0.7f,0.56f,0.34f,0.18f,0.05f,0,0,0},
            {0,0,0,0,0,0.14f,0.4f,0.63f,0.68f,0.68f,0.7f,0.66f,0.56f,0.44f,0.27f,0.09f,0,0,0,0}
        };
        
        int ox = GridWidth / 2 - 10;
        int oy = GridHeight / 2 - 10;
        
        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                if (ox + x >= 0 && ox + x < GridWidth && oy + y >= 0 && oy + y < GridHeight)
                {
                    currentGrid[ox + x, oy + y] = orbium[y, x];
                }
            }
        }
    }
    
    public void StepOneFrame()
    {
        UpdateSimulation();
    }
    
    public float[,] GetCurrentGrid()
    {
        return currentGrid;
    }
    
    public Sprite2D GetDisplaySprite()
    {
        return displaySprite;
    }
    
    public Image GetCanvasScreenshot()
    {
        if (gridImage == null) return null;
        
        // Create a copy of the current grid image
        var screenshot = Image.CreateEmpty(GridWidth, GridHeight, false, Image.Format.Rgb8);
        screenshot.CopyFrom(gridImage);
        
        return screenshot;
    }
    
    public void SetGridValue(int x, int y, float value)
    {
        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
        {
            currentGrid[x, y] = Mathf.Clamp(value, 0.0f, 1.0f);
        }
    }
}