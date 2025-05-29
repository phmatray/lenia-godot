using Godot;

public partial class MenuManager : Control
{
    private VBoxContainer menuContainer;
    private Button startButton;
    private Button aboutButton;
    private Button quitButton;
    private Panel aboutPanel;
    private Button backButton;
    
    public override void _Ready()
    {
        // Get references to scene nodes
        menuContainer = GetNode<VBoxContainer>("MenuContainer");
        startButton = GetNode<Button>("MenuContainer/StartButton");
        aboutButton = GetNode<Button>("MenuContainer/AboutButton");
        quitButton = GetNode<Button>("MenuContainer/QuitButton");
        aboutPanel = GetNode<Panel>("AboutPanel");
        backButton = GetNode<Button>("AboutPanel/AboutContainer/BackButton");
        
        // Connect signals
        startButton.Pressed += OnStartPressed;
        aboutButton.Pressed += OnAboutPressed;
        quitButton.Pressed += OnQuitPressed;
        backButton.Pressed += OnBackPressed;
        
        // Apply hover styles
        ApplyHoverStyle(startButton, new Color(0.2f, 0.6f, 0.3f));
        ApplyHoverStyle(aboutButton, new Color(0.3f, 0.4f, 0.7f));
        ApplyHoverStyle(quitButton, new Color(0.7f, 0.3f, 0.3f));
        ApplyHoverStyle(backButton, new Color(0.5f, 0.5f, 0.6f));
    }
    
    private void ApplyHoverStyle(Button button, Color accentColor)
    {
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(accentColor.R * 0.3f, accentColor.G * 0.3f, accentColor.B * 0.3f, 0.9f);
        hoverStyle.SetBorderWidthAll(2);
        hoverStyle.BorderColor = accentColor;
        hoverStyle.SetCornerRadiusAll(10);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        var pressedStyle = new StyleBoxFlat();
        pressedStyle.BgColor = new Color(accentColor.R * 0.2f, accentColor.G * 0.2f, accentColor.B * 0.2f, 0.95f);
        pressedStyle.SetBorderWidthAll(2);
        pressedStyle.BorderColor = accentColor * 1.2f;
        pressedStyle.SetCornerRadiusAll(10);
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
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