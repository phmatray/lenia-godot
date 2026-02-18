using Godot;
using System;
using System.Collections.Generic;

namespace Lenia.UI
{
    /// <summary>
    /// Rich tooltip system with icons, descriptions, and keyboard shortcuts
    /// </summary>
    public partial class RichTooltip : PanelContainer
    {
        private VBoxContainer contentContainer;
        private HBoxContainer headerContainer;
        private TextureRect iconRect;
        private Label titleLabel;
        private Label descriptionLabel;
        private HSeparator separator;
        private Label shortcutLabel;
        private Timer hideTimer;
        
        private Control targetControl;
        private Vector2 offset = new Vector2(10, 10);
        private float fadeInDuration = 0.2f;
        private float fadeOutDuration = 0.15f;
        private float showDelay = 0.5f;
        
        public override void _Ready()
        {
            // Set up the tooltip structure
            MouseFilter = MouseFilterEnum.Ignore;
            ZIndex = 1000; // Always on top
            Visible = false;
            
            // Apply modern tooltip style
            AddThemeStyleboxOverride("panel", ModernTheme.CreateTooltipStyle());
            
            // Create content structure
            contentContainer = new VBoxContainer();
            contentContainer.AddThemeConstantOverride("separation", 8);
            AddChild(contentContainer);
            
            // Header with icon and title
            headerContainer = new HBoxContainer();
            headerContainer.AddThemeConstantOverride("separation", 8);
            contentContainer.AddChild(headerContainer);
            
            iconRect = new TextureRect();
            iconRect.CustomMinimumSize = new Vector2(24, 24);
            iconRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
            iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            headerContainer.AddChild(iconRect);
            
            titleLabel = new Label();
            titleLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.TextPrimary);
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
            headerContainer.AddChild(titleLabel);
            
            // Separator
            separator = new HSeparator();
            separator.AddThemeColorOverride("separation", ModernTheme.Colors.BackgroundSurface);
            contentContainer.AddChild(separator);
            
            // Description
            descriptionLabel = new Label();
            descriptionLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.TextSecondary);
            descriptionLabel.AddThemeFontSizeOverride("font_size", 14);
            descriptionLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            descriptionLabel.CustomMinimumSize = new Vector2(300, 0);
            contentContainer.AddChild(descriptionLabel);
            
            // Keyboard shortcut
            shortcutLabel = new Label();
            shortcutLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.Primary);
            shortcutLabel.AddThemeFontSizeOverride("font_size", 12);
            shortcutLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
            contentContainer.AddChild(shortcutLabel);
            
            // Hide timer
            hideTimer = new Timer();
            hideTimer.WaitTime = 0.1f;
            hideTimer.OneShot = true;
            hideTimer.Timeout += Hide;
            AddChild(hideTimer);
        }
        
        public void ShowTooltip(Control control, TooltipData data)
        {
            targetControl = control;
            
            // Update content
            if (data.Icon != null)
            {
                iconRect.Texture = data.Icon;
                iconRect.Visible = true;
            }
            else
            {
                iconRect.Visible = false;
            }
            
            titleLabel.Text = data.Title;
            titleLabel.Visible = !string.IsNullOrEmpty(data.Title);
            
            descriptionLabel.Text = data.Description;
            descriptionLabel.Visible = !string.IsNullOrEmpty(data.Description);
            
            separator.Visible = titleLabel.Visible && descriptionLabel.Visible;
            
            if (!string.IsNullOrEmpty(data.ShortcutText))
            {
                shortcutLabel.Text = $"Shortcut: {data.ShortcutText}";
                shortcutLabel.Visible = true;
            }
            else
            {
                shortcutLabel.Visible = false;
            }
            
            // Position the tooltip
            UpdatePosition();
            
            // Animate in
            Visible = true;
            Modulate = new Color(1, 1, 1, 0);
            
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, fadeInDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
        }
        
        public new void Hide()
        {
            if (!Visible) return;
            
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, fadeOutDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.In);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }
        
        public void StartHideTimer()
        {
            hideTimer.Start();
        }
        
        public void StopHideTimer()
        {
            hideTimer.Stop();
        }
        
        private void UpdatePosition()
        {
            if (targetControl == null) return;
            
            var viewport = GetViewport();
            var viewportSize = viewport.GetVisibleRect().Size;
            var globalPos = targetControl.GlobalPosition;
            var targetSize = targetControl.Size;
            
            // Calculate tooltip size
            var tooltipSize = Size;
            
            // Try to position below the control
            var pos = globalPos + new Vector2(0, targetSize.Y) + offset;
            
            // Check if tooltip goes off-screen and adjust
            if (pos.X + tooltipSize.X > viewportSize.X)
            {
                pos.X = viewportSize.X - tooltipSize.X - offset.X;
            }
            
            if (pos.Y + tooltipSize.Y > viewportSize.Y)
            {
                // Position above the control instead
                pos.Y = globalPos.Y - tooltipSize.Y - offset.Y;
            }
            
            // Ensure tooltip doesn't go off the left or top edge
            pos.X = Mathf.Max(offset.X, pos.X);
            pos.Y = Mathf.Max(offset.Y, pos.Y);
            
            GlobalPosition = pos;
        }
        
        public override void _Process(double delta)
        {
            if (Visible && targetControl != null)
            {
                UpdatePosition();
            }
        }
    }
    
    /// <summary>
    /// Data structure for tooltip content
    /// </summary>
    public class TooltipData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Texture2D Icon { get; set; }
        public string ShortcutText { get; set; }
        
        public TooltipData(string title, string description, Texture2D icon = null, string shortcut = null)
        {
            Title = title;
            Description = description;
            Icon = icon;
            ShortcutText = shortcut;
        }
    }
    
    /// <summary>
    /// Helper class to easily add tooltips to controls
    /// </summary>
    public static class TooltipHelper
    {
        private static Dictionary<Control, TooltipData> tooltipData = new Dictionary<Control, TooltipData>();
        private static RichTooltip sharedTooltip;
        private static Timer showTimer;
        private static Control pendingControl;
        
        public static void Initialize(Node rootNode)
        {
            // Create shared tooltip instance
            sharedTooltip = new RichTooltip();
            rootNode.AddChild(sharedTooltip);
            
            // Create show timer
            showTimer = new Timer();
            showTimer.WaitTime = 0.5f;
            showTimer.OneShot = true;
            showTimer.Timeout += () =>
            {
                if (pendingControl != null && tooltipData.ContainsKey(pendingControl))
                {
                    sharedTooltip.ShowTooltip(pendingControl, tooltipData[pendingControl]);
                }
            };
            rootNode.AddChild(showTimer);
        }
        
        public static void AddTooltip(Control control, string title, string description, Texture2D icon = null, string shortcut = null)
        {
            var data = new TooltipData(title, description, icon, shortcut);
            tooltipData[control] = data;
            
            // Connect mouse events
            control.MouseEntered += () => OnControlMouseEntered(control);
            control.MouseExited += () => OnControlMouseExited(control);
            control.TreeExiting += () => RemoveTooltip(control);
        }
        
        public static void RemoveTooltip(Control control)
        {
            if (tooltipData.ContainsKey(control))
            {
                tooltipData.Remove(control);
            }
        }
        
        private static void OnControlMouseEntered(Control control)
        {
            pendingControl = control;
            showTimer.Start();
            sharedTooltip?.StopHideTimer();
        }
        
        private static void OnControlMouseExited(Control control)
        {
            pendingControl = null;
            showTimer.Stop();
            sharedTooltip?.StartHideTimer();
        }
    }
}