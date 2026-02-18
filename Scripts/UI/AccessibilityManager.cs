using Godot;
using System.Collections.Generic;

namespace Lenia.UI
{
    /// <summary>
    /// Manages accessibility features including high contrast mode, screen reader support, and keyboard navigation
    /// </summary>
    public partial class AccessibilityManager : Node
    {
        private static AccessibilityManager _instance;
        public static AccessibilityManager Instance => _instance;
        
        [Signal] public delegate void AccessibilityModeChangedEventHandler(AccessibilityMode mode);
        
        private AccessibilityMode currentMode = AccessibilityMode.None;
        private bool highContrastEnabled = false;
        private bool reducedMotionEnabled = false;
        private bool screenReaderEnabled = false;
        private float fontScaleFactor = 1.0f;
        
        private Dictionary<Control, AccessibilityData> accessibilityData = new Dictionary<Control, AccessibilityData>();
        private Control focusedControl = null;
        private AudioStreamPlayer audioPlayer;
        
        public enum AccessibilityMode
        {
            None,
            HighContrast,
            ReducedMotion,
            LargeText,
            ScreenReader,
            KeyboardOnly
        }
        
        public override void _Ready()
        {
            _instance = this;
            ProcessMode = ProcessModeEnum.Always;
            
            // Create audio player for screen reader
            audioPlayer = new AudioStreamPlayer();
            AddChild(audioPlayer);
            
            // Load accessibility preferences
            LoadAccessibilitySettings();
        }
        
        /// <summary>
        /// Enable or disable high contrast mode
        /// </summary>
        public void SetHighContrastMode(bool enabled)
        {
            highContrastEnabled = enabled;
            
            if (enabled)
            {
                ApplyHighContrastTheme();
            }
            else
            {
                RestoreDefaultTheme();
            }
            
            SaveAccessibilitySettings();
            EmitSignal(SignalName.AccessibilityModeChanged, (int)AccessibilityMode.HighContrast);
        }
        
        /// <summary>
        /// Enable or disable reduced motion
        /// </summary>
        public void SetReducedMotion(bool enabled)
        {
            reducedMotionEnabled = enabled;
            
            if (enabled)
            {
                // TODO: Implement reduced motion by modifying AnimationSystem
                // For now, we just store the state
                // AnimationSystem needs to check this flag when creating animations
            }
            else
            {
                // TODO: Restore normal animation durations
                // AnimationSystem needs to check this flag when creating animations
            }
            
            SaveAccessibilitySettings();
            EmitSignal(SignalName.AccessibilityModeChanged, (int)AccessibilityMode.ReducedMotion);
        }
        
        /// <summary>
        /// Set font scale factor for large text mode
        /// </summary>
        public void SetFontScale(float scale)
        {
            fontScaleFactor = Mathf.Clamp(scale, 1.0f, 2.0f);
            ApplyFontScaling();
            
            SaveAccessibilitySettings();
            EmitSignal(SignalName.AccessibilityModeChanged, (int)AccessibilityMode.LargeText);
        }
        
        /// <summary>
        /// Register a control with accessibility data
        /// </summary>
        public static void RegisterControl(Control control, string label, string description = "", string role = "button")
        {
            if (Instance == null) return;
            
            var data = new AccessibilityData
            {
                Label = label,
                Description = description,
                Role = role,
                Control = control
            };
            
            Instance.accessibilityData[control] = data;
            
            // Set up keyboard navigation
            control.FocusMode = Control.FocusModeEnum.All;
            control.FocusEntered += () => Instance.OnControlFocused(control);
            control.FocusExited += () => Instance.OnControlUnfocused(control);
            
            // Add visual focus indicator
            control.Draw += () => Instance.DrawFocusIndicator(control);
        }
        
        /// <summary>
        /// Announce text to screen reader
        /// </summary>
        public static void Announce(string text, AnnouncementPriority priority = AnnouncementPriority.Normal)
        {
            if (Instance == null || !Instance.screenReaderEnabled) return;
            
            // In a real implementation, this would interface with the OS screen reader
            // For now, we'll just log it and could play audio cues
            GD.Print($"[Screen Reader] {priority}: {text}");
            
            if (priority == AnnouncementPriority.High)
            {
                // Play attention sound
                Instance.PlayAccessibilitySound("attention");
            }
        }
        
        private void OnControlFocused(Control control)
        {
            focusedControl = control;
            
            if (accessibilityData.TryGetValue(control, out var data))
            {
                // Announce control to screen reader
                var announcement = $"{data.Role}: {data.Label}";
                if (!string.IsNullOrEmpty(data.Description))
                {
                    announcement += $". {data.Description}";
                }
                
                Announce(announcement);
            }
            
            control.QueueRedraw();
        }
        
        private void OnControlUnfocused(Control control)
        {
            if (focusedControl == control)
            {
                focusedControl = null;
            }
            control.QueueRedraw();
        }
        
        private void DrawFocusIndicator(Control control)
        {
            if (focusedControl != control) return;
            
            // Draw custom focus indicator
            var rect = new Rect2(Vector2.Zero, control.Size);
            var focusColor = highContrastEnabled ? Colors.Yellow : ModernTheme.Colors.Primary;
            
            // Draw outer focus ring
            control.DrawRect(rect.Grow(2), focusColor, false, 3.0f);
            
            // Draw inner focus ring for better visibility
            control.DrawRect(rect.Grow(1), new Color(0, 0, 0, 0.5f), false, 1.0f);
        }
        
        private void ApplyHighContrastTheme()
        {
            // Create high contrast theme
            var theme = new Theme();
            
            // High contrast colors
            var bgColor = Colors.Black;
            var fgColor = Colors.White;
            var accentColor = Colors.Yellow;
            var borderColor = Colors.White;
            
            // Button styles
            var buttonNormal = new StyleBoxFlat();
            buttonNormal.BgColor = bgColor;
            buttonNormal.BorderWidthLeft = 2;
            buttonNormal.BorderWidthTop = 2;
            buttonNormal.BorderWidthRight = 2;
            buttonNormal.BorderWidthBottom = 2;
            buttonNormal.BorderColor = borderColor;
            buttonNormal.SetCornerRadiusAll(4);
            
            var buttonHover = new StyleBoxFlat();
            buttonHover.BgColor = accentColor;
            buttonHover.BorderWidthLeft = 2;
            buttonHover.BorderWidthTop = 2;
            buttonHover.BorderWidthRight = 2;
            buttonHover.BorderWidthBottom = 2;
            buttonHover.BorderColor = borderColor;
            buttonHover.SetCornerRadiusAll(4);
            
            theme.SetStylebox("normal", "Button", buttonNormal);
            theme.SetStylebox("hover", "Button", buttonHover);
            theme.SetStylebox("pressed", "Button", buttonHover);
            
            theme.SetColor("font_color", "Button", fgColor);
            theme.SetColor("font_hover_color", "Button", bgColor);
            theme.SetColor("font_pressed_color", "Button", bgColor);
            
            // Panel styles
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = bgColor;
            panelStyle.BorderWidthLeft = 2;
            panelStyle.BorderWidthTop = 2;
            panelStyle.BorderWidthRight = 2;
            panelStyle.BorderWidthBottom = 2;
            panelStyle.BorderColor = borderColor;
            
            theme.SetStylebox("panel", "Panel", panelStyle);
            theme.SetStylebox("panel", "PanelContainer", panelStyle);
            
            // Label colors
            theme.SetColor("font_color", "Label", fgColor);
            
            // Apply to root
            GetTree().Root.Theme = theme;
        }
        
        private void RestoreDefaultTheme()
        {
            // Restore the original Lenia theme
            var themeResource = GD.Load<Theme>("res://lenia_theme.tres");
            GetTree().Root.Theme = themeResource;
        }
        
        private void ApplyFontScaling()
        {
            // This would recursively apply font scaling to all controls
            ApplyFontScaleToNode(GetTree().Root);
        }
        
        private void ApplyFontScaleToNode(Node node)
        {
            if (node is Control control)
            {
                // Scale font sizes
                var baseFontSize = control.GetThemeFontSize("font_size");
                if (baseFontSize > 0)
                {
                    control.AddThemeFontSizeOverride("font_size", (int)(baseFontSize * fontScaleFactor));
                }
            }
            
            foreach (var child in node.GetChildren())
            {
                ApplyFontScaleToNode(child);
            }
        }
        
        private void PlayAccessibilitySound(string soundType)
        {
            // In a real implementation, load and play appropriate sound effects
            // For example: attention beeps, success sounds, error sounds
        }
        
        private void LoadAccessibilitySettings()
        {
            var config = new ConfigFile();
            var err = config.Load("user://accessibility.cfg");
            if (err != Error.Ok) return;
            
            highContrastEnabled = config.GetValue("accessibility", "high_contrast", false).AsBool();
            reducedMotionEnabled = config.GetValue("accessibility", "reduced_motion", false).AsBool();
            screenReaderEnabled = config.GetValue("accessibility", "screen_reader", false).AsBool();
            fontScaleFactor = (float)config.GetValue("accessibility", "font_scale", 1.0f).AsDouble();
            
            // Apply loaded settings
            if (highContrastEnabled) SetHighContrastMode(true);
            if (reducedMotionEnabled) SetReducedMotion(true);
            if (fontScaleFactor > 1.0f) SetFontScale(fontScaleFactor);
        }
        
        private void SaveAccessibilitySettings()
        {
            var config = new ConfigFile();
            config.SetValue("accessibility", "high_contrast", highContrastEnabled);
            config.SetValue("accessibility", "reduced_motion", reducedMotionEnabled);
            config.SetValue("accessibility", "screen_reader", screenReaderEnabled);
            config.SetValue("accessibility", "font_scale", fontScaleFactor);
            config.Save("user://accessibility.cfg");
        }
        
        public override void _Input(InputEvent @event)
        {
            // Handle keyboard navigation enhancements
            if (@event.IsActionPressed("ui_focus_next"))
            {
                // Tab navigation with screen reader announcements
                var nextControl = GetNextFocusableControl();
                if (nextControl != null)
                {
                    nextControl.GrabFocus();
                }
            }
            else if (@event.IsActionPressed("ui_focus_prev"))
            {
                // Shift+Tab navigation
                var prevControl = GetPreviousFocusableControl();
                if (prevControl != null)
                {
                    prevControl.GrabFocus();
                }
            }
        }
        
        private Control GetNextFocusableControl()
        {
            // Implementation would find the next focusable control in tab order
            return null;
        }
        
        private Control GetPreviousFocusableControl()
        {
            // Implementation would find the previous focusable control in tab order
            return null;
        }
        
        private class AccessibilityData
        {
            public string Label { get; set; }
            public string Description { get; set; }
            public string Role { get; set; }
            public Control Control { get; set; }
        }
        
        public enum AnnouncementPriority
        {
            Low,
            Normal,
            High
        }
    }
}