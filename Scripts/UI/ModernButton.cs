using Godot;

namespace Lenia.UI
{
    /// <summary>
    /// Modern button with smooth hover effects, ripple animations, and icon support
    /// </summary>
    public partial class ModernButton : Button
    {
        [Export] public bool EnableRipple { get; set; } = true;
        [Export] public bool EnableHoverAnimation { get; set; } = true;
        [Export] public bool EnablePressAnimation { get; set; } = true;
        [Export] public Color RippleColor { get; set; } = new Color(1, 1, 1, 0.3f);
        [Export] public float HoverScale { get; set; } = 1.05f;
        
        private bool isHovered = false;
        private Tween currentTween;
        private Color originalModulate;
        private Vector2 originalScale;
        
        public override void _Ready()
        {
            // Store original values
            originalModulate = Modulate;
            originalScale = Scale;
            
            // Set pivot to center for scaling
            PivotOffset = Size / 2;
            
            // Apply modern theme
            ApplyModernTheme();
            
            // Connect signals
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
            ButtonDown += OnButtonDown;
            ButtonUp += OnButtonUp;
            GuiInput += OnGuiInput;
            Resized += () => PivotOffset = Size / 2;
            
            // Add tooltip if text is provided
            if (!string.IsNullOrEmpty(TooltipText))
            {
                var parts = TooltipText.Split('|');
                if (parts.Length >= 2)
                {
                    TooltipHelper.AddTooltip(this, parts[0], parts[1], 
                        Icon, parts.Length > 2 ? parts[2] : null);
                }
            }
        }
        
        private void ApplyModernTheme()
        {
            // Apply theme overrides
            AddThemeStyleboxOverride("normal", ModernTheme.NeumorphicButton.CreateNormal());
            AddThemeStyleboxOverride("hover", ModernTheme.NeumorphicButton.CreateHover());
            AddThemeStyleboxOverride("pressed", ModernTheme.NeumorphicButton.CreatePressed());
            AddThemeStyleboxOverride("disabled", ModernTheme.NeumorphicButton.CreateDisabled());
            
            // Font settings
            AddThemeColorOverride("font_color", ModernTheme.Colors.TextSecondary);
            AddThemeColorOverride("font_hover_color", ModernTheme.Colors.TextPrimary);
            AddThemeColorOverride("font_pressed_color", ModernTheme.Colors.Primary);
            AddThemeColorOverride("font_disabled_color", ModernTheme.Colors.TextMuted);
            
            // Icon settings
            AddThemeColorOverride("icon_normal_color", ModernTheme.Colors.TextSecondary);
            AddThemeColorOverride("icon_hover_color", ModernTheme.Colors.TextPrimary);
            AddThemeColorOverride("icon_pressed_color", ModernTheme.Colors.Primary);
            AddThemeColorOverride("icon_disabled_color", ModernTheme.Colors.TextMuted);
        }
        
        private void OnMouseEntered()
        {
            if (!EnableHoverAnimation || Disabled) return;
            
            isHovered = true;
            
            // Kill any existing tween
            currentTween?.Kill();
            currentTween = CreateTween();
            currentTween.SetParallel();
            
            // Scale animation
            currentTween.TweenProperty(this, "scale", originalScale * HoverScale, 0.2f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
            
            // Subtle glow effect
            var glowColor = originalModulate.Lerp(ModernTheme.Colors.PrimaryGlow, 0.1f);
            currentTween.TweenProperty(this, "modulate", glowColor, 0.2f);
            
            // Elevate shadow (if using custom draw)
            QueueRedraw();
        }
        
        private void OnMouseExited()
        {
            if (!EnableHoverAnimation || Disabled) return;
            
            isHovered = false;
            
            // Kill any existing tween
            currentTween?.Kill();
            currentTween = CreateTween();
            currentTween.SetParallel();
            
            // Return to original scale
            currentTween.TweenProperty(this, "scale", originalScale, 0.2f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
            
            // Return to original color
            currentTween.TweenProperty(this, "modulate", originalModulate, 0.2f);
            
            QueueRedraw();
        }
        
        private void OnButtonDown()
        {
            if (!EnablePressAnimation || Disabled) return;
            
            // Kill any existing tween
            currentTween?.Kill();
            currentTween = CreateTween();
            
            // Press down animation
            currentTween.TweenProperty(this, "scale", originalScale * 0.95f, 0.1f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
        }
        
        private void OnButtonUp()
        {
            if (!EnablePressAnimation || Disabled) return;
            
            // Kill any existing tween
            currentTween?.Kill();
            currentTween = CreateTween();
            
            // Bounce back animation
            var targetScale = isHovered && EnableHoverAnimation ? originalScale * HoverScale : originalScale;
            currentTween.TweenProperty(this, "scale", targetScale, 0.15f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
        }
        
        private void OnGuiInput(InputEvent @event)
        {
            if (!EnableRipple || Disabled) return;
            
            if (@event is InputEventMouseButton mouseEvent && 
                mouseEvent.Pressed && 
                mouseEvent.ButtonIndex == MouseButton.Left)
            {
                CreateRippleEffect(mouseEvent.Position);
            }
        }
        
        private void CreateRippleEffect(Vector2 clickPosition)
        {
            // Create ripple container
            var rippleContainer = new Control();
            rippleContainer.MouseFilter = MouseFilterEnum.Ignore;
            rippleContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            rippleContainer.ClipContents = true;
            AddChild(rippleContainer);
            MoveChild(rippleContainer, 0); // Place behind button content
            
            // Create ripple circle
            var ripple = new Control();
            ripple.CustomMinimumSize = Vector2.One * 10;
            ripple.Position = clickPosition - ripple.CustomMinimumSize / 2;
            ripple.PivotOffset = ripple.CustomMinimumSize / 2;
            ripple.MouseFilter = MouseFilterEnum.Ignore;
            rippleContainer.AddChild(ripple);
            
            // Set initial state
            ripple.Scale = Vector2.One;
            ripple.Modulate = RippleColor;
            
            // Animate ripple
            var tween = ripple.CreateTween();
            tween.SetParallel();
            
            // Calculate max distance to cover entire button
            var maxDistance = Mathf.Max(
                clickPosition.Length(),
                (Size - clickPosition).Length()
            ) * 2;
            
            var scaleFactor = maxDistance / ripple.CustomMinimumSize.X;
            
            tween.TweenProperty(ripple, "scale", Vector2.One * scaleFactor, 0.6f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
            
            tween.TweenProperty(ripple, "modulate:a", 0.0f, 0.6f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.In);
            
            // Clean up after animation
            tween.Finished += () => rippleContainer.QueueFree();
        }
        
        public override void _Draw()
        {
            // Draw custom ripple background
            if (EnableRipple)
            {
                // This is handled by the ripple effect nodes
            }
            
            // Add subtle inner shadow when pressed
            if (ButtonPressed && EnablePressAnimation)
            {
                var rect = new Rect2(Vector2.Zero, Size);
                var shadowColor = new Color(0, 0, 0, 0.2f);
                
                // Draw inner shadow effect
                for (int i = 0; i < 3; i++)
                {
                    var inset = i * 2;
                    var shadowRect = rect.Grow(-inset);
                    DrawRect(shadowRect, shadowColor * (1.0f - i * 0.3f), false, 1.0f);
                }
            }
        }
        
        /// <summary>
        /// Set button style variant
        /// </summary>
        public void SetVariant(ButtonVariant variant)
        {
            switch (variant)
            {
                case ButtonVariant.Primary:
                    AddThemeColorOverride("font_color", Colors.White);
                    AddThemeColorOverride("font_hover_color", Colors.White);
                    AddThemeColorOverride("font_pressed_color", Colors.White);
                    Modulate = ModernTheme.Colors.Primary;
                    break;
                    
                case ButtonVariant.Secondary:
                    Modulate = ModernTheme.Colors.Secondary;
                    break;
                    
                case ButtonVariant.Success:
                    Modulate = ModernTheme.Colors.Success;
                    break;
                    
                case ButtonVariant.Warning:
                    Modulate = ModernTheme.Colors.Warning;
                    break;
                    
                case ButtonVariant.Danger:
                    Modulate = ModernTheme.Colors.Error;
                    break;
                    
                case ButtonVariant.Ghost:
                    AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
                    AddThemeStyleboxOverride("hover", CreateGhostHoverStyle());
                    AddThemeStyleboxOverride("pressed", CreateGhostPressedStyle());
                    break;
            }
            
            originalModulate = Modulate;
        }
        
        private StyleBoxFlat CreateGhostHoverStyle()
        {
            var style = new StyleBoxFlat();
            style.BgColor = ModernTheme.Colors.BackgroundLight.Lerp(Colors.Transparent, 0.7f);
            style.SetCornerRadiusAll(8);
            return style;
        }
        
        private StyleBoxFlat CreateGhostPressedStyle()
        {
            var style = new StyleBoxFlat();
            style.BgColor = ModernTheme.Colors.BackgroundLight.Lerp(Colors.Transparent, 0.5f);
            style.SetCornerRadiusAll(8);
            return style;
        }
        
        public enum ButtonVariant
        {
            Default,
            Primary,
            Secondary,
            Success,
            Warning,
            Danger,
            Ghost
        }
    }
}