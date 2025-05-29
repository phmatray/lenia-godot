using Godot;

public partial class LeftToolbar : Panel
{
    public enum Tool
    {
        Paint,
        Erase,
        Pattern,
        Move
    }
    
    [Signal]
    public delegate void ToolChangedEventHandler(Tool tool);
    
    private Tool currentTool = Tool.Paint;
    private Button paintButton;
    private Button eraseButton;
    private Button patternButton;
    private Button moveButton;
    
    public override void _Ready()
    {
        SetupToolbar();
    }
    
    private void SetupToolbar()
    {
        var toolbarStyle = new StyleBoxFlat();
        toolbarStyle.BgColor = new Color(0.1f, 0.12f, 0.2f, 0.95f);
        toolbarStyle.BorderWidthRight = 2;
        toolbarStyle.BorderColor = new Color(0.2f, 0.4f, 0.8f, 0.4f);
        AddThemeStyleboxOverride("panel", toolbarStyle);
        
        var vbox = new VBoxContainer();
        vbox.AnchorLeft = 0;
        vbox.AnchorTop = 0;
        vbox.AnchorRight = 0;
        vbox.AnchorBottom = 1;
        vbox.OffsetLeft = 10;
        vbox.OffsetTop = 10;
        vbox.OffsetRight = 70;
        vbox.OffsetBottom = -10;
        vbox.AddThemeConstantOverride("separation", 10);
        AddChild(vbox);
        
        paintButton = CreateToolButton("🖌", "Paint", Tool.Paint);
        eraseButton = CreateToolButton("🧹", "Erase", Tool.Erase);
        patternButton = CreateToolButton("🎨", "Pattern", Tool.Pattern);
        moveButton = CreateToolButton("✋", "Move", Tool.Move);
        
        vbox.AddChild(paintButton);
        vbox.AddChild(eraseButton);
        vbox.AddChild(patternButton);
        vbox.AddChild(moveButton);
        
        vbox.AddChild(new HSeparator());
        
        UpdateButtonStates();
    }
    
    private Button CreateToolButton(string icon, string tooltip, Tool tool)
    {
        var button = new Button();
        button.Text = icon;
        button.CustomMinimumSize = new Vector2(50, 50);
        button.AddThemeFontSizeOverride("font_size", 20);
        button.TooltipText = tooltip;
        
        var normalStyle = new StyleBoxFlat();
        normalStyle.BgColor = new Color(0.15f, 0.2f, 0.35f, 0.8f);
        normalStyle.BorderWidthTop = 1;
        normalStyle.BorderWidthBottom = 1;
        normalStyle.BorderWidthLeft = 1;
        normalStyle.BorderWidthRight = 1;
        normalStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        normalStyle.CornerRadiusTopLeft = 8;
        normalStyle.CornerRadiusTopRight = 8;
        normalStyle.CornerRadiusBottomLeft = 8;
        normalStyle.CornerRadiusBottomRight = 8;
        button.AddThemeStyleboxOverride("normal", normalStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.2f, 0.3f, 0.5f, 0.9f);
        hoverStyle.BorderWidthTop = 1;
        hoverStyle.BorderWidthBottom = 1;
        hoverStyle.BorderWidthLeft = 1;
        hoverStyle.BorderWidthRight = 1;
        hoverStyle.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        hoverStyle.CornerRadiusTopLeft = 8;
        hoverStyle.CornerRadiusTopRight = 8;
        hoverStyle.CornerRadiusBottomLeft = 8;
        hoverStyle.CornerRadiusBottomRight = 8;
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        var pressedStyle = new StyleBoxFlat();
        pressedStyle.BgColor = new Color(0.3f, 0.5f, 0.8f, 1.0f);
        pressedStyle.BorderWidthTop = 2;
        pressedStyle.BorderWidthBottom = 2;
        pressedStyle.BorderWidthLeft = 2;
        pressedStyle.BorderWidthRight = 2;
        pressedStyle.BorderColor = new Color(0.5f, 0.7f, 1.0f);
        pressedStyle.CornerRadiusTopLeft = 8;
        pressedStyle.CornerRadiusTopRight = 8;
        pressedStyle.CornerRadiusBottomLeft = 8;
        pressedStyle.CornerRadiusBottomRight = 8;
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        
        button.Pressed += () => OnToolSelected(tool);
        
        return button;
    }
    
    private void OnToolSelected(Tool tool)
    {
        currentTool = tool;
        UpdateButtonStates();
        EmitSignal(SignalName.ToolChanged, (int)tool);
    }
    
    private void UpdateButtonStates()
    {
        UpdateButtonState(paintButton, currentTool == Tool.Paint);
        UpdateButtonState(eraseButton, currentTool == Tool.Erase);
        UpdateButtonState(patternButton, currentTool == Tool.Pattern);
        UpdateButtonState(moveButton, currentTool == Tool.Move);
    }
    
    private void UpdateButtonState(Button button, bool isSelected)
    {
        if (isSelected)
        {
            var selectedStyle = new StyleBoxFlat();
            selectedStyle.BgColor = new Color(0.3f, 0.5f, 0.8f, 1.0f);
            selectedStyle.BorderWidthTop = 2;
            selectedStyle.BorderWidthBottom = 2;
            selectedStyle.BorderWidthLeft = 2;
            selectedStyle.BorderWidthRight = 2;
            selectedStyle.BorderColor = new Color(0.5f, 0.7f, 1.0f);
            selectedStyle.CornerRadiusTopLeft = 8;
            selectedStyle.CornerRadiusTopRight = 8;
            selectedStyle.CornerRadiusBottomLeft = 8;
            selectedStyle.CornerRadiusBottomRight = 8;
            button.AddThemeStyleboxOverride("normal", selectedStyle);
        }
        else
        {
            var normalStyle = new StyleBoxFlat();
            normalStyle.BgColor = new Color(0.15f, 0.2f, 0.35f, 0.8f);
            normalStyle.BorderWidthTop = 1;
            normalStyle.BorderWidthBottom = 1;
            normalStyle.BorderWidthLeft = 1;
            normalStyle.BorderWidthRight = 1;
            normalStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
            normalStyle.CornerRadiusTopLeft = 8;
            normalStyle.CornerRadiusTopRight = 8;
            normalStyle.CornerRadiusBottomLeft = 8;
            normalStyle.CornerRadiusBottomRight = 8;
            button.AddThemeStyleboxOverride("normal", normalStyle);
        }
    }
}