using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Lenia.UI
{
    /// <summary>
    /// Overlay showing all keyboard shortcuts in a modern, organized layout
    /// </summary>
    public partial class KeyboardShortcutsOverlay : Control
    {
        private Panel backgroundDimmer;
        private PanelContainer mainPanel;
        private VBoxContainer contentContainer;
        private Label titleLabel;
        private Button closeButton;
        private ScrollContainer scrollContainer;
        private VBoxContainer shortcutsContainer;
        
        private Dictionary<string, List<ShortcutInfo>> shortcutCategories;
        
        public override void _Ready()
        {
            // Fill the entire viewport
            SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
            Visible = false;
            ZIndex = 999;
            
            // Create semi-transparent background
            backgroundDimmer = new Panel();
            backgroundDimmer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            backgroundDimmer.MouseFilter = MouseFilterEnum.Stop;
            
            var dimmerStyle = new StyleBoxFlat();
            dimmerStyle.BgColor = new Color(0, 0, 0, 0.7f);
            backgroundDimmer.AddThemeStyleboxOverride("panel", dimmerStyle);
            AddChild(backgroundDimmer);
            
            // Main panel with glassmorphic effect
            mainPanel = new PanelContainer();
            mainPanel.CustomMinimumSize = new Vector2(800, 600);
            mainPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            mainPanel.AddThemeStyleboxOverride("panel", ModernTheme.CreateGlassPanel(16.0f, 2.0f));
            AddChild(mainPanel);
            
            // Content container
            contentContainer = new VBoxContainer();
            contentContainer.AddThemeConstantOverride("separation", 16);
            mainPanel.AddChild(contentContainer);
            
            // Header
            var headerContainer = new HBoxContainer();
            contentContainer.AddChild(headerContainer);
            
            titleLabel = new Label();
            titleLabel.Text = "Keyboard Shortcuts";
            titleLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.TextPrimary);
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            headerContainer.AddChild(titleLabel);
            
            closeButton = new Button();
            closeButton.Text = "✕";
            closeButton.CustomMinimumSize = new Vector2(32, 32);
            closeButton.AddThemeStyleboxOverride("normal", ModernTheme.NeumorphicButton.CreateNormal());
            closeButton.AddThemeStyleboxOverride("hover", ModernTheme.NeumorphicButton.CreateHover());
            closeButton.AddThemeStyleboxOverride("pressed", ModernTheme.NeumorphicButton.CreatePressed());
            closeButton.Pressed += Hide;
            headerContainer.AddChild(closeButton);
            
            // Separator
            var separator = new HSeparator();
            separator.AddThemeColorOverride("separation", ModernTheme.Colors.BackgroundSurface);
            contentContainer.AddChild(separator);
            
            // Scroll container for shortcuts
            scrollContainer = new ScrollContainer();
            scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            contentContainer.AddChild(scrollContainer);
            
            shortcutsContainer = new VBoxContainer();
            shortcutsContainer.AddThemeConstantOverride("separation", 24);
            scrollContainer.AddChild(shortcutsContainer);
            
            // Initialize shortcuts
            InitializeShortcuts();
            BuildShortcutsList();
            
            // Connect input handling
            backgroundDimmer.GuiInput += OnBackgroundInput;
        }
        
        private void InitializeShortcuts()
        {
            shortcutCategories = new Dictionary<string, List<ShortcutInfo>>
            {
                ["Simulation Control"] = new List<ShortcutInfo>
                {
                    new ShortcutInfo("Space", "Play/Pause simulation", "Toggle the simulation state"),
                    new ShortcutInfo("R", "Reset simulation", "Clear the grid and reset to initial state"),
                    new ShortcutInfo("S", "Step simulation", "Advance simulation by one frame"),
                    new ShortcutInfo("Ctrl+S", "Save pattern", "Save current pattern to file"),
                    new ShortcutInfo("Ctrl+O", "Load pattern", "Load pattern from file")
                },
                
                ["Drawing Tools"] = new List<ShortcutInfo>
                {
                    new ShortcutInfo("B", "Brush tool", "Paint cells on the grid"),
                    new ShortcutInfo("E", "Eraser tool", "Remove cells from the grid"),
                    new ShortcutInfo("P", "Picker tool", "Sample cell values"),
                    new ShortcutInfo("[", "Decrease brush size", "Make the brush smaller"),
                    new ShortcutInfo("]", "Increase brush size", "Make the brush larger"),
                    new ShortcutInfo("Shift+Drag", "Draw straight line", "Constrain drawing to straight lines")
                },
                
                ["View Controls"] = new List<ShortcutInfo>
                {
                    new ShortcutInfo("Scroll", "Zoom in/out", "Zoom the simulation view"),
                    new ShortcutInfo("Middle Mouse", "Pan view", "Move the camera around"),
                    new ShortcutInfo("F", "Fit to screen", "Reset zoom to fit entire grid"),
                    new ShortcutInfo("G", "Toggle grid", "Show/hide grid lines"),
                    new ShortcutInfo("H", "Toggle UI", "Show/hide user interface")
                },
                
                ["Parameters"] = new List<ShortcutInfo>
                {
                    new ShortcutInfo("1-9", "Quick presets", "Load parameter preset 1-9"),
                    new ShortcutInfo("Shift+1-9", "Save to preset", "Save current parameters to slot"),
                    new ShortcutInfo("Q/A", "Adjust growth μ", "Increase/decrease growth center"),
                    new ShortcutInfo("W/S", "Adjust growth σ", "Increase/decrease growth width"),
                    new ShortcutInfo("E/D", "Adjust kernel radius", "Increase/decrease kernel size")
                },
                
                ["Advanced Features"] = new List<ShortcutInfo>
                {
                    new ShortcutInfo("Ctrl+G", "Open gallery", "Browse pattern gallery"),
                    new ShortcutInfo("Ctrl+T", "Start tutorial", "Launch interactive tutorial"),
                    new ShortcutInfo("Ctrl+R", "Record timelapse", "Start/stop recording"),
                    new ShortcutInfo("Ctrl+P", "Performance stats", "Toggle performance overlay"),
                    new ShortcutInfo("F1", "Help", "Show this shortcuts overlay")
                }
            };
        }
        
        private void BuildShortcutsList()
        {
            foreach (var category in shortcutCategories)
            {
                // Category header
                var categoryContainer = new VBoxContainer();
                categoryContainer.AddThemeConstantOverride("separation", 12);
                shortcutsContainer.AddChild(categoryContainer);
                
                var categoryLabel = new Label();
                categoryLabel.Text = category.Key;
                categoryLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.Primary);
                categoryLabel.AddThemeFontSizeOverride("font_size", 18);
                categoryContainer.AddChild(categoryLabel);
                
                // Shortcuts grid
                var gridContainer = new GridContainer();
                gridContainer.Columns = 3;
                gridContainer.AddThemeConstantOverride("h_separation", 32);
                gridContainer.AddThemeConstantOverride("v_separation", 8);
                categoryContainer.AddChild(gridContainer);
                
                foreach (var shortcut in category.Value)
                {
                    // Shortcut key
                    var keyContainer = new PanelContainer();
                    var keyStyle = new StyleBoxFlat();
                    keyStyle.BgColor = ModernTheme.Colors.BackgroundSurface;
                    keyStyle.SetCornerRadiusAll(4);
                    keyStyle.ContentMarginLeft = 8;
                    keyStyle.ContentMarginRight = 8;
                    keyStyle.ContentMarginTop = 4;
                    keyStyle.ContentMarginBottom = 4;
                    keyContainer.AddThemeStyleboxOverride("panel", keyStyle);
                    
                    var keyLabel = new Label();
                    keyLabel.Text = shortcut.Key;
                    keyLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.TextPrimary);
                    keyLabel.AddThemeFontSizeOverride("font_size", 14);
                    keyLabel.CustomMinimumSize = new Vector2(100, 0);
                    keyContainer.AddChild(keyLabel);
                    gridContainer.AddChild(keyContainer);
                    
                    // Action name
                    var actionLabel = new Label();
                    actionLabel.Text = shortcut.Action;
                    actionLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.TextPrimary);
                    actionLabel.AddThemeFontSizeOverride("font_size", 14);
                    actionLabel.CustomMinimumSize = new Vector2(150, 0);
                    gridContainer.AddChild(actionLabel);
                    
                    // Description
                    var descLabel = new Label();
                    descLabel.Text = shortcut.Description;
                    descLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.TextMuted);
                    descLabel.AddThemeFontSizeOverride("font_size", 12);
                    descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
                    descLabel.CustomMinimumSize = new Vector2(300, 0);
                    gridContainer.AddChild(descLabel);
                }
            }
        }
        
        public new void Show()
        {
            Visible = true;
            
            // Animate entrance
            backgroundDimmer.Modulate = new Color(1, 1, 1, 0);
            mainPanel.Scale = Vector2.One * 0.9f;
            mainPanel.Modulate = new Color(1, 1, 1, 0);
            mainPanel.PivotOffset = mainPanel.Size / 2;
            
            var tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(backgroundDimmer, "modulate:a", 1.0f, 0.2f);
            tween.TweenProperty(mainPanel, "modulate:a", 1.0f, 0.25f);
            tween.TweenProperty(mainPanel, "scale", Vector2.One, 0.25f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
        }
        
        public new void Hide()
        {
            var tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(backgroundDimmer, "modulate:a", 0.0f, 0.15f);
            tween.TweenProperty(mainPanel, "modulate:a", 0.0f, 0.15f);
            tween.TweenProperty(mainPanel, "scale", Vector2.One * 0.9f, 0.15f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.In);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }
        
        private void OnBackgroundInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                Hide();
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (Visible && @event.IsActionPressed("ui_cancel"))
            {
                Hide();
                GetViewport().SetInputAsHandled();
            }
        }
        
        private class ShortcutInfo
        {
            public string Key { get; set; }
            public string Action { get; set; }
            public string Description { get; set; }
            
            public ShortcutInfo(string key, string action, string description)
            {
                Key = key;
                Action = action;
                Description = description;
            }
        }
    }
}