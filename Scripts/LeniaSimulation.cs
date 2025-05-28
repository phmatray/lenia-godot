using Godot;
using System;

public partial class LeniaSimulation : Node2D
{
    [Export] public int GridWidth = 256;
    [Export] public int GridHeight = 256;
    [Export] public float DeltaTime = 0.1f;
    [Export] public float KernelRadius = 13.0f;
    [Export] public float GrowthMean = 0.15f;
    [Export] public float GrowthSigma = 0.015f;
    [Export] public ColorMapper.ColorScheme CurrentColorScheme = ColorMapper.ColorScheme.Heat;
    
    private float[,] currentGrid;
    private float[,] nextGrid;
    private float[,] kernel;
    private Image gridImage;
    private ImageTexture gridTexture;
    private Sprite2D displaySprite;
    
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
    }
    
    public void CreateKernel()
    {
        int kernelSize = (int)(KernelRadius * 2) + 1;
        kernel = new float[kernelSize, kernelSize];
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
        
        for (int i = 0; i < kernelSize; i++)
        {
            for (int j = 0; j < kernelSize; j++)
            {
                kernel[i, j] /= kernelSum;
            }
        }
    }
    
    private void SetupDisplay()
    {
        gridImage = Image.CreateEmpty(GridWidth, GridHeight, false, Image.Format.Rgb8);
        gridTexture = ImageTexture.CreateFromImage(gridImage);
        
        displaySprite = new Sprite2D();
        displaySprite.Texture = gridTexture;
        displaySprite.Centered = false;
        AddChild(displaySprite);
        
        var viewport = GetViewport();
        var screenSize = viewport.GetVisibleRect().Size;
        
        var panelWidth = 400;
        var availableWidth = screenSize.X - panelWidth;
        var availableHeight = screenSize.Y;
        
        var scale = Mathf.Min(availableWidth / GridWidth, availableHeight / GridHeight);
        displaySprite.Scale = new Vector2(scale, scale);
        
        // Center the simulation in the available space
        var centeredX = panelWidth + (availableWidth - GridWidth * scale) / 2;
        var centeredY = (availableHeight - GridHeight * scale) / 2;
        displaySprite.Position = new Vector2(centeredX, centeredY);
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
        UpdateSimulation();
        UpdateDisplay();
    }
    
    private void UpdateSimulation()
    {
        int kernelRadius = (int)KernelRadius;
        
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                float convolution = 0;
                
                for (int kx = -kernelRadius; kx <= kernelRadius; kx++)
                {
                    for (int ky = -kernelRadius; ky <= kernelRadius; ky++)
                    {
                        int nx = (x + kx + GridWidth) % GridWidth;
                        int ny = (y + ky + GridHeight) % GridHeight;
                        
                        convolution += currentGrid[nx, ny] * kernel[kx + kernelRadius, ky + kernelRadius];
                    }
                }
                
                float growth = GrowthFunction(convolution);
                nextGrid[x, y] = Mathf.Clamp(currentGrid[x, y] + DeltaTime * growth, 0.0f, 1.0f);
            }
        }
        
        var temp = currentGrid;
        currentGrid = nextGrid;
        nextGrid = temp;
    }
    
    private float GrowthFunction(float u)
    {
        return 2.0f * Mathf.Exp(-Mathf.Pow((u - GrowthMean) / GrowthSigma, 2) / 2.0f) - 1.0f;
    }
    
    private void UpdateDisplay()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                var color = ColorMapper.MapValue(currentGrid[x, y], CurrentColorScheme);
                gridImage.SetPixel(x, y, color);
            }
        }
        
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
}