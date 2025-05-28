using Godot;

public partial class MenuManager : Control
{
    private VBoxContainer menuContainer;
    private Label titleLabel;
    private Label subtitleLabel;
    private Button startButton;
    private Button aboutButton;
    private Button quitButton;
    private Panel aboutPanel;
    private RichTextLabel aboutText;
    private Button backButton;
    private TextureRect backgroundGradient;
    
    public override void _Ready()
    {
        AnchorLeft = 0;
        AnchorTop = 0;
        AnchorRight = 1;
        AnchorBottom = 1;
        CreateMainMenu();
        CreateAboutPanel();
    }
    
    private void CreateMainMenu()
    {
        // Create gradient background
        backgroundGradient = new TextureRect();
        backgroundGradient.AnchorLeft = 0;
        backgroundGradient.AnchorTop = 0;
        backgroundGradient.AnchorRight = 1;
        backgroundGradient.AnchorBottom = 1;
        backgroundGradient.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
        backgroundGradient.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
        
        var gradient = new Gradient();
        gradient.AddPoint(0.0f, new Color(0.02f, 0.02f, 0.08f));
        gradient.AddPoint(0.5f, new Color(0.05f, 0.05f, 0.15f));
        gradient.AddPoint(1.0f, new Color(0.08f, 0.03f, 0.12f));
        
        var gradientTexture = new GradientTexture2D();
        gradientTexture.Gradient = gradient;
        gradientTexture.FillFrom = new Vector2(0, 0);
        gradientTexture.FillTo = new Vector2(1, 1);
        
        backgroundGradient.Texture = gradientTexture;
        AddChild(backgroundGradient);
        
        menuContainer = new VBoxContainer();
        menuContainer.AnchorLeft = 0.5f;
        menuContainer.AnchorTop = 0.5f;
        menuContainer.AnchorRight = 0.5f;
        menuContainer.AnchorBottom = 0.5f;
        menuContainer.Position = new Vector2(-200, -250);
        menuContainer.AddThemeConstantOverride("separation", 25);
        AddChild(menuContainer);
        
        // Main title with glow effect
        titleLabel = new Label();
        titleLabel.Text = "LENIA";
        titleLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.95f, 1.0f));
        titleLabel.AddThemeFontSizeOverride("font_size", 96);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        menuContainer.AddChild(titleLabel);
        
        // Subtitle
        subtitleLabel = new Label();
        subtitleLabel.Text = "Continuous Cellular Automaton";
        subtitleLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.9f));
        subtitleLabel.AddThemeFontSizeOverride("font_size", 20);
        subtitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        menuContainer.AddChild(subtitleLabel);
        
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 50);
        menuContainer.AddChild(spacer);
        
        startButton = CreateStyledMenuButton("▶ Start Simulation", new Color(0.2f, 0.6f, 0.3f));
        startButton.Pressed += OnStartPressed;
        menuContainer.AddChild(startButton);
        
        aboutButton = CreateStyledMenuButton("ℹ About", new Color(0.3f, 0.4f, 0.7f));
        aboutButton.Pressed += OnAboutPressed;
        menuContainer.AddChild(aboutButton);
        
        quitButton = CreateStyledMenuButton("✕ Quit", new Color(0.7f, 0.3f, 0.3f));
        quitButton.Pressed += OnQuitPressed;
        menuContainer.AddChild(quitButton);
    }
    
    private Button CreateStyledMenuButton(string text, Color accentColor)
    {
        var button = new Button();
        button.Text = text;
        button.CustomMinimumSize = new Vector2(400, 70);
        button.AddThemeFontSizeOverride("font_size", 20);
        
        var normalStyle = new StyleBoxFlat();
        normalStyle.BgColor = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        normalStyle.BorderWidthTop = 2;
        normalStyle.BorderWidthBottom = 2;
        normalStyle.BorderWidthLeft = 2;
        normalStyle.BorderWidthRight = 2;
        normalStyle.BorderColor = accentColor;
        normalStyle.CornerRadiusTopLeft = 10;
        normalStyle.CornerRadiusTopRight = 10;
        normalStyle.CornerRadiusBottomLeft = 10;
        normalStyle.CornerRadiusBottomRight = 10;
        button.AddThemeStyleboxOverride("normal", normalStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(accentColor.R * 0.3f, accentColor.G * 0.3f, accentColor.B * 0.3f, 0.9f);
        hoverStyle.BorderWidthTop = 2;
        hoverStyle.BorderWidthBottom = 2;
        hoverStyle.BorderWidthLeft = 2;
        hoverStyle.BorderWidthRight = 2;
        hoverStyle.BorderColor = accentColor;
        hoverStyle.CornerRadiusTopLeft = 10;
        hoverStyle.CornerRadiusTopRight = 10;
        hoverStyle.CornerRadiusBottomLeft = 10;
        hoverStyle.CornerRadiusBottomRight = 10;
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        button.AddThemeColorOverride("font_color", new Color(0.95f, 0.95f, 1.0f));
        
        return button;
    }
    
    private void CreateAboutPanel()
    {
        aboutPanel = new Panel();
        aboutPanel.AnchorLeft = 0;
        aboutPanel.AnchorTop = 0;
        aboutPanel.AnchorRight = 1;
        aboutPanel.AnchorBottom = 1;
        aboutPanel.Visible = false;
        AddChild(aboutPanel);
        
        var aboutBackground = new ColorRect();
        aboutBackground.Color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        aboutBackground.AnchorLeft = 0;
        aboutBackground.AnchorTop = 0;
        aboutBackground.AnchorRight = 1;
        aboutBackground.AnchorBottom = 1;
        aboutPanel.AddChild(aboutBackground);
        
        var aboutContainer = new VBoxContainer();
        aboutContainer.AnchorLeft = 0.5f;
        aboutContainer.AnchorTop = 0.5f;
        aboutContainer.AnchorRight = 0.5f;
        aboutContainer.AnchorBottom = 0.5f;
        aboutContainer.Position = new Vector2(-500, -350);
        aboutContainer.CustomMinimumSize = new Vector2(1000, 700);
        aboutContainer.AddThemeConstantOverride("separation", 25);
        aboutPanel.AddChild(aboutContainer);
        
        var aboutTitle = new Label();
        aboutTitle.Text = "About Lenia";
        aboutTitle.HorizontalAlignment = HorizontalAlignment.Center;
        aboutTitle.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        aboutTitle.AddThemeFontSizeOverride("font_size", 40);
        aboutContainer.AddChild(aboutTitle);
        
        aboutText = new RichTextLabel();
        aboutText.BbcodeEnabled = true;
        aboutText.FitContent = true;
        aboutText.AddThemeFontSizeOverride("normal_font_size", 18);
        aboutText.Text = @"[center][b]Lenia - Continuous Cellular Automaton[/b][/center]

Lenia is a family of continuous cellular automata that extends Conway's Game of Life to continuous space, time, and states. Unlike traditional cellular automata with discrete on/off states, Lenia uses floating-point values creating smooth, lifelike patterns.

[b]Key Features:[/b]
• Continuous states (0.0 to 1.0) instead of binary
• Smooth kernel convolution for neighbor interactions
• Growth function based on local density
• Emergent life-like behaviors and patterns

[b]Created by:[/b] Bert Wang-Chak Chan
[b]Implementation:[/b] Godot C# Version

[b]Controls:[/b]
• Adjust parameters in real-time
• Try different initialization patterns
• Watch emergent behaviors evolve

This implementation demonstrates the beauty of continuous cellular automata and the emergence of complex life-like patterns from simple mathematical rules.";
        aboutContainer.AddChild(aboutText);
        
        backButton = CreateStyledMenuButton("← Back", new Color(0.5f, 0.5f, 0.6f));
        backButton.CustomMinimumSize = new Vector2(200, 60);
        backButton.Pressed += OnBackPressed;
        aboutContainer.AddChild(backButton);
    }
    
    private void OnStartPressed()
    {
        GetTree().ChangeSceneToFile("res://lenia.tscn");
    }
    
    private void OnAboutPressed()
    {
        menuContainer.Visible = false;
        aboutPanel.Visible = true;
    }
    
    private void OnBackPressed()
    {
        aboutPanel.Visible = false;
        menuContainer.Visible = true;
    }
    
    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}