using Godot;
using Lenia.UI;

public partial class UIShowcase : Control
{
    private UIInitializer uiInitializer;
    private AnimationSystem animationSystem;
    private AccessibilityManager accessibilityManager;
    private KeyboardShortcutsOverlay shortcutsOverlay;
    private OnboardingFlow onboardingFlow;
    
    public override void _Ready()
    {
        // Initialize UI systems
        InitializeUISystems();
        
        // Connect showcase buttons (deferred to ensure tooltip system is ready)
        CallDeferred(nameof(ConnectShowcaseButtons));
        
        // Create modern button examples (deferred to ensure UI systems are ready)
        CallDeferred(nameof(CreateModernButtonExamples));
        
        // Apply entrance animations (deferred to ensure UI systems are ready)
        CallDeferred(nameof(ApplyShowcaseAnimations));
    }
    
    private void InitializeUISystems()
    {
        // Create UI initializer
        uiInitializer = new UIInitializer();
        uiInitializer.Name = "UIInitializer";
        AddChild(uiInitializer);
        
        // We need to defer getting references until after UIInitializer's _Ready() has run
        CallDeferred(nameof(GetUISystemReferences));
    }
    
    private void GetUISystemReferences()
    {
        // Get references to systems - they are children of UIInitializer
        animationSystem = uiInitializer.GetNodeOrNull<AnimationSystem>("AnimationSystem");
        accessibilityManager = uiInitializer.GetNodeOrNull<AccessibilityManager>("AccessibilityManager");
        // These are added to root by UIInitializer
        shortcutsOverlay = GetTree().Root.GetNodeOrNull<KeyboardShortcutsOverlay>("KeyboardShortcutsOverlay");
        onboardingFlow = GetTree().Root.GetNodeOrNull<OnboardingFlow>("OnboardingFlow");
    }
    
    private void ConnectShowcaseButtons()
    {
        // Tooltip demo
        var tooltipBtn = GetNode<Button>("VBoxContainer/ButtonsContainer/ShowTooltip");
        // Use TooltipHelper to add tooltips
        TooltipHelper.AddTooltip(tooltipBtn, 
            "Rich Tooltip Example",
            "This is a modern tooltip with multiple content types, animations, and smart positioning.",
            null, // icon texture
            "Ctrl+T");
        
        // Shortcuts overlay
        var shortcutsBtn = GetNode<Button>("VBoxContainer/ButtonsContainer/ShowShortcuts");
        shortcutsBtn.Pressed += () => shortcutsOverlay?.Show();
        
        // Onboarding
        var onboardingBtn = GetNode<Button>("VBoxContainer/ButtonsContainer/StartOnboarding");
        onboardingBtn.Pressed += () => {
            // Reset onboarding state and start
            OS.SetEnvironment("LENIA_ONBOARDING_COMPLETED", "false");
            onboardingFlow?.StartOnboarding();
        };
        
        // Animation demos
        var fadeInBtn = GetNode<Button>("VBoxContainer/AnimationsContainer/AnimButtons/FadeIn");
        fadeInBtn.Pressed += () => {
            var panel = CreateDemoPanel();
            AnimationSystem.AnimateEntrance(panel, AnimationSystem.EntranceType.FadeIn);
        };
        
        var slideInBtn = GetNode<Button>("VBoxContainer/AnimationsContainer/AnimButtons/SlideIn");
        slideInBtn.Pressed += () => {
            var panel = CreateDemoPanel();
            AnimationSystem.AnimateEntrance(panel, AnimationSystem.EntranceType.SlideFromRight);
        };
        
        var bounceBtn = GetNode<Button>("VBoxContainer/AnimationsContainer/AnimButtons/Bounce");
        bounceBtn.Pressed += () => {
            var panel = CreateDemoPanel();
            AnimationSystem.AnimateEntrance(panel, AnimationSystem.EntranceType.Bounce);
        };
        
        var rippleBtn = GetNode<Button>("VBoxContainer/AnimationsContainer/AnimButtons/Ripple");
        rippleBtn.Pressed += () => {
            AnimationSystem.AnimateRipple(rippleBtn, rippleBtn.Size / 2);
        };
        
        // Accessibility options
        var highContrastBtn = GetNode<CheckButton>("VBoxContainer/AccessibilityContainer/AccessButtons/HighContrast");
        highContrastBtn.Toggled += (bool pressed) => accessibilityManager?.SetHighContrastMode(pressed);
        
        var reducedMotionBtn = GetNode<CheckButton>("VBoxContainer/AccessibilityContainer/AccessButtons/ReducedMotion");
        reducedMotionBtn.Toggled += (bool pressed) => accessibilityManager?.SetReducedMotion(pressed);
        
        var largeTextBtn = GetNode<CheckButton>("VBoxContainer/AccessibilityContainer/AccessButtons/LargeText");
        largeTextBtn.Toggled += (bool pressed) => accessibilityManager?.SetFontScale(pressed ? 1.5f : 1.0f);
    }
    
    private void CreateModernButtonExamples()
    {
        var buttonGrid = GetNode<GridContainer>("VBoxContainer/ModernButtons/ButtonGrid");
        
        // Create button variants
        var variants = new[]
        {
            (ModernButton.ButtonVariant.Primary, "Primary", "res://icon.svg"),
            (ModernButton.ButtonVariant.Secondary, "Secondary", null),
            (ModernButton.ButtonVariant.Success, "Success", null),
            (ModernButton.ButtonVariant.Warning, "Warning", null),
            (ModernButton.ButtonVariant.Danger, "Danger", null),
            (ModernButton.ButtonVariant.Ghost, "Ghost", null)
        };
        
        foreach (var (variant, text, icon) in variants)
        {
            var modernBtn = new ModernButton();
            modernBtn.Text = text;
            modernBtn.SetVariant(variant);
            modernBtn.CustomMinimumSize = new Vector2(120, 40);
            
            if (icon != null)
            {
                modernBtn.Icon = GD.Load<Texture2D>(icon);
            }
            
            buttonGrid.AddChild(modernBtn);
            
            // Add click handler for demo
            modernBtn.Pressed += () => {
                GD.Print($"Modern {variant} button clicked!");
                AnimationSystem.AnimatePulse(modernBtn);
            };
        }
    }
    
    private Panel CreateDemoPanel()
    {
        var panel = new Panel();
        panel.CustomMinimumSize = new Vector2(200, 100);
        panel.Position = new Vector2(
            GetViewport().GetVisibleRect().Size.X / 2 - 100,
            GetViewport().GetVisibleRect().Size.Y / 2 - 50
        );
        
        var style = new StyleBoxFlat();
        style.BgColor = ModernTheme.Colors.BackgroundSurface;
        style.BorderWidthLeft = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = ModernTheme.Colors.Primary;
        style.SetCornerRadiusAll(8);
        panel.AddThemeStyleboxOverride("panel", style);
        
        var label = new Label();
        label.Text = "Animated Panel";
        label.AddThemeColorOverride("font_color", ModernTheme.Colors.TextPrimary);
        label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        panel.AddChild(label);
        
        AddChild(panel);
        
        // Auto-remove after 3 seconds
        GetTree().CreateTimer(3.0).Timeout += () => {
            // AnimationSystem doesn't have AnimateExit, so we'll fade out and queue free
            var tween = panel.CreateTween();
            tween.TweenProperty(panel, "modulate:a", 0.0f, 0.3f);
            tween.TweenCallback(Callable.From(() => panel.QueueFree()));
        };
        
        return panel;
    }
    
    private void ApplyShowcaseAnimations()
    {
        // Animate all sections with staggered entrance
        var sections = new Node[]
        {
            GetNode("VBoxContainer/Title"),
            GetNode("VBoxContainer/Description"),
            GetNode("VBoxContainer/ButtonsContainer"),
            GetNode("VBoxContainer/AnimationsContainer"),
            GetNode("VBoxContainer/ModernButtons"),
            GetNode("VBoxContainer/AccessibilityContainer")
        };
        
        for (int i = 0; i < sections.Length; i++)
        {
            if (sections[i] is Control control)
            {
                control.Modulate = new Color(1, 1, 1, 0);
                var delay = i * 0.1f;
                GetTree().CreateTimer(delay).Timeout += () => {
                    AnimationSystem.AnimateEntrance(control, AnimationSystem.EntranceType.FadeScale);
                };
            }
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        // Handle global shortcuts
        if (@event.IsActionPressed("ui_cancel"))
        {
            // Return to main menu
            GetTree().ChangeSceneToFile("res://menu.tscn");
        }
    }
}