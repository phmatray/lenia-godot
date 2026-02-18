using Godot;
using System.Collections.Generic;

namespace Lenia.UI
{
    /// <summary>
    /// Modern theme manager providing glassmorphic and neumorphic design styles
    /// </summary>
    public static class ModernTheme
    {
        // Color Palette - Modern gradient-based design
        public static class Colors
        {
            // Primary colors with variations
            public static readonly Color Primary = new Color("3B82F6");
            public static readonly Color PrimaryDark = new Color("2563EB");
            public static readonly Color PrimaryLight = new Color("60A5FA");
            public static readonly Color PrimaryGlow = new Color("93BBFD");
            
            // Secondary colors
            public static readonly Color Secondary = new Color("8B5CF6");
            public static readonly Color SecondaryDark = new Color("7C3AED");
            public static readonly Color SecondaryLight = new Color("A78BFA");
            
            // Accent colors
            public static readonly Color Accent = new Color("EC4899");
            public static readonly Color AccentGlow = new Color("F472B6");
            
            // Background colors
            public static readonly Color BackgroundDark = new Color("0F172A");
            public static readonly Color Background = new Color("1E293B");
            public static readonly Color BackgroundLight = new Color("334155");
            public static readonly Color BackgroundSurface = new Color("475569");
            
            // Glass effect colors
            public static readonly Color GlassWhite = new Color(1.0f, 1.0f, 1.0f, 0.1f);
            public static readonly Color GlassDark = new Color(0.0f, 0.0f, 0.0f, 0.2f);
            public static readonly Color GlassBorder = new Color(1.0f, 1.0f, 1.0f, 0.2f);
            
            // Text colors
            public static readonly Color TextPrimary = new Color("F8FAFC");
            public static readonly Color TextSecondary = new Color("CBD5E1");
            public static readonly Color TextMuted = new Color("94A3B8");
            
            // State colors
            public static readonly Color Success = new Color("10B981");
            public static readonly Color Warning = new Color("F59E0B");
            public static readonly Color Error = new Color("EF4444");
            public static readonly Color Info = new Color("3B82F6");
        }
        
        // Animation curves for smooth transitions
        public static class Animations
        {
            public const float FastDuration = 0.15f;
            public const float NormalDuration = 0.3f;
            public const float SlowDuration = 0.5f;
            
            public static Tween.TransitionType DefaultTransition = Tween.TransitionType.Cubic;
            public static Tween.EaseType DefaultEase = Tween.EaseType.Out;
            public static Tween.TransitionType BounceTransition = Tween.TransitionType.Back;
            public static Tween.EaseType BounceEase = Tween.EaseType.Out;
        }
        
        // Create glassmorphic panel style
        public static StyleBoxFlat CreateGlassPanel(float cornerRadius = 12.0f, float borderWidth = 1.0f)
        {
            var style = new StyleBoxFlat();
            
            // Semi-transparent background with blur effect
            style.BgColor = new Color(0.1f, 0.12f, 0.2f, 0.7f);
            
            // Subtle border for glass edge effect
            style.BorderWidthLeft = (int)borderWidth;
            style.BorderWidthTop = (int)borderWidth;
            style.BorderWidthRight = (int)borderWidth;
            style.BorderWidthBottom = (int)borderWidth;
            style.BorderColor = Colors.GlassBorder;
            
            // Rounded corners
            style.CornerRadiusTopLeft = (int)cornerRadius;
            style.CornerRadiusTopRight = (int)cornerRadius;
            style.CornerRadiusBottomLeft = (int)cornerRadius;
            style.CornerRadiusBottomRight = (int)cornerRadius;
            
            // Inner shadow for depth
            style.ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.3f);
            style.ShadowSize = 4;
            style.ShadowOffset = new Vector2(0, 2);
            
            // Content margins
            style.ContentMarginLeft = 16;
            style.ContentMarginTop = 16;
            style.ContentMarginRight = 16;
            style.ContentMarginBottom = 16;
            
            return style;
        }
        
        // Create neumorphic button styles
        public static class NeumorphicButton
        {
            public static StyleBoxFlat CreateNormal()
            {
                var style = new StyleBoxFlat();
                style.BgColor = Colors.Background;
                
                // Soft rounded corners
                style.SetCornerRadiusAll(8);
                
                // Double shadow for neumorphic effect
                style.ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.25f);
                style.ShadowSize = 6;
                style.ShadowOffset = new Vector2(3, 3);
                
                // Subtle border for definition
                style.BorderWidthLeft = 1;
                style.BorderWidthTop = 1;
                style.BorderWidthRight = 1;
                style.BorderWidthBottom = 1;
                style.BorderColor = new Color(1.0f, 1.0f, 1.0f, 0.05f);
                
                // Padding
                style.ContentMarginLeft = 12;
                style.ContentMarginTop = 8;
                style.ContentMarginRight = 12;
                style.ContentMarginBottom = 8;
                
                return style;
            }
            
            public static StyleBoxFlat CreateHover()
            {
                var style = CreateNormal();
                style.BgColor = Colors.BackgroundLight;
                style.BorderColor = Colors.PrimaryGlow.Lerp(Colors.GlassWhite, 0.5f);
                style.ShadowSize = 8;
                style.ShadowOffset = new Vector2(4, 4);
                return style;
            }
            
            public static StyleBoxFlat CreatePressed()
            {
                var style = new StyleBoxFlat();
                style.BgColor = Colors.BackgroundDark;
                style.SetCornerRadiusAll(8);
                
                // Inverted shadow for pressed effect
                style.ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.4f);
                style.ShadowSize = 2;
                style.ShadowOffset = new Vector2(-1, -1);
                
                style.BorderWidthLeft = 2;
                style.BorderWidthTop = 2;
                style.BorderWidthRight = 2;
                style.BorderWidthBottom = 2;
                style.BorderColor = Colors.Primary;
                
                style.ContentMarginLeft = 12;
                style.ContentMarginTop = 9;
                style.ContentMarginRight = 12;
                style.ContentMarginBottom = 7;
                
                return style;
            }
            
            public static StyleBoxFlat CreateDisabled()
            {
                var style = CreateNormal();
                style.BgColor = Colors.BackgroundDark.Lerp(Colors.Background, 0.5f);
                style.BorderColor = new Color(1.0f, 1.0f, 1.0f, 0.02f);
                style.ShadowSize = 2;
                return style;
            }
        }
        
        // Create gradient background
        public static GradientTexture2D CreateGradientBackground(bool isDark = true)
        {
            var gradient = new Gradient();
            var texture = new GradientTexture2D();
            
            if (isDark)
            {
                gradient.SetColor(0, Colors.BackgroundDark);
                gradient.SetColor(1, Colors.Background.Lerp(Colors.Primary, 0.05f));
            }
            else
            {
                gradient.SetColor(0, Colors.Background);
                gradient.SetColor(1, Colors.BackgroundLight.Lerp(Colors.Primary, 0.1f));
            }
            
            texture.Gradient = gradient;
            texture.Width = 1920;
            texture.Height = 1080;
            texture.FillFrom = new Vector2(0, 0);
            texture.FillTo = new Vector2(1, 1);
            
            return texture;
        }
        
        // Create tooltip style
        public static StyleBoxFlat CreateTooltipStyle()
        {
            var style = new StyleBoxFlat();
            
            // Dark semi-transparent background
            style.BgColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);
            
            // Subtle glow border
            style.BorderWidthLeft = 1;
            style.BorderWidthTop = 1;
            style.BorderWidthRight = 1;
            style.BorderWidthBottom = 1;
            style.BorderColor = Colors.PrimaryGlow.Lerp(Colors.GlassWhite, 0.3f);
            
            // Rounded corners
            style.SetCornerRadiusAll(6);
            
            // Soft shadow
            style.ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
            style.ShadowSize = 8;
            style.ShadowOffset = new Vector2(2, 4);
            
            // Padding
            style.ContentMarginLeft = 12;
            style.ContentMarginTop = 8;
            style.ContentMarginRight = 12;
            style.ContentMarginBottom = 8;
            
            return style;
        }
        
        // Apply theme to a control and its children
        public static void ApplyTheme(Control control)
        {
            // Create a new theme resource
            var theme = new Theme();
            
            // Button styles
            theme.SetStylebox("normal", "Button", NeumorphicButton.CreateNormal());
            theme.SetStylebox("hover", "Button", NeumorphicButton.CreateHover());
            theme.SetStylebox("pressed", "Button", NeumorphicButton.CreatePressed());
            theme.SetStylebox("disabled", "Button", NeumorphicButton.CreateDisabled());
            
            // Button colors
            theme.SetColor("font_color", "Button", Colors.TextSecondary);
            theme.SetColor("font_hover_color", "Button", Colors.TextPrimary);
            theme.SetColor("font_pressed_color", "Button", Colors.Primary);
            theme.SetColor("font_disabled_color", "Button", Colors.TextMuted);
            
            // Panel styles
            theme.SetStylebox("panel", "Panel", CreateGlassPanel());
            theme.SetStylebox("panel", "PanelContainer", CreateGlassPanel());
            
            // LineEdit styles
            var lineEditStyle = new StyleBoxFlat();
            lineEditStyle.BgColor = Colors.BackgroundDark.Lerp(Colors.GlassDark, 0.5f);
            lineEditStyle.BorderWidthLeft = 1;
            lineEditStyle.BorderWidthTop = 1;
            lineEditStyle.BorderWidthRight = 1;
            lineEditStyle.BorderWidthBottom = 2;
            lineEditStyle.BorderColor = Colors.BackgroundSurface;
            lineEditStyle.SetCornerRadiusAll(4);
            lineEditStyle.ContentMarginLeft = 8;
            lineEditStyle.ContentMarginRight = 8;
            
            theme.SetStylebox("normal", "LineEdit", lineEditStyle);
            theme.SetStylebox("focus", "LineEdit", CreateFocusedLineEdit());
            
            // Tooltip
            theme.SetStylebox("panel", "TooltipPanel", CreateTooltipStyle());
            theme.SetColor("font_color", "TooltipLabel", Colors.TextPrimary);
            theme.SetConstant("shadow_offset_x", "TooltipLabel", 1);
            theme.SetConstant("shadow_offset_y", "TooltipLabel", 1);
            
            // Apply theme
            control.Theme = theme;
        }
        
        private static StyleBoxFlat CreateFocusedLineEdit()
        {
            var style = new StyleBoxFlat();
            style.BgColor = Colors.BackgroundDark;
            style.BorderWidthLeft = 1;
            style.BorderWidthTop = 1;
            style.BorderWidthRight = 1;
            style.BorderWidthBottom = 2;
            style.BorderColor = Colors.Primary;
            style.SetCornerRadiusAll(4);
            style.ContentMarginLeft = 8;
            style.ContentMarginRight = 8;
            
            // Add glow effect
            style.ShadowColor = Colors.PrimaryGlow;
            style.ShadowSize = 4;
            style.ShadowOffset = new Vector2(0, 0);
            
            return style;
        }
    }
}