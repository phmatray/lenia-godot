using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PatternLibrary : Control
{
    [Signal]
    public delegate void PatternSelectedEventHandler(string patternId);
    
    public enum PatternCategory
    {
        Classic,
        Creatures,
        Oscillators,
        Generators,
        Experimental,
        UserSaved
    }
    
    public class LeniaPattern
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PatternCategory Category { get; set; }
        public float[,] Data { get; set; }
        public LeniaParameters Parameters { get; set; }
        public float Difficulty { get; set; } // 1-5 stars
        public bool IsUserPattern { get; set; }
        public System.Action<LeniaSimulation> ApplyAction { get; set; }
    }
    
    public class LeniaParameters
    {
        public float DeltaTime { get; set; } = 0.1f;
        public float KernelRadius { get; set; } = 13.0f;
        public float GrowthMean { get; set; } = 0.15f;
        public float GrowthSigma { get; set; } = 0.015f;
        public ColorMapper.ColorScheme ColorScheme { get; set; } = ColorMapper.ColorScheme.Heat;
    }
    
    private Dictionary<PatternCategory, List<LeniaPattern>> patternsByCategory;
    private TabContainer categoryTabs;
    private Control searchContainer;
    private LineEdit searchField;
    private OptionButton difficultyFilter;
    private List<LeniaPattern> filteredPatterns;
    private LeniaSimulation simulation;
    
    public override void _Ready()
    {
        SetupPatternLibrary();
        CreateUI();
        LoadBuiltInPatterns();
        LoadUserPatterns();
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
    }
    
    private void SetupPatternLibrary()
    {
        patternsByCategory = new Dictionary<PatternCategory, List<LeniaPattern>>();
        foreach (PatternCategory category in System.Enum.GetValues<PatternCategory>())
        {
            patternsByCategory[category] = new List<LeniaPattern>();
        }
        filteredPatterns = new List<LeniaPattern>();
    }
    
    private void CreateUI()
    {
        // Main container
        var vbox = new VBoxContainer();
        AddChild(vbox);
        
        // Title
        var title = new Label();
        title.Text = "Pattern Library";
        title.AddThemeStyleboxOverride("normal", CreateHeaderStyle());
        title.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        title.AddThemeFontSizeOverride("font_size", 16);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.CustomMinimumSize = new Vector2(0, 40);
        vbox.AddChild(title);
        
        // Search and filters
        CreateSearchControls(vbox);
        
        // Category tabs
        categoryTabs = new TabContainer();
        categoryTabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(categoryTabs);
        
        // Create tabs for each category
        foreach (PatternCategory category in System.Enum.GetValues<PatternCategory>())
        {
            CreateCategoryTab(category);
        }
    }
    
    private void CreateSearchControls(VBoxContainer parent)
    {
        searchContainer = new HBoxContainer();
        searchContainer.AddThemeConstantOverride("separation", 10);
        parent.AddChild(searchContainer);
        
        // Search field
        searchField = new LineEdit();
        searchField.PlaceholderText = "Search patterns...";
        searchField.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        searchField.TextChanged += OnSearchTextChanged;
        searchContainer.AddChild(searchField);
        
        // Difficulty filter
        var difficultyLabel = new Label();
        difficultyLabel.Text = "Difficulty:";
        searchContainer.AddChild(difficultyLabel);
        
        difficultyFilter = new OptionButton();
        difficultyFilter.AddItem("All");
        difficultyFilter.AddItem("★ Beginner");
        difficultyFilter.AddItem("★★ Easy");
        difficultyFilter.AddItem("★★★ Medium");
        difficultyFilter.AddItem("★★★★ Hard");
        difficultyFilter.AddItem("★★★★★ Expert");
        difficultyFilter.Selected = 0;
        difficultyFilter.ItemSelected += OnDifficultyFilterChanged;
        searchContainer.AddChild(difficultyFilter);
    }
    
    private void CreateCategoryTab(PatternCategory category)
    {
        var scrollContainer = new ScrollContainer();
        scrollContainer.Name = GetCategoryDisplayName(category);
        categoryTabs.AddChild(scrollContainer);
        
        var gridContainer = new GridContainer();
        gridContainer.Columns = 3;
        gridContainer.AddThemeConstantOverride("h_separation", 15);
        gridContainer.AddThemeConstantOverride("v_separation", 15);
        scrollContainer.AddChild(gridContainer);
    }
    
    private void LoadBuiltInPatterns()
    {
        // Classic patterns
        AddPattern(new LeniaPattern
        {
            Id = "orbium",
            Name = "Orbium",
            Description = "The classic self-propelling creature. Moves in smooth circles and can interact with other Orbiums.",
            Category = PatternCategory.Classic,
            Difficulty = 2.0f,
            Parameters = new LeniaParameters { DeltaTime = 0.1f, KernelRadius = 13.0f, GrowthMean = 0.15f, GrowthSigma = 0.015f },
            ApplyAction = sim => sim.OrbiumPattern()
        });
        
        AddPattern(new LeniaPattern
        {
            Id = "smooth_glider",
            Name = "Smooth Life Glider",
            Description = "A variant that glides smoothly across the grid. Great for beginners to understand movement.",
            Category = PatternCategory.Creatures,
            Difficulty = 1.0f,
            Parameters = new LeniaParameters { DeltaTime = 0.08f, KernelRadius = 10.0f, GrowthMean = 0.12f, GrowthSigma = 0.02f },
            ApplyAction = sim => CreateGliderPattern(sim)
        });
        
        AddPattern(new LeniaPattern
        {
            Id = "asymptotic_orbium",
            Name = "Asymptotic Orbium",
            Description = "An evolved Orbium that creates beautiful trailing patterns as it moves.",
            Category = PatternCategory.Creatures,
            Difficulty = 3.0f,
            Parameters = new LeniaParameters { DeltaTime = 0.12f, KernelRadius = 15.0f, GrowthMean = 0.18f, GrowthSigma = 0.012f },
            ApplyAction = sim => CreateAsymptoticOrbium(sim)
        });
        
        AddPattern(new LeniaPattern
        {
            Id = "pulse_generator",
            Name = "Pulse Generator",
            Description = "Creates regular pulses that emanate from the center. Hypnotic and rhythmic.",
            Category = PatternCategory.Generators,
            Difficulty = 2.0f,
            Parameters = new LeniaParameters { DeltaTime = 0.15f, KernelRadius = 20.0f, GrowthMean = 0.22f, GrowthSigma = 0.025f },
            ApplyAction = sim => CreatePulseGenerator(sim)
        });
        
        AddPattern(new LeniaPattern
        {
            Id = "spiral_oscillator",
            Name = "Spiral Oscillator",
            Description = "Creates beautiful rotating spiral patterns. Mesmerizing to watch.",
            Category = PatternCategory.Oscillators,
            Difficulty = 3.0f,
            Parameters = new LeniaParameters { DeltaTime = 0.09f, KernelRadius = 12.0f, GrowthMean = 0.14f, GrowthSigma = 0.018f },
            ApplyAction = sim => CreateSpiralOscillator(sim)
        });
        
        AddPattern(new LeniaPattern
        {
            Id = "breathing_cell",
            Name = "Breathing Cell",
            Description = "A stationary pattern that pulses in and out like breathing. Very relaxing.",
            Category = PatternCategory.Oscillators,
            Difficulty = 1.0f,
            Parameters = new LeniaParameters { DeltaTime = 0.13f, KernelRadius = 8.0f, GrowthMean = 0.16f, GrowthSigma = 0.022f },
            ApplyAction = sim => CreateBreathingCell(sim)
        });
        
        AddPattern(new LeniaPattern
        {
            Id = "chaos_seed",
            Name = "Chaos Seed",
            Description = "Unpredictable pattern that creates different behaviors each time. For the adventurous!",
            Category = PatternCategory.Experimental,
            Difficulty = 5.0f,
            Parameters = new LeniaParameters { DeltaTime = 0.11f, KernelRadius = 16.0f, GrowthMean = 0.19f, GrowthSigma = 0.014f },
            ApplyAction = sim => CreateChaosSeed(sim)
        });
        
        AddPattern(new LeniaPattern
        {
            Id = "wave_collision",
            Name = "Wave Collision",
            Description = "Two wave sources that create beautiful interference patterns when they meet.",
            Category = PatternCategory.Experimental,
            Difficulty = 4.0f,
            Parameters = new LeniaParameters { DeltaTime = 0.1f, KernelRadius = 18.0f, GrowthMean = 0.2f, GrowthSigma = 0.02f },
            ApplyAction = sim => CreateWaveCollision(sim)
        });
        
        RefreshAllCategories();
    }
    
    private void LoadUserPatterns()
    {
        var userPatternsDir = "user://patterns/";
        if (!DirAccess.DirExistsAbsolute(userPatternsDir))
        {
            DirAccess.MakeDirRecursiveAbsolute(userPatternsDir);
            return;
        }
        
        // Load user-saved patterns from files
        var dir = DirAccess.Open(userPatternsDir);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            
            while (fileName != "")
            {
                if (fileName.EndsWith(".json"))
                {
                    LoadUserPattern(userPatternsDir + fileName);
                }
                fileName = dir.GetNext();
            }
        }
    }
    
    private void LoadUserPattern(string filePath)
    {
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null) return;
        
        var jsonString = file.GetAsText();
        file.Close();
        
        var json = new Json();
        var parseResult = json.Parse(jsonString);
        
        if (parseResult == Error.Ok)
        {
            var data = json.Data.AsGodotDictionary();
            // Parse user pattern data and add to library
            // Implementation would depend on the JSON structure
        }
    }
    
    private void AddPattern(LeniaPattern pattern)
    {
        patternsByCategory[pattern.Category].Add(pattern);
    }
    
    private void RefreshAllCategories()
    {
        foreach (var category in patternsByCategory.Keys)
        {
            RefreshCategoryDisplay(category);
        }
    }
    
    private void RefreshCategoryDisplay(PatternCategory category)
    {
        var tabIndex = (int)category;
        if (tabIndex >= categoryTabs.GetChildCount()) return;
        
        var scrollContainer = categoryTabs.GetChild(tabIndex) as ScrollContainer;
        var gridContainer = scrollContainer?.GetChild(0) as GridContainer;
        if (gridContainer == null) return;
        
        // Clear existing patterns
        foreach (Node child in gridContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add patterns for this category
        var patterns = GetFilteredPatterns(category);
        foreach (var pattern in patterns)
        {
            CreatePatternCard(gridContainer, pattern);
        }
    }
    
    private List<LeniaPattern> GetFilteredPatterns(PatternCategory category)
    {
        var patterns = patternsByCategory[category];
        
        if (!string.IsNullOrEmpty(searchField?.Text))
        {
            patterns = patterns.Where(p => 
                p.Name.ToLower().Contains(searchField.Text.ToLower()) ||
                p.Description.ToLower().Contains(searchField.Text.ToLower())).ToList();
        }
        
        if (difficultyFilter?.Selected > 0)
        {
            float minDifficulty = difficultyFilter.Selected;
            float maxDifficulty = difficultyFilter.Selected + 0.99f;
            patterns = patterns.Where(p => p.Difficulty >= minDifficulty && p.Difficulty <= maxDifficulty).ToList();
        }
        
        return patterns.OrderBy(p => p.Difficulty).ToList();
    }
    
    private void CreatePatternCard(GridContainer parent, LeniaPattern pattern)
    {
        var card = new Panel();
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = new Color(0.15f, 0.18f, 0.25f, 0.9f);
        cardStyle.SetBorderWidthAll(2);
        cardStyle.BorderColor = new Color(0.3f, 0.4f, 0.6f, 0.6f);
        cardStyle.SetCornerRadiusAll(8);
        card.AddThemeStyleboxOverride("panel", cardStyle);
        card.CustomMinimumSize = new Vector2(180, 220);
        parent.AddChild(card);
        
        var cardContainer = new VBoxContainer();
        cardContainer.AddThemeConstantOverride("separation", 8);
        card.AddChild(cardContainer);
        
        // Pattern preview
        var preview = new PatternPreview();
        if (pattern.Data != null)
        {
            preview.SetPattern(pattern.Data);
        }
        else
        {
            preview.SetPattern(GeneratePreviewPattern(pattern));
        }
        preview.CustomMinimumSize = new Vector2(160, 100);
        cardContainer.AddChild(preview);
        
        // Pattern name
        var nameLabel = new Label();
        nameLabel.Text = pattern.Name;
        nameLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        nameLabel.AddThemeFontSizeOverride("font_size", 12);
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        cardContainer.AddChild(nameLabel);
        
        // Difficulty stars
        var difficultyContainer = new HBoxContainer();
        difficultyContainer.Alignment = BoxContainer.AlignmentMode.Center;
        for (int i = 1; i <= 5; i++)
        {
            var star = new Label();
            star.Text = i <= pattern.Difficulty ? "★" : "☆";
            star.AddThemeColorOverride("font_color", i <= pattern.Difficulty ? 
                new Color(1.0f, 0.8f, 0.2f) : new Color(0.4f, 0.4f, 0.4f));
            difficultyContainer.AddChild(star);
        }
        cardContainer.AddChild(difficultyContainer);
        
        // Description
        var descLabel = new Label();
        descLabel.Text = pattern.Description;
        descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 0.9f));
        descLabel.AddThemeFontSizeOverride("font_size", 9);
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        descLabel.CustomMinimumSize = new Vector2(160, 40);
        cardContainer.AddChild(descLabel);
        
        // Load button
        var loadButton = new Button();
        loadButton.Text = "Load Pattern";
        loadButton.AddThemeFontSizeOverride("font_size", 10);
        loadButton.CustomMinimumSize = new Vector2(0, 28);
        loadButton.Pressed += () => ApplyPattern(pattern);
        cardContainer.AddChild(loadButton);
        
        // Hover effects
        card.MouseEntered += () => {
            cardStyle.BorderColor = new Color(0.5f, 0.7f, 1.0f, 0.9f);
            card.AddThemeStyleboxOverride("panel", cardStyle);
        };
        card.MouseExited += () => {
            cardStyle.BorderColor = new Color(0.3f, 0.4f, 0.6f, 0.6f);
            card.AddThemeStyleboxOverride("panel", cardStyle);
        };
    }
    
    private float[,] GeneratePreviewPattern(LeniaPattern pattern)
    {
        var preview = new float[32, 32];
        var random = new System.Random(pattern.Name.GetHashCode());
        
        // Generate a small preview based on pattern characteristics
        switch (pattern.Category)
        {
            case PatternCategory.Creatures:
                CreateCreaturePreview(preview, random);
                break;
            case PatternCategory.Oscillators:
                CreateOscillatorPreview(preview, random);
                break;
            case PatternCategory.Generators:
                CreateGeneratorPreview(preview, random);
                break;
            default:
                CreateGenericPreview(preview, random);
                break;
        }
        
        return preview;
    }
    
    private void CreateCreaturePreview(float[,] preview, System.Random random)
    {
        int centerX = 16, centerY = 16;
        for (int i = 0; i < 5; i++)
        {
            int x = centerX + random.Next(-8, 9);
            int y = centerY + random.Next(-8, 9);
            if (x >= 0 && x < 32 && y >= 0 && y < 32)
            {
                preview[x, y] = 0.8f + (float)random.NextDouble() * 0.2f;
            }
        }
    }
    
    private void CreateOscillatorPreview(float[,] preview, System.Random random)
    {
        int centerX = 16, centerY = 16;
        for (int r = 5; r <= 10; r += 2)
        {
            for (int angle = 0; angle < 360; angle += 30)
            {
                int x = centerX + (int)(r * Mathf.Cos(angle * Mathf.Pi / 180));
                int y = centerY + (int)(r * Mathf.Sin(angle * Mathf.Pi / 180));
                if (x >= 0 && x < 32 && y >= 0 && y < 32)
                {
                    preview[x, y] = 0.6f + (float)random.NextDouble() * 0.3f;
                }
            }
        }
    }
    
    private void CreateGeneratorPreview(float[,] preview, System.Random random)
    {
        int centerX = 16, centerY = 16;
        preview[centerX, centerY] = 1.0f;
        
        for (int r = 1; r < 10; r++)
        {
            float intensity = 1.0f - (r / 10.0f);
            for (int angle = 0; angle < 360; angle += 45)
            {
                int x = centerX + (int)(r * Mathf.Cos(angle * Mathf.Pi / 180));
                int y = centerY + (int)(r * Mathf.Sin(angle * Mathf.Pi / 180));
                if (x >= 0 && x < 32 && y >= 0 && y < 32)
                {
                    preview[x, y] = intensity * (0.5f + (float)random.NextDouble() * 0.3f);
                }
            }
        }
    }
    
    private void CreateGenericPreview(float[,] preview, System.Random random)
    {
        for (int x = 10; x < 22; x++)
        {
            for (int y = 10; y < 22; y++)
            {
                if (random.NextDouble() < 0.3)
                {
                    preview[x, y] = (float)random.NextDouble() * 0.8f;
                }
            }
        }
    }
    
    private void ApplyPattern(LeniaPattern pattern)
    {
        if (simulation == null) return;
        
        // Apply pattern parameters to simulation
        simulation.DeltaTime = pattern.Parameters.DeltaTime;
        simulation.KernelRadius = pattern.Parameters.KernelRadius;
        simulation.GrowthMean = pattern.Parameters.GrowthMean;
        simulation.GrowthSigma = pattern.Parameters.GrowthSigma;
        simulation.CurrentColorScheme = pattern.Parameters.ColorScheme;
        
        // Recreate kernel with new parameters
        simulation.CreateKernel();
        
        // Apply the pattern
        pattern.ApplyAction?.Invoke(simulation);
        
        EmitSignal(SignalName.PatternSelected, pattern.Id);
    }
    
    private void OnSearchTextChanged(string newText)
    {
        RefreshAllCategories();
    }
    
    private void OnDifficultyFilterChanged(long index)
    {
        RefreshAllCategories();
    }
    
    private string GetCategoryDisplayName(PatternCategory category)
    {
        return category switch
        {
            PatternCategory.Classic => "Classic",
            PatternCategory.Creatures => "Creatures",
            PatternCategory.Oscillators => "Oscillators",
            PatternCategory.Generators => "Generators",
            PatternCategory.Experimental => "Experimental",
            PatternCategory.UserSaved => "My Patterns",
            _ => category.ToString()
        };
    }
    
    private StyleBox CreateHeaderStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.2f, 0.3f, 0.5f, 0.8f);
        style.SetCornerRadiusAll(6);
        style.SetBorderWidthAll(1);
        style.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.6f);
        return style;
    }
    
    // Pattern creation methods
    private void CreateGliderPattern(LeniaSimulation sim)
    {
        var grid = sim.GetCurrentGrid();
        int centerX = sim.GridWidth / 2;
        int centerY = sim.GridHeight / 2;
        
        // Create a simple glider shape
        for (int x = 0; x < sim.GridWidth; x++)
        {
            for (int y = 0; y < sim.GridHeight; y++)
            {
                grid[x, y] = 0.0f;
            }
        }
        
        // Simple asymmetric blob that will glide
        for (int dx = -3; dx <= 5; dx++)
        {
            for (int dy = -2; dy <= 3; dy++)
            {
                int x = centerX + dx;
                int y = centerY + dy;
                if (x >= 0 && x < sim.GridWidth && y >= 0 && y < sim.GridHeight)
                {
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance < 4.0f)
                    {
                        float asymmetry = (dx + 3) / 8.0f; // Creates directional bias
                        grid[x, y] = (1.0f - distance / 4.0f) * (0.5f + asymmetry * 0.4f);
                    }
                }
            }
        }
    }
    
    private void CreateAsymptoticOrbium(LeniaSimulation sim)
    {
        sim.OrbiumPattern();
        // Add trailing effect by modifying parameters
        sim.DeltaTime = 0.12f;
        sim.GrowthSigma = 0.012f;
    }
    
    private void CreatePulseGenerator(LeniaSimulation sim)
    {
        var grid = sim.GetCurrentGrid();
        int centerX = sim.GridWidth / 2;
        int centerY = sim.GridHeight / 2;
        
        for (int x = 0; x < sim.GridWidth; x++)
        {
            for (int y = 0; y < sim.GridHeight; y++)
            {
                grid[x, y] = 0.0f;
            }
        }
        
        // Central pulsing core
        grid[centerX, centerY] = 1.0f;
        for (int r = 1; r <= 3; r++)
        {
            for (int angle = 0; angle < 360; angle += 45)
            {
                int x = centerX + (int)(r * Mathf.Cos(angle * Mathf.Pi / 180));
                int y = centerY + (int)(r * Mathf.Sin(angle * Mathf.Pi / 180));
                if (x >= 0 && x < sim.GridWidth && y >= 0 && y < sim.GridHeight)
                {
                    grid[x, y] = 1.0f - (r / 3.0f) * 0.3f;
                }
            }
        }
    }
    
    private void CreateSpiralOscillator(LeniaSimulation sim)
    {
        var grid = sim.GetCurrentGrid();
        int centerX = sim.GridWidth / 2;
        int centerY = sim.GridHeight / 2;
        
        for (int x = 0; x < sim.GridWidth; x++)
        {
            for (int y = 0; y < sim.GridHeight; y++)
            {
                grid[x, y] = 0.0f;
            }
        }
        
        // Create spiral pattern
        for (int r = 5; r <= 25; r += 2)
        {
            for (int angle = 0; angle < 360; angle += 15)
            {
                float spiralAngle = angle + r * 10; // Creates spiral offset
                int x = centerX + (int)(r * Mathf.Cos(spiralAngle * Mathf.Pi / 180));
                int y = centerY + (int)(r * Mathf.Sin(spiralAngle * Mathf.Pi / 180));
                if (x >= 0 && x < sim.GridWidth && y >= 0 && y < sim.GridHeight)
                {
                    grid[x, y] = 0.6f + Mathf.Sin(r * 0.3f) * 0.3f;
                }
            }
        }
    }
    
    private void CreateBreathingCell(LeniaSimulation sim)
    {
        var grid = sim.GetCurrentGrid();
        int centerX = sim.GridWidth / 2;
        int centerY = sim.GridHeight / 2;
        
        for (int x = 0; x < sim.GridWidth; x++)
        {
            for (int y = 0; y < sim.GridHeight; y++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distance <= 15.0f)
                {
                    float intensity = 1.0f - (distance / 15.0f);
                    grid[x, y] = intensity * 0.7f;
                }
                else
                {
                    grid[x, y] = 0.0f;
                }
            }
        }
    }
    
    private void CreateChaosSeed(LeniaSimulation sim)
    {
        var grid = sim.GetCurrentGrid();
        var random = new RandomNumberGenerator();
        random.Randomize();
        
        int centerX = sim.GridWidth / 2;
        int centerY = sim.GridHeight / 2;
        
        for (int x = 0; x < sim.GridWidth; x++)
        {
            for (int y = 0; y < sim.GridHeight; y++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distance <= 20.0f && random.Randf() < 0.4f)
                {
                    grid[x, y] = random.Randf() * 0.9f;
                }
                else
                {
                    grid[x, y] = 0.0f;
                }
            }
        }
    }
    
    private void CreateWaveCollision(LeniaSimulation sim)
    {
        var grid = sim.GetCurrentGrid();
        int centerX = sim.GridWidth / 2;
        int centerY = sim.GridHeight / 2;
        
        for (int x = 0; x < sim.GridWidth; x++)
        {
            for (int y = 0; y < sim.GridHeight; y++)
            {
                grid[x, y] = 0.0f;
            }
        }
        
        // Two wave sources
        int source1X = centerX - 30;
        int source1Y = centerY;
        int source2X = centerX + 30;
        int source2Y = centerY;
        
        // Create initial wave rings
        for (int r = 5; r <= 15; r += 5)
        {
            CreateWaveRing(grid, source1X, source1Y, r, sim.GridWidth, sim.GridHeight);
            CreateWaveRing(grid, source2X, source2Y, r, sim.GridWidth, sim.GridHeight);
        }
    }
    
    private void CreateWaveRing(float[,] grid, int centerX, int centerY, int radius, int gridWidth, int gridHeight)
    {
        for (int angle = 0; angle < 360; angle += 10)
        {
            int x = centerX + (int)(radius * Mathf.Cos(angle * Mathf.Pi / 180));
            int y = centerY + (int)(radius * Mathf.Sin(angle * Mathf.Pi / 180));
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                grid[x, y] = 0.6f;
            }
        }
    }
}