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
        // Get button references from scene
        paintButton = GetNode<Button>("VBoxContainer/PaintButton");
        eraseButton = GetNode<Button>("VBoxContainer/EraseButton");
        patternButton = GetNode<Button>("VBoxContainer/PatternButton");
        moveButton = GetNode<Button>("VBoxContainer/MoveButton");
        
        // Style the panel
        var toolbarStyle = new StyleBoxFlat();
        toolbarStyle.BgColor = new Color(0.1f, 0.12f, 0.2f, 0.95f);
        toolbarStyle.BorderWidthRight = 2;
        toolbarStyle.BorderColor = new Color(0.2f, 0.4f, 0.8f, 0.4f);
        AddThemeStyleboxOverride("panel", toolbarStyle);
        
        // Style buttons
        StyleButton(paintButton);
        StyleButton(eraseButton);
        StyleButton(patternButton);
        StyleButton(moveButton);
        
        // Connect signals
        paintButton.Pressed += () => OnToolSelected(Tool.Paint);
        eraseButton.Pressed += () => OnToolSelected(Tool.Erase);
        patternButton.Pressed += () => OnToolSelected(Tool.Pattern);
        moveButton.Pressed += () => OnToolSelected(Tool.Move);
        
        // Set initial state
        UpdateButtonStates();
    }
    
    private void StyleButton(Button button)
    {
        var normalStyle = new StyleBoxFlat();
        normalStyle.BgColor = new Color(0.15f, 0.2f, 0.35f, 0.8f);
        normalStyle.SetBorderWidthAll(1);
        normalStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        normalStyle.SetCornerRadiusAll(8);
        button.AddThemeStyleboxOverride("normal", normalStyle);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.2f, 0.3f, 0.5f, 0.9f);
        hoverStyle.SetBorderWidthAll(1);
        hoverStyle.BorderColor = new Color(0.4f, 0.6f, 0.9f);
        hoverStyle.SetCornerRadiusAll(8);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        var pressedStyle = new StyleBoxFlat();
        pressedStyle.BgColor = new Color(0.3f, 0.5f, 0.8f, 1.0f);
        pressedStyle.SetBorderWidthAll(2);
        pressedStyle.BorderColor = new Color(0.5f, 0.7f, 1.0f);
        pressedStyle.SetCornerRadiusAll(8);
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        
        button.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
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
            selectedStyle.SetBorderWidthAll(2);
            selectedStyle.BorderColor = new Color(0.5f, 0.7f, 1.0f);
            selectedStyle.SetCornerRadiusAll(8);
            button.AddThemeStyleboxOverride("normal", selectedStyle);
        }
        else
        {
            var normalStyle = new StyleBoxFlat();
            normalStyle.BgColor = new Color(0.15f, 0.2f, 0.35f, 0.8f);
            normalStyle.SetBorderWidthAll(1);
            normalStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
            normalStyle.SetCornerRadiusAll(8);
            button.AddThemeStyleboxOverride("normal", normalStyle);
        }
    }
}