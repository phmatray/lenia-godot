using Godot;
using System.Collections.Generic;
using System.Linq;

public class ScreenshotMetadata
{
    public string Timestamp { get; set; }
    public string GridSize { get; set; }
    public float DeltaTime { get; set; }
    public float KernelRadius { get; set; }
    public float GrowthMean { get; set; }
    public float GrowthSigma { get; set; }
    public float SimulationSpeed { get; set; }
    public string ColorScheme { get; set; }
    public float BrushSize { get; set; }
    public float BrushIntensity { get; set; }
    public float PopulationPercent { get; set; }
    public float TotalPopulation { get; set; }
}

public partial class Gallery : Control
{
    private GridContainer imageGrid;
    private Label statusLabel;
    private Label countLabel;
    private Button backButton;
    private Button clearButton;
    private Button exportButton;
    private Panel background;
    private Panel headerBar;
    private Panel leftSidebar;
    private LineEdit searchInput;
    private OptionButton sortOption;
    private HSlider gridSizeSlider;
    private Label gridSizeLabel;
    
    private List<string> allImageFiles = new List<string>();
    private List<string> filteredImageFiles = new List<string>();
    
    private enum SortMode
    {
        NewestFirst,
        OldestFirst,
        NameAZ,
        NameZA
    }
    
    public override void _Ready()
    {
        try
        {
            // Get node references
            imageGrid = GetNode<GridContainer>("VBoxContainer/MiddleSection/MainContent/ContentMargin/ImageGrid");
            statusLabel = GetNode<Label>("VBoxContainer/MiddleSection/LeftSidebar/SidebarMargin/SidebarContent/StatusSection/StatusLabel");
            countLabel = GetNode<Label>("VBoxContainer/HeaderBar/HeaderContent/StatsContainer/CountLabel");
            backButton = GetNode<Button>("VBoxContainer/HeaderBar/HeaderContent/BackButton");
            clearButton = GetNode<Button>("VBoxContainer/HeaderBar/HeaderContent/ActionButtons/ClearButton");
            exportButton = GetNode<Button>("VBoxContainer/HeaderBar/HeaderContent/ActionButtons/ExportButton");
            background = GetNode<Panel>("Background");
            headerBar = GetNode<Panel>("VBoxContainer/HeaderBar");
            leftSidebar = GetNode<Panel>("VBoxContainer/MiddleSection/LeftSidebar");
            searchInput = GetNode<LineEdit>("VBoxContainer/MiddleSection/LeftSidebar/SidebarMargin/SidebarContent/FiltersSection/SearchContainer/SearchInput");
            sortOption = GetNode<OptionButton>("VBoxContainer/MiddleSection/LeftSidebar/SidebarMargin/SidebarContent/FiltersSection/SortContainer/SortOption");
            gridSizeSlider = GetNode<HSlider>("VBoxContainer/MiddleSection/LeftSidebar/SidebarMargin/SidebarContent/ViewSection/GridContainer/GridControls/GridSizeSlider");
            gridSizeLabel = GetNode<Label>("VBoxContainer/MiddleSection/LeftSidebar/SidebarMargin/SidebarContent/ViewSection/GridContainer/GridControls/GridSizeLabel");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Error getting node references in Gallery: {e.Message}");
            return;
        }
        
        // Style the background
        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(0.05f, 0.08f, 0.12f, 1.0f);
        background.AddThemeStyleboxOverride("panel", bgStyle);
        
        // Style header bar to match main UI
        var headerStyle = new StyleBoxFlat();
        headerStyle.BgColor = new Color(0.12f, 0.15f, 0.22f, 0.95f);
        headerStyle.BorderWidthBottom = 2;
        headerStyle.BorderColor = new Color(0.2f, 0.4f, 0.8f, 0.4f);
        headerBar.AddThemeStyleboxOverride("panel", headerStyle);
        
        // Style left sidebar to match main UI
        var sidebarStyle = new StyleBoxFlat();
        sidebarStyle.BgColor = new Color(0.08f, 0.1f, 0.15f, 0.95f);
        sidebarStyle.BorderWidthRight = 2;
        sidebarStyle.BorderColor = new Color(0.2f, 0.4f, 0.8f, 0.3f);
        leftSidebar.AddThemeStyleboxOverride("panel", sidebarStyle);
        
        // Style buttons
        StyleButton(backButton);
        StyleButton(clearButton);
        StyleButton(exportButton);
        
        // Setup controls
        SetupSortOptions();
        SetupSearchAndFilters();
        
        // Connect signals
        backButton.Pressed += OnBackPressed;
        clearButton.Pressed += OnClearPressed;
        exportButton.Pressed += OnExportPressed;
        searchInput.TextChanged += OnSearchChanged;
        sortOption.ItemSelected += OnSortChanged;
        gridSizeSlider.ValueChanged += OnGridSizeChanged;
        
        // Load images
        LoadGalleryImages();
    }
    
    private void StyleButton(Button button)
    {
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.2f, 0.3f, 0.5f, 0.8f);
        buttonStyle.SetBorderWidthAll(1);
        buttonStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f);
        buttonStyle.SetCornerRadiusAll(6);
        button.AddThemeStyleboxOverride("normal", buttonStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.3f, 0.4f, 0.6f, 0.9f);
        hoverStyle.SetBorderWidthAll(1);
        hoverStyle.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        hoverStyle.SetCornerRadiusAll(6);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
    }
    
    private void SetupSortOptions()
    {
        sortOption.AddItem("Newest First");
        sortOption.AddItem("Oldest First");
        sortOption.AddItem("Name A-Z");
        sortOption.AddItem("Name Z-A");
        sortOption.Selected = 0;
    }
    
    private void SetupSearchAndFilters()
    {
        // Style search input
        var searchStyle = new StyleBoxFlat();
        searchStyle.BgColor = new Color(0.15f, 0.18f, 0.25f, 0.9f);
        searchStyle.SetBorderWidthAll(1);
        searchStyle.BorderColor = new Color(0.3f, 0.4f, 0.6f, 0.6f);
        searchStyle.SetCornerRadiusAll(4);
        searchInput.AddThemeStyleboxOverride("normal", searchStyle);
        searchInput.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        
        // Style sort option
        var sortStyle = new StyleBoxFlat();
        sortStyle.BgColor = new Color(0.15f, 0.18f, 0.25f, 0.9f);
        sortStyle.SetBorderWidthAll(1);
        sortStyle.BorderColor = new Color(0.3f, 0.4f, 0.6f, 0.6f);
        sortStyle.SetCornerRadiusAll(4);
        sortOption.AddThemeStyleboxOverride("normal", sortStyle);
        
        // Update grid size label
        gridSizeLabel.Text = gridSizeSlider.Value.ToString();
        imageGrid.Columns = (int)gridSizeSlider.Value;
    }
    
    private void LoadGalleryImages()
    {
        var screenshotsDir = "user://screenshots/";
        if (!DirAccess.DirExistsAbsolute(screenshotsDir))
        {
            statusLabel.Text = "No screenshots found. Take some screenshots in the simulation!";
            return;
        }
        
        var dir = DirAccess.Open(screenshotsDir);
        if (dir != null)
        {
            dir.ListDirBegin();
            var fileName = dir.GetNext();
            
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && fileName.ToLower().EndsWith(".png"))
                {
                    allImageFiles.Add(screenshotsDir + fileName);
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        
        if (allImageFiles.Count == 0)
        {
            statusLabel.Text = "No screenshots found.\\nTake some screenshots in the simulation!";
            countLabel.Text = "0 screenshots";
            return;
        }
        
        // Apply initial sorting and filtering
        ApplyFiltersAndSort();
        RefreshGallery();
    }
    
    private void ApplyFiltersAndSort()
    {
        var searchText = searchInput.Text.ToLower().Trim();
        
        // Filter by search text
        if (string.IsNullOrEmpty(searchText))
        {
            filteredImageFiles = new List<string>(allImageFiles);
        }
        else
        {
            filteredImageFiles = allImageFiles.Where(path => 
                path.GetFile().ToLower().Contains(searchText)
            ).ToList();
        }
        
        // Apply sorting
        var sortMode = (SortMode)sortOption.Selected;
        switch (sortMode)
        {
            case SortMode.NewestFirst:
                filteredImageFiles.Sort();
                filteredImageFiles.Reverse();
                break;
            case SortMode.OldestFirst:
                filteredImageFiles.Sort();
                break;
            case SortMode.NameAZ:
                filteredImageFiles = filteredImageFiles.OrderBy(path => path.GetFile()).ToList();
                break;
            case SortMode.NameZA:
                filteredImageFiles = filteredImageFiles.OrderByDescending(path => path.GetFile()).ToList();
                break;
        }
        
        // Update status
        if (filteredImageFiles.Count == 0 && !string.IsNullOrEmpty(searchText))
        {
            statusLabel.Text = $"No screenshots match '{searchText}'";
            countLabel.Text = $"{allImageFiles.Count} total";
        }
        else
        {
            statusLabel.Text = $"Showing {filteredImageFiles.Count} of {allImageFiles.Count}";
            countLabel.Text = $"{allImageFiles.Count} screenshot{(allImageFiles.Count == 1 ? "" : "s")}";
        }
    }
    
    private void RefreshGallery()
    {
        // Clear existing thumbnails
        foreach (Node child in imageGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        // Create new thumbnails
        foreach (var imagePath in filteredImageFiles)
        {
            CreateImageThumbnail(imagePath);
        }
    }
    
    private void OnSearchChanged(string newText)
    {
        ApplyFiltersAndSort();
        RefreshGallery();
    }
    
    private void OnSortChanged(long index)
    {
        ApplyFiltersAndSort();
        RefreshGallery();
    }
    
    private void OnGridSizeChanged(double value)
    {
        var gridSize = (int)value;
        gridSizeLabel.Text = gridSize.ToString();
        imageGrid.Columns = gridSize;
        
        // Adjust thumbnail size based on grid size
        var thumbnailSize = GetThumbnailSize(gridSize);
        UpdateThumbnailSizes(thumbnailSize);
    }
    
    private Vector2 GetThumbnailSize(int gridColumns)
    {
        // Calculate thumbnail size based on available space and grid columns
        var viewportWidth = GetViewport().GetVisibleRect().Size.X;
        var sidebarWidth = 280; // Fixed sidebar width from gallery.tscn
        var margins = 40; // Content margins (20 left + 20 right)
        var gridSpacing = 20 * (gridColumns - 1); // Spacing between grid items
        
        var availableWidth = viewportWidth - sidebarWidth - margins - gridSpacing;
        var thumbnailWidth = availableWidth / gridColumns;
        var thumbnailHeight = thumbnailWidth + 140; // Add space for metadata display
        
        // Ensure minimum and maximum thumbnail sizes for good UX
        thumbnailWidth = Mathf.Clamp(thumbnailWidth, 250, 400);
        thumbnailHeight = Mathf.Clamp(thumbnailHeight, 350, 540);
        
        return new Vector2(thumbnailWidth, thumbnailHeight);
    }
    
    private void UpdateThumbnailSizes(Vector2 newSize)
    {
        foreach (Node child in imageGrid.GetChildren())
        {
            if (child is Panel panel)
            {
                panel.CustomMinimumSize = newSize;
            }
        }
    }
    
    private void CreateImageThumbnail(string imagePath)
    {
        // Load metadata
        var metadata = LoadMetadata(imagePath);
        
        // Create container for thumbnail
        var container = new Panel();
        var thumbnailSize = GetThumbnailSize(imageGrid.Columns);
        container.CustomMinimumSize = thumbnailSize;
        
        var containerStyle = new StyleBoxFlat();
        containerStyle.BgColor = new Color(0.08f, 0.1f, 0.15f, 0.95f);
        containerStyle.SetBorderWidthAll(2);
        containerStyle.BorderColor = new Color(0.2f, 0.3f, 0.5f, 0.8f);
        containerStyle.SetCornerRadiusAll(8);
        container.AddThemeStyleboxOverride("panel", containerStyle);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 10);
        vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        container.AddChild(vbox);
        
        // Add margin for better spacing
        var marginContainer = new MarginContainer();
        marginContainer.AddThemeConstantOverride("margin_left", 12);
        marginContainer.AddThemeConstantOverride("margin_top", 12);
        marginContainer.AddThemeConstantOverride("margin_right", 12);
        marginContainer.AddThemeConstantOverride("margin_bottom", 12);
        marginContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        vbox.AddChild(marginContainer);
        
        var contentVBox = new VBoxContainer();
        contentVBox.AddThemeConstantOverride("separation", 8);
        contentVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        marginContainer.AddChild(contentVBox);
        
        // Create image display area
        var imageContainer = new AspectRatioContainer();
        imageContainer.Ratio = 1.0f; // Square aspect ratio
        imageContainer.StretchMode = AspectRatioContainer.StretchModeEnum.Fit;
        imageContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        
        var imageButton = new TextureButton();
        imageButton.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        
        // Load and scale image
        var image = Image.LoadFromFile(imagePath);
        if (image != null)
        {
            // Scale image to a reasonable thumbnail size
            var targetSize = (int)(thumbnailSize.X - 40); // Account for margins
            image.Resize(targetSize, targetSize, Image.Interpolation.Lanczos);
            var texture = ImageTexture.CreateFromImage(image);
            imageButton.TextureNormal = texture;
        }
        
        // Connect click to open full size
        imageButton.Pressed += () => OpenFullsizeImage(imagePath);
        
        imageContainer.AddChild(imageButton);
        contentVBox.AddChild(imageContainer);
        
        // Create metadata panel
        var metadataPanel = new Panel();
        var metaStyle = new StyleBoxFlat();
        metaStyle.BgColor = new Color(0.05f, 0.07f, 0.12f, 0.9f);
        metaStyle.SetBorderWidthAll(1);
        metaStyle.BorderColor = new Color(0.15f, 0.2f, 0.3f, 0.8f);
        metaStyle.SetCornerRadiusAll(4);
        metadataPanel.AddThemeStyleboxOverride("panel", metaStyle);
        metadataPanel.CustomMinimumSize = new Vector2(0, 100);
        metadataPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentVBox.AddChild(metadataPanel);
        
        var metaMargin = new MarginContainer();
        metaMargin.AddThemeConstantOverride("margin_left", 8);
        metaMargin.AddThemeConstantOverride("margin_top", 6);
        metaMargin.AddThemeConstantOverride("margin_right", 8);
        metaMargin.AddThemeConstantOverride("margin_bottom", 6);
        metaMargin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        metadataPanel.AddChild(metaMargin);
        
        var metaVBox = new VBoxContainer();
        metaVBox.AddThemeConstantOverride("separation", 2);
        metaVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        metaMargin.AddChild(metaVBox);
        
        // Add metadata information - keep text concise to prevent wrapping
        if (metadata != null)
        {
            var timestamp = metadata.Timestamp?.Length > 16 ? metadata.Timestamp.Substring(11, 5) : metadata.Timestamp ?? "Unknown";
            AddMetadataLine(metaVBox, "📅 " + timestamp, new Color(0.9f, 0.9f, 1.0f), 11);
            AddMetadataLine(metaVBox, $"🎯 Pop: {metadata.PopulationPercent:F1}%", new Color(0.8f, 1.0f, 0.8f), 10);
            AddMetadataLine(metaVBox, "📏 " + (metadata.GridSize ?? "N/A"), new Color(0.8f, 0.9f, 1.0f), 10);
            AddMetadataLine(metaVBox, "🎨 " + (metadata.ColorScheme ?? "N/A"), new Color(1.0f, 0.9f, 0.8f), 10);
        }
        else
        {
            AddMetadataLine(metaVBox, "📅 Unknown", new Color(0.9f, 0.9f, 1.0f), 11);
            AddMetadataLine(metaVBox, "🎯 No data", new Color(0.8f, 1.0f, 0.8f), 10);
            AddMetadataLine(metaVBox, "📏 N/A", new Color(0.8f, 0.9f, 1.0f), 10);
            AddMetadataLine(metaVBox, "🎨 N/A", new Color(1.0f, 0.9f, 0.8f), 10);
        }
        
        // Add action buttons
        var buttonContainer = new HBoxContainer();
        buttonContainer.AddThemeConstantOverride("separation", 8);
        contentVBox.AddChild(buttonContainer);
        
        var viewButton = new Button();
        viewButton.Text = "View";
        viewButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        viewButton.AddThemeFontSizeOverride("font_size", 10);
        viewButton.Pressed += () => OpenFullsizeImage(imagePath);
        buttonContainer.AddChild(viewButton);
        
        var deleteButton = new Button();
        deleteButton.Text = "Delete";
        deleteButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        deleteButton.AddThemeFontSizeOverride("font_size", 10);
        deleteButton.Pressed += () => DeleteImage(imagePath, container);
        buttonContainer.AddChild(deleteButton);
        
        imageGrid.AddChild(container);
    }
    
    private ScreenshotMetadata LoadMetadata(string imagePath)
    {
        var metadataPath = imagePath.Replace(".png", ".json");
        if (!FileAccess.FileExists(metadataPath))
            return null;
            
        var file = FileAccess.Open(metadataPath, FileAccess.ModeFlags.Read);
        if (file == null) return null;
        
        var jsonString = file.GetAsText();
        file.Close();
        
        var json = new Json();
        var parseResult = json.Parse(jsonString);
        if (parseResult != Error.Ok) return null;
        
        var data = json.Data.AsGodotDictionary();
        
        return new ScreenshotMetadata
        {
            Timestamp = data.GetValueOrDefault("timestamp", "").AsString(),
            GridSize = data.GetValueOrDefault("grid_size", "").AsString(),
            DeltaTime = data.GetValueOrDefault("delta_time", 0.0).AsSingle(),
            KernelRadius = data.GetValueOrDefault("kernel_radius", 0.0).AsSingle(),
            GrowthMean = data.GetValueOrDefault("growth_mean", 0.0).AsSingle(),
            GrowthSigma = data.GetValueOrDefault("growth_sigma", 0.0).AsSingle(),
            SimulationSpeed = data.GetValueOrDefault("simulation_speed", 0.0).AsSingle(),
            ColorScheme = data.GetValueOrDefault("color_scheme", "").AsString(),
            BrushSize = data.GetValueOrDefault("brush_size", 0.0).AsSingle(),
            BrushIntensity = data.GetValueOrDefault("brush_intensity", 0.0).AsSingle(),
            PopulationPercent = data.GetValueOrDefault("population_percent", 0.0).AsSingle(),
            TotalPopulation = data.GetValueOrDefault("total_population", 0.0).AsSingle()
        };
    }
    
    private void AddMetadataLine(VBoxContainer parent, string text, Color color, int fontSize)
    {
        var label = new Label();
        label.Text = text;
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AutowrapMode = TextServer.AutowrapMode.Off; // Disable wrapping for metadata labels
        label.HorizontalAlignment = HorizontalAlignment.Left;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis; // Add ellipsis if text is too long
        parent.AddChild(label);
    }
    
    private void OpenFullsizeImage(string imagePath)
    {
        var metadata = LoadMetadata(imagePath);
        
        // Create fullscreen overlay
        var overlay = new Control();
        overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        overlay.MouseFilter = Control.MouseFilterEnum.Stop;
        
        var overlayBg = new ColorRect();
        overlayBg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        overlayBg.Color = new Color(0, 0, 0, 0.9f);
        overlay.AddChild(overlayBg);
        
        // Create main container
        var mainContainer = new HBoxContainer();
        mainContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 20);
        overlay.AddChild(mainContainer);
        
        // Add margin
        var leftMargin = new MarginContainer();
        leftMargin.AddThemeConstantOverride("margin_left", 40);
        leftMargin.AddThemeConstantOverride("margin_top", 40);
        leftMargin.AddThemeConstantOverride("margin_right", 20);
        leftMargin.AddThemeConstantOverride("margin_bottom", 40);
        leftMargin.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(leftMargin);
        
        // Add full size image
        var imageContainer = new AspectRatioContainer();
        imageContainer.Ratio = 1.0f;
        imageContainer.StretchMode = AspectRatioContainer.StretchModeEnum.Fit;
        leftMargin.AddChild(imageContainer);
        
        var textureRect = new TextureRect();
        textureRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
        textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        
        var image = Image.LoadFromFile(imagePath);
        if (image != null)
        {
            var texture = ImageTexture.CreateFromImage(image);
            textureRect.Texture = texture;
        }
        
        imageContainer.AddChild(textureRect);
        
        // Add metadata panel
        if (metadata != null)
        {
            var metadataContainer = new MarginContainer();
            metadataContainer.AddThemeConstantOverride("margin_left", 20);
            metadataContainer.AddThemeConstantOverride("margin_top", 40);
            metadataContainer.AddThemeConstantOverride("margin_right", 40);
            metadataContainer.AddThemeConstantOverride("margin_bottom", 40);
            metadataContainer.CustomMinimumSize = new Vector2(350, 0);
            mainContainer.AddChild(metadataContainer);
            
            var metadataPanel = new Panel();
            var metaStyle = new StyleBoxFlat();
            metaStyle.BgColor = new Color(0.08f, 0.1f, 0.15f, 0.95f);
            metaStyle.SetBorderWidthAll(2);
            metaStyle.BorderColor = new Color(0.2f, 0.3f, 0.5f, 0.8f);
            metaStyle.SetCornerRadiusAll(8);
            metadataPanel.AddThemeStyleboxOverride("panel", metaStyle);
            metadataContainer.AddChild(metadataPanel);
            
            var scrollContainer = new ScrollContainer();
            scrollContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            scrollContainer.AddThemeConstantOverride("margin_left", 20);
            scrollContainer.AddThemeConstantOverride("margin_top", 20);
            scrollContainer.AddThemeConstantOverride("margin_right", 20);
            scrollContainer.AddThemeConstantOverride("margin_bottom", 20);
            metadataPanel.AddChild(scrollContainer);
            
            var metaVBox = new VBoxContainer();
            metaVBox.AddThemeConstantOverride("separation", 12);
            scrollContainer.AddChild(metaVBox);
            
            // Title
            AddMetadataSection(metaVBox, "📊 SIMULATION DATA", new Color(0.9f, 0.95f, 1.0f), 16);
            
            // Basic info
            AddMetadataSection(metaVBox, "📅 Date/Time", new Color(0.8f, 0.9f, 1.0f), 14);
            AddMetadataLine(metaVBox, metadata.Timestamp, new Color(0.7f, 0.8f, 0.9f), 12);
            
            AddMetadataSection(metaVBox, "🎯 Population", new Color(0.8f, 1.0f, 0.8f), 14);
            AddMetadataLine(metaVBox, $"Density: {metadata.PopulationPercent:F2}%", new Color(0.7f, 0.9f, 0.7f), 12);
            AddMetadataLine(metaVBox, $"Total: {metadata.TotalPopulation:F0}", new Color(0.7f, 0.9f, 0.7f), 12);
            
            AddMetadataSection(metaVBox, "📏 Grid Settings", new Color(0.8f, 0.9f, 1.0f), 14);
            AddMetadataLine(metaVBox, $"Size: {metadata.GridSize}", new Color(0.7f, 0.8f, 0.9f), 12);
            
            AddMetadataSection(metaVBox, "⚙️ Simulation Parameters", new Color(1.0f, 0.9f, 0.8f), 14);
            AddMetadataLine(metaVBox, $"Delta Time: {metadata.DeltaTime:F3}", new Color(0.9f, 0.8f, 0.7f), 12);
            AddMetadataLine(metaVBox, $"Kernel Radius: {metadata.KernelRadius:F1}", new Color(0.9f, 0.8f, 0.7f), 12);
            AddMetadataLine(metaVBox, $"Growth Mean: {metadata.GrowthMean:F3}", new Color(0.9f, 0.8f, 0.7f), 12);
            AddMetadataLine(metaVBox, $"Growth Sigma: {metadata.GrowthSigma:F4}", new Color(0.9f, 0.8f, 0.7f), 12);
            AddMetadataLine(metaVBox, $"Speed: {metadata.SimulationSpeed:F1}x", new Color(0.9f, 0.8f, 0.7f), 12);
            
            AddMetadataSection(metaVBox, "🎨 Visual Settings", new Color(1.0f, 0.9f, 0.8f), 14);
            AddMetadataLine(metaVBox, $"Color Scheme: {metadata.ColorScheme}", new Color(0.9f, 0.8f, 0.7f), 12);
            
            AddMetadataSection(metaVBox, "🖌️ Brush Settings", new Color(0.9f, 0.8f, 1.0f), 14);
            AddMetadataLine(metaVBox, $"Size: {metadata.BrushSize:F1}", new Color(0.8f, 0.7f, 0.9f), 12);
            AddMetadataLine(metaVBox, $"Intensity: {metadata.BrushIntensity:F1}", new Color(0.8f, 0.7f, 0.9f), 12);
        }
        
        // Add close instruction
        var closeLabel = new Label();
        closeLabel.Text = "Click anywhere to close • ESC to exit";
        closeLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomLeft);
        closeLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        closeLabel.AddThemeFontSizeOverride("font_size", 14);
        closeLabel.HorizontalAlignment = HorizontalAlignment.Center;
        closeLabel.AnchorRight = 1.0f;
        closeLabel.AnchorTop = 0.95f;
        overlay.AddChild(closeLabel);
        
        // Close on click or escape
        overlay.GuiInput += (InputEvent @event) => {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                overlay.QueueFree();
            }
            else if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                overlay.QueueFree();
            }
        };
        
        GetTree().CurrentScene.AddChild(overlay);
    }
    
    private void AddMetadataSection(VBoxContainer parent, string title, Color color, int fontSize)
    {
        var label = new Label();
        label.Text = title;
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        parent.AddChild(label);
        
        // Add a subtle separator
        var separator = new HSeparator();
        separator.AddThemeColorOverride("separator", new Color(0.3f, 0.4f, 0.6f, 0.4f));
        parent.AddChild(separator);
    }
    
    private void DeleteImage(string imagePath, Control container)
    {
        // Create confirmation dialog
        var dialog = new AcceptDialog();
        dialog.DialogText = $"Delete this screenshot?\\n{imagePath.GetFile()}";
        dialog.Title = "Confirm Delete";
        dialog.Size = new Vector2I(400, 150);
        
        dialog.AddCancelButton("Cancel");
        
        dialog.Confirmed += () => {
            var file = FileAccess.Open(imagePath, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                file.Close();
                DirAccess.RemoveAbsolute(imagePath);
                
                // Also delete associated metadata file
                var metadataPath = imagePath.Replace(".png", ".json");
                if (FileAccess.FileExists(metadataPath))
                {
                    DirAccess.RemoveAbsolute(metadataPath);
                }
                
                container.QueueFree();
                allImageFiles.Remove(imagePath);
                filteredImageFiles.Remove(imagePath);
                ApplyFiltersAndSort();
            }
        };
        
        GetTree().CurrentScene.AddChild(dialog);
        dialog.PopupCentered();
    }
    
    private void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://lenia.tscn");
    }
    
    private void OnClearPressed()
    {
        if (allImageFiles.Count == 0) return;
        
        var dialog = new AcceptDialog();
        dialog.DialogText = $"Delete ALL {allImageFiles.Count} screenshots?\\nThis cannot be undone!";
        dialog.Title = "Confirm Clear All";
        dialog.Size = new Vector2I(400, 150);
        
        dialog.AddCancelButton("Cancel");
        
        dialog.Confirmed += () => {
            foreach (var imagePath in allImageFiles.ToList())
            {
                DirAccess.RemoveAbsolute(imagePath);
            }
            
            // Clear the grid
            foreach (Node child in imageGrid.GetChildren())
            {
                child.QueueFree();
            }
            
            allImageFiles.Clear();
            filteredImageFiles.Clear();
            statusLabel.Text = "All screenshots deleted.";
        };
        
        GetTree().CurrentScene.AddChild(dialog);
        dialog.PopupCentered();
    }
    
    private void OnExportPressed()
    {
        if (filteredImageFiles.Count == 0) return;
        
        // Create export directory
        var exportDir = "user://exports/";
        if (!DirAccess.DirExistsAbsolute(exportDir))
        {
            DirAccess.MakeDirRecursiveAbsolute(exportDir);
        }
        
        var timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var exportFolder = exportDir + $"lenia_export_{timestamp}/";
        DirAccess.MakeDirRecursiveAbsolute(exportFolder);
        
        var dialog = new AcceptDialog();
        dialog.DialogText = $"Export {filteredImageFiles.Count} screenshot{(filteredImageFiles.Count == 1 ? "" : "s")} to\\n{exportFolder}?";
        dialog.Title = "Export Screenshots";
        dialog.Size = new Vector2I(500, 150);
        
        dialog.AddCancelButton("Cancel");
        
        dialog.Confirmed += () => {
            var copiedCount = 0;
            foreach (var imagePath in filteredImageFiles)
            {
                var fileName = imagePath.GetFile();
                var destinationPath = exportFolder + fileName;
                
                var sourceFile = FileAccess.Open(imagePath, FileAccess.ModeFlags.Read);
                if (sourceFile != null)
                {
                    var data = sourceFile.GetBuffer((int)sourceFile.GetLength());
                    sourceFile.Close();
                    
                    var destFile = FileAccess.Open(destinationPath, FileAccess.ModeFlags.Write);
                    if (destFile != null)
                    {
                        destFile.StoreBuffer(data);
                        destFile.Close();
                        copiedCount++;
                    }
                }
            }
            
            // Create export summary
            CreateExportSummary(exportFolder, copiedCount);
            
            // Show completion notification
            var completedDialog = new AcceptDialog();
            completedDialog.DialogText = $"Successfully exported {copiedCount} screenshot{(copiedCount == 1 ? "" : "s")}!\\n\\nLocation: {exportFolder}";
            completedDialog.Title = "Export Complete";
            completedDialog.Size = new Vector2I(500, 180);
            GetTree().CurrentScene.AddChild(completedDialog);
            completedDialog.PopupCentered();
        };
        
        GetTree().CurrentScene.AddChild(dialog);
        dialog.PopupCentered();
    }
    
    private void CreateExportSummary(string exportFolder, int fileCount)
    {
        var summaryPath = exportFolder + "export_summary.txt";
        var summaryFile = FileAccess.Open(summaryPath, FileAccess.ModeFlags.Write);
        
        if (summaryFile != null)
        {
            summaryFile.StoreLine("LENIA SCREENSHOT EXPORT SUMMARY");
            summaryFile.StoreLine("================================");
            summaryFile.StoreLine($"Export Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            summaryFile.StoreLine($"Total Files: {fileCount}");
            summaryFile.StoreLine($"Source Application: Lenia Cellular Automaton Simulator");
            summaryFile.StoreLine("");
            summaryFile.StoreLine("Files Exported:");
            summaryFile.StoreLine("--------------");
            
            foreach (var imagePath in filteredImageFiles)
            {
                summaryFile.StoreLine(imagePath.GetFile());
            }
            
            summaryFile.Close();
        }
    }
}