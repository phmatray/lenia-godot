using Godot;

public partial class MenuManager : Control
{
    private VBoxContainer menuContainer;
    private Label titleLabel;
    private Button startButton;
    private Button aboutButton;
    private Button quitButton;
    private Panel aboutPanel;
    private RichTextLabel aboutText;
    private Button backButton;
    
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
        var background = new ColorRect();
        background.Color = new Color(0.05f, 0.05f, 0.1f);
        background.AnchorLeft = 0;
        background.AnchorTop = 0;
        background.AnchorRight = 1;
        background.AnchorBottom = 1;
        AddChild(background);
        
        menuContainer = new VBoxContainer();
        menuContainer.AnchorLeft = 0;
        menuContainer.AnchorTop = 0.5f;
        menuContainer.AnchorRight = 0;
        menuContainer.AnchorBottom = 0.5f;
        menuContainer.Position = new Vector2(100, -100);
        menuContainer.AddThemeConstantOverride("separation", 20);
        AddChild(menuContainer);
        
        titleLabel = new Label();
        titleLabel.Text = "LENIA";
        titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        titleLabel.AddThemeFontSizeOverride("font_size", 64);
        menuContainer.AddChild(titleLabel);
        
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 40);
        menuContainer.AddChild(spacer);
        
        startButton = new Button();
        startButton.Text = "Start Simulation";
        startButton.CustomMinimumSize = new Vector2(200, 50);
        startButton.Pressed += OnStartPressed;
        menuContainer.AddChild(startButton);
        
        aboutButton = new Button();
        aboutButton.Text = "About";
        aboutButton.CustomMinimumSize = new Vector2(200, 50);
        aboutButton.Pressed += OnAboutPressed;
        menuContainer.AddChild(aboutButton);
        
        quitButton = new Button();
        quitButton.Text = "Quit";
        quitButton.CustomMinimumSize = new Vector2(200, 50);
        quitButton.Pressed += OnQuitPressed;
        menuContainer.AddChild(quitButton);
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
        aboutContainer.Position = new Vector2(-300, -200);
        aboutContainer.CustomMinimumSize = new Vector2(600, 400);
        aboutContainer.AddThemeConstantOverride("separation", 20);
        aboutPanel.AddChild(aboutContainer);
        
        var aboutTitle = new Label();
        aboutTitle.Text = "About Lenia";
        aboutTitle.HorizontalAlignment = HorizontalAlignment.Center;
        aboutTitle.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        aboutTitle.AddThemeFontSizeOverride("font_size", 32);
        aboutContainer.AddChild(aboutTitle);
        
        aboutText = new RichTextLabel();
        aboutText.BbcodeEnabled = true;
        aboutText.FitContent = true;
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
        
        backButton = new Button();
        backButton.Text = "Back";
        backButton.CustomMinimumSize = new Vector2(100, 40);
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