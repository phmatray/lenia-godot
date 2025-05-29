using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Gallery : Control
{
    private GridContainer imageGrid;
    private Label statusLabel;
    private Button backButton;
    private Button clearButton;
    private Panel background;
    
    private List<string> imageFiles = new List<string>();
    
    public override void _Ready()
    {
        // Get node references
        imageGrid = GetNode<GridContainer>("VBoxContainer/ScrollContainer/ImageGrid");
        statusLabel = GetNode<Label>("VBoxContainer/StatusContainer/StatusLabel");
        backButton = GetNode<Button>("VBoxContainer/HeaderContainer/BackButton");
        clearButton = GetNode<Button>("VBoxContainer/StatusContainer/ClearButton");
        background = GetNode<Panel>("Background");
        
        // Style the background
        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(0.05f, 0.08f, 0.12f, 1.0f);
        background.AddThemeStyleboxOverride("panel", bgStyle);
        
        // Style buttons
        StyleButton(backButton);
        StyleButton(clearButton);
        
        // Connect signals
        backButton.Pressed += OnBackPressed;
        clearButton.Pressed += OnClearPressed;
        
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
                    imageFiles.Add(screenshotsDir + fileName);
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        
        // Sort by filename (which includes timestamp)
        imageFiles.Sort();
        imageFiles.Reverse(); // Show newest first
        
        if (imageFiles.Count == 0)
        {
            statusLabel.Text = "No screenshots found. Take some screenshots in the simulation!";
            return;
        }
        
        statusLabel.Text = $"Found {imageFiles.Count} screenshot{(imageFiles.Count == 1 ? "" : "s")}";
        
        // Create image thumbnails
        CreateImageThumbnails();
    }
    
    private void CreateImageThumbnails()
    {
        foreach (var imagePath in imageFiles)
        {
            CreateImageThumbnail(imagePath);
        }
    }
    
    private void CreateImageThumbnail(string imagePath)
    {
        // Create container for thumbnail
        var container = new Panel();
        container.CustomMinimumSize = new Vector2(250, 200);
        
        var containerStyle = new StyleBoxFlat();
        containerStyle.BgColor = new Color(0.1f, 0.12f, 0.18f, 0.9f);
        containerStyle.SetBorderWidthAll(2);
        containerStyle.BorderColor = new Color(0.2f, 0.3f, 0.5f, 0.6f);
        containerStyle.SetCornerRadiusAll(8);
        container.AddThemeStyleboxOverride("panel", containerStyle);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 8);
        container.AddChild(vbox);
        
        // Create image button
        var imageButton = new TextureButton();
        imageButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        imageButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        
        // Load image
        var image = Image.LoadFromFile(imagePath);
        if (image != null)
        {
            var texture = ImageTexture.CreateFromImage(image);
            imageButton.TextureNormal = texture;
        }
        
        // Connect click to open full size
        imageButton.Pressed += () => OpenFullsizeImage(imagePath);
        
        vbox.AddChild(imageButton);
        
        // Add filename label
        var filenameLabel = new Label();
        var filename = imagePath.GetFile().Replace("lenia_screenshot_", "").Replace(".png", "");
        filenameLabel.Text = filename;
        filenameLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 0.9f));
        filenameLabel.AddThemeFontSizeOverride("font_size", 11);
        filenameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        filenameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vbox.AddChild(filenameLabel);
        
        // Add delete button
        var deleteButton = new Button();
        deleteButton.Text = "Delete";
        deleteButton.AddThemeFontSizeOverride("font_size", 10);
        deleteButton.CustomMinimumSize = new Vector2(0, 25);
        deleteButton.Pressed += () => DeleteImage(imagePath, container);
        vbox.AddChild(deleteButton);
        
        imageGrid.AddChild(container);
    }
    
    private void OpenFullsizeImage(string imagePath)
    {
        // Create fullscreen overlay
        var overlay = new Control();
        overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        overlay.MouseFilter = Control.MouseFilterEnum.Stop;
        
        var overlayBg = new ColorRect();
        overlayBg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        overlayBg.Color = new Color(0, 0, 0, 0.8f);
        overlay.AddChild(overlayBg);
        
        // Add full size image
        var textureRect = new TextureRect();
        textureRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
        textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        textureRect.AnchorTop = 0.05f;
        textureRect.AnchorBottom = 0.85f;
        
        var image = Image.LoadFromFile(imagePath);
        if (image != null)
        {
            var texture = ImageTexture.CreateFromImage(image);
            textureRect.Texture = texture;
        }
        
        overlay.AddChild(textureRect);
        
        // Add close instruction
        var closeLabel = new Label();
        closeLabel.Text = "Click anywhere to close";
        closeLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomLeft);
        closeLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        closeLabel.AddThemeFontSizeOverride("font_size", 16);
        closeLabel.HorizontalAlignment = HorizontalAlignment.Center;
        closeLabel.AnchorRight = 1.0f;
        closeLabel.AnchorTop = 0.9f;
        overlay.AddChild(closeLabel);
        
        // Close on click
        overlay.GuiInput += (InputEvent @event) => {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                overlay.QueueFree();
            }
        };
        
        GetTree().CurrentScene.AddChild(overlay);
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
                container.QueueFree();
                imageFiles.Remove(imagePath);
                statusLabel.Text = $"Found {imageFiles.Count} screenshot{(imageFiles.Count == 1 ? "" : "s")}";
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
        if (imageFiles.Count == 0) return;
        
        var dialog = new AcceptDialog();
        dialog.DialogText = $"Delete ALL {imageFiles.Count} screenshots?\\nThis cannot be undone!";
        dialog.Title = "Confirm Clear All";
        dialog.Size = new Vector2I(400, 150);
        
        dialog.AddCancelButton("Cancel");
        
        dialog.Confirmed += () => {
            foreach (var imagePath in imageFiles.ToList())
            {
                DirAccess.RemoveAbsolute(imagePath);
            }
            
            // Clear the grid
            foreach (Node child in imageGrid.GetChildren())
            {
                child.QueueFree();
            }
            
            imageFiles.Clear();
            statusLabel.Text = "All screenshots deleted.";
        };
        
        GetTree().CurrentScene.AddChild(dialog);
        dialog.PopupCentered();
    }
}