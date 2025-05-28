using Godot;

public partial class CollapsibleSection : VBoxContainer
{
    private Button headerButton;
    private VBoxContainer contentContainer;
    private bool isExpanded = true;
    private string sectionTitle;
    
    public VBoxContainer Content => contentContainer;
    
    public CollapsibleSection(string title, bool startExpanded = true)
    {
        sectionTitle = title;
        isExpanded = startExpanded;
        SetupSection();
    }
    
    private void SetupSection()
    {
        AddThemeConstantOverride("separation", 5);
        
        // Create header button
        headerButton = new Button();
        headerButton.Text = GetHeaderText();
        headerButton.CustomMinimumSize = new Vector2(340, 30);
        headerButton.AddThemeFontSizeOverride("font_size", 14);
        headerButton.Alignment = HorizontalAlignment.Left;
        
        var headerStyle = new StyleBoxFlat();
        headerStyle.BgColor = new Color(0.2f, 0.25f, 0.4f, 0.8f);
        headerStyle.BorderWidthTop = 1;
        headerStyle.BorderWidthBottom = 1;
        headerStyle.BorderWidthLeft = 1;
        headerStyle.BorderWidthRight = 1;
        headerStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        headerStyle.CornerRadiusTopLeft = 6;
        headerStyle.CornerRadiusTopRight = 6;
        headerStyle.CornerRadiusBottomLeft = 6;
        headerStyle.CornerRadiusBottomRight = 6;
        headerButton.AddThemeStyleboxOverride("normal", headerStyle);
        
        headerButton.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1.0f));
        headerButton.Pressed += ToggleExpanded;
        
        AddChild(headerButton);
        
        // Create content container
        contentContainer = new VBoxContainer();
        contentContainer.AddThemeConstantOverride("separation", 8);
        contentContainer.Visible = isExpanded;
        AddChild(contentContainer);
    }
    
    private string GetHeaderText()
    {
        string arrow = isExpanded ? "▼" : "▶";
        return $"{arrow} {sectionTitle}";
    }
    
    private void ToggleExpanded()
    {
        isExpanded = !isExpanded;
        contentContainer.Visible = isExpanded;
        headerButton.Text = GetHeaderText();
    }
}