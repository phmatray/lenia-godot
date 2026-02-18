using Godot;
using System;
using System.Collections.Generic;

namespace Lenia.UI
{
    /// <summary>
    /// Provides smooth animations and transitions for UI elements
    /// </summary>
    public partial class AnimationSystem : Node
    {
        private static AnimationSystem _instance;
        public static AnimationSystem Instance => _instance;
        
        private Dictionary<Control, List<Tween>> activeTweens = new Dictionary<Control, List<Tween>>();
        
        public override void _Ready()
        {
            _instance = this;
            ProcessMode = ProcessModeEnum.Always; // Continue animations even when paused
        }
        
        /// <summary>
        /// Animate a control's entrance with various effects
        /// </summary>
        public static void AnimateEntrance(Control control, EntranceType type = EntranceType.FadeScale, float delay = 0.0f)
        {
            if (Instance == null) return;
            
            var tween = control.CreateTween();
            Instance.RegisterTween(control, tween);
            
            switch (type)
            {
                case EntranceType.FadeIn:
                    control.Modulate = new Color(1, 1, 1, 0);
                    tween.TweenProperty(control, "modulate:a", 1.0f, ModernTheme.Animations.NormalDuration)
                        .SetDelay(delay)
                        .SetTrans(ModernTheme.Animations.DefaultTransition)
                        .SetEase(ModernTheme.Animations.DefaultEase);
                    break;
                    
                case EntranceType.SlideFromLeft:
                    var originalX = control.Position.X;
                    control.Position = new Vector2(-control.Size.X, control.Position.Y);
                    tween.TweenProperty(control, "position:x", originalX, ModernTheme.Animations.NormalDuration)
                        .SetDelay(delay)
                        .SetTrans(ModernTheme.Animations.BounceTransition)
                        .SetEase(ModernTheme.Animations.BounceEase);
                    break;
                    
                case EntranceType.SlideFromRight:
                    var origX = control.Position.X;
                    control.Position = new Vector2(control.GetViewportRect().Size.X, control.Position.Y);
                    tween.TweenProperty(control, "position:x", origX, ModernTheme.Animations.NormalDuration)
                        .SetDelay(delay)
                        .SetTrans(ModernTheme.Animations.BounceTransition)
                        .SetEase(ModernTheme.Animations.BounceEase);
                    break;
                    
                case EntranceType.SlideFromTop:
                    var originalY = control.Position.Y;
                    control.Position = new Vector2(control.Position.X, -control.Size.Y);
                    tween.TweenProperty(control, "position:y", originalY, ModernTheme.Animations.NormalDuration)
                        .SetDelay(delay)
                        .SetTrans(ModernTheme.Animations.BounceTransition)
                        .SetEase(ModernTheme.Animations.BounceEase);
                    break;
                    
                case EntranceType.FadeScale:
                    control.Modulate = new Color(1, 1, 1, 0);
                    control.Scale = Vector2.One * 0.8f;
                    control.PivotOffset = control.Size / 2;
                    
                    tween.Parallel();
                    tween.TweenProperty(control, "modulate:a", 1.0f, ModernTheme.Animations.NormalDuration)
                        .SetDelay(delay);
                    tween.TweenProperty(control, "scale", Vector2.One, ModernTheme.Animations.NormalDuration)
                        .SetDelay(delay)
                        .SetTrans(ModernTheme.Animations.BounceTransition)
                        .SetEase(ModernTheme.Animations.BounceEase);
                    break;
                    
                case EntranceType.Bounce:
                    control.Scale = Vector2.Zero;
                    control.PivotOffset = control.Size / 2;
                    tween.TweenProperty(control, "scale", Vector2.One, ModernTheme.Animations.SlowDuration)
                        .SetDelay(delay)
                        .SetTrans(Tween.TransitionType.Elastic)
                        .SetEase(Tween.EaseType.Out);
                    break;
            }
            
            tween.Finished += () => Instance.CleanupTween(control, tween);
        }
        
        /// <summary>
        /// Animate hover effect on a control
        /// </summary>
        public static void AnimateHover(Control control, bool isHovering)
        {
            if (Instance == null) return;
            
            var tween = control.CreateTween();
            Instance.RegisterTween(control, tween);
            
            if (isHovering)
            {
                // Scale up slightly and add glow
                control.PivotOffset = control.Size / 2;
                tween.Parallel();
                tween.TweenProperty(control, "scale", Vector2.One * 1.05f, ModernTheme.Animations.FastDuration)
                    .SetTrans(ModernTheme.Animations.DefaultTransition)
                    .SetEase(ModernTheme.Animations.DefaultEase);
                
                // Add glow effect if it's a button
                if (control is Button)
                {
                    tween.TweenProperty(control, "modulate", ModernTheme.Colors.PrimaryGlow, ModernTheme.Animations.FastDuration);
                }
            }
            else
            {
                // Return to normal
                tween.Parallel();
                tween.TweenProperty(control, "scale", Vector2.One, ModernTheme.Animations.FastDuration)
                    .SetTrans(ModernTheme.Animations.DefaultTransition)
                    .SetEase(ModernTheme.Animations.DefaultEase);
                
                if (control is Button)
                {
                    tween.TweenProperty(control, "modulate", Colors.White, ModernTheme.Animations.FastDuration);
                }
            }
            
            tween.Finished += () => Instance.CleanupTween(control, tween);
        }
        
        /// <summary>
        /// Animate a pulse effect (useful for notifications or attention)
        /// </summary>
        public static void AnimatePulse(Control control, int pulseCount = 2, Color? pulseColor = null)
        {
            if (Instance == null) return;
            
            var tween = control.CreateTween();
            Instance.RegisterTween(control, tween);
            
            var originalModulate = control.Modulate;
            var targetColor = pulseColor ?? ModernTheme.Colors.AccentGlow;
            
            tween.SetLoops(pulseCount);
            tween.TweenProperty(control, "modulate", targetColor, 0.3f);
            tween.TweenProperty(control, "modulate", originalModulate, 0.3f);
            
            tween.Finished += () => Instance.CleanupTween(control, tween);
        }
        
        /// <summary>
        /// Shake animation (useful for errors or invalid input)
        /// </summary>
        public static void AnimateShake(Control control, float intensity = 10.0f)
        {
            if (Instance == null) return;
            
            var tween = control.CreateTween();
            Instance.RegisterTween(control, tween);
            
            var originalPos = control.Position;
            var shakeDuration = 0.05f;
            
            for (int i = 0; i < 6; i++)
            {
                var offset = new Vector2(
                    (float)GD.RandRange(-intensity, intensity),
                    (float)GD.RandRange(-intensity * 0.5f, intensity * 0.5f)
                );
                tween.TweenProperty(control, "position", originalPos + offset, shakeDuration);
            }
            
            tween.TweenProperty(control, "position", originalPos, shakeDuration);
            tween.Finished += () => Instance.CleanupTween(control, tween);
        }
        
        /// <summary>
        /// Animate panel expansion/collapse
        /// </summary>
        public static void AnimateExpand(Control control, bool expand, float targetHeight)
        {
            if (Instance == null) return;
            
            var tween = control.CreateTween();
            Instance.RegisterTween(control, tween);
            
            if (expand)
            {
                control.CustomMinimumSize = new Vector2(control.CustomMinimumSize.X, 0);
                control.Visible = true;
                tween.TweenProperty(control, "custom_minimum_size:y", targetHeight, ModernTheme.Animations.NormalDuration)
                    .SetTrans(ModernTheme.Animations.DefaultTransition)
                    .SetEase(ModernTheme.Animations.DefaultEase);
            }
            else
            {
                tween.TweenProperty(control, "custom_minimum_size:y", 0.0f, ModernTheme.Animations.NormalDuration)
                    .SetTrans(ModernTheme.Animations.DefaultTransition)
                    .SetEase(ModernTheme.Animations.DefaultEase);
                tween.TweenCallback(Callable.From(() => control.Visible = false));
            }
            
            tween.Finished += () => Instance.CleanupTween(control, tween);
        }
        
        /// <summary>
        /// Ripple effect from a point (useful for click feedback)
        /// </summary>
        public static void AnimateRipple(Control control, Vector2 localPosition)
        {
            if (Instance == null || control == null) return;
            
            // Create a ripple effect node
            var ripple = new ColorRect();
            ripple.Color = ModernTheme.Colors.PrimaryGlow;
            ripple.Size = Vector2.One * 10;
            ripple.Position = localPosition - ripple.Size / 2;
            ripple.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            // Make it circular
            var circle = new StyleBoxFlat();
            circle.SetCornerRadiusAll(5);
            circle.BgColor = ModernTheme.Colors.PrimaryGlow;
            ripple.AddThemeStyleboxOverride("panel", circle);
            
            control.AddChild(ripple);
            
            var tween = ripple.CreateTween();
            tween.SetParallel();
            
            // Expand and fade out
            tween.TweenProperty(ripple, "scale", Vector2.One * 10, 0.5f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(ripple, "modulate:a", 0.0f, 0.5f);
            
            tween.Finished += () => ripple.QueueFree();
        }
        
        /// <summary>
        /// Smooth value change animation (useful for progress bars, sliders)
        /// </summary>
        public static void AnimateValue(Node node, string property, float targetValue, float duration = 0.3f)
        {
            if (Instance == null) return;
            
            var tween = node.CreateTween();
            
            tween.TweenProperty(node, property, targetValue, duration)
                .SetTrans(ModernTheme.Animations.DefaultTransition)
                .SetEase(ModernTheme.Animations.DefaultEase);
        }
        
        /// <summary>
        /// Stop all animations on a control
        /// </summary>
        public static void StopAnimations(Control control)
        {
            if (Instance == null) return;
            Instance.StopControlAnimations(control);
        }
        
        private void RegisterTween(Control control, Tween tween)
        {
            if (!activeTweens.ContainsKey(control))
            {
                activeTweens[control] = new List<Tween>();
            }
            activeTweens[control].Add(tween);
        }
        
        private void CleanupTween(Control control, Tween tween)
        {
            if (activeTweens.ContainsKey(control))
            {
                activeTweens[control].Remove(tween);
                if (activeTweens[control].Count == 0)
                {
                    activeTweens.Remove(control);
                }
            }
        }
        
        private void StopControlAnimations(Control control)
        {
            if (activeTweens.ContainsKey(control))
            {
                foreach (var tween in activeTweens[control])
                {
                    tween.Kill();
                }
                activeTweens.Remove(control);
            }
        }
        
        public enum EntranceType
        {
            FadeIn,
            SlideFromLeft,
            SlideFromRight,
            SlideFromTop,
            FadeScale,
            Bounce
        }
    }
}