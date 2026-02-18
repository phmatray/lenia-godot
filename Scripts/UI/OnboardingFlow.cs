using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Lenia.UI
{
    /// <summary>
    /// Interactive onboarding flow for new users with step-by-step guidance
    /// </summary>
    public partial class OnboardingFlow : Control
    {
        private Panel dimmerPanel;
        private Control spotlightMask;
        private PanelContainer tooltipPanel;
        private VBoxContainer tooltipContent;
        private Label stepLabel;
        private Label titleLabel;
        private Label descriptionLabel;
        private HBoxContainer buttonContainer;
        private Button skipButton;
        private Button backButton;
        private Button nextButton;
        
        private List<OnboardingStep> steps;
        private int currentStep = 0;
        private bool isActive = false;
        
        [Signal]
        public delegate void OnboardingCompletedEventHandler();
        
        public override void _Ready()
        {
            // Fill viewport
            SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Pass;
            Visible = false;
            ZIndex = 998;
            
            // Create dimmer overlay
            dimmerPanel = new Panel();
            dimmerPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            dimmerPanel.MouseFilter = MouseFilterEnum.Pass;
            
            var dimmerStyle = new StyleBoxFlat();
            dimmerStyle.BgColor = new Color(0, 0, 0, 0.8f);
            dimmerPanel.AddThemeStyleboxOverride("panel", dimmerStyle);
            AddChild(dimmerPanel);
            
            // Create spotlight mask (will be positioned dynamically)
            spotlightMask = new Control();
            spotlightMask.MouseFilter = MouseFilterEnum.Pass;
            AddChild(spotlightMask);
            
            // Create tooltip panel
            tooltipPanel = new PanelContainer();
            tooltipPanel.AddThemeStyleboxOverride("panel", CreateOnboardingTooltipStyle());
            tooltipPanel.MouseFilter = MouseFilterEnum.Stop;
            AddChild(tooltipPanel);
            
            // Tooltip content
            tooltipContent = new VBoxContainer();
            tooltipContent.AddThemeConstantOverride("separation", 12);
            tooltipPanel.AddChild(tooltipContent);
            
            // Step indicator
            stepLabel = new Label();
            stepLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.Primary);
            stepLabel.AddThemeFontSizeOverride("font_size", 12);
            tooltipContent.AddChild(stepLabel);
            
            // Title
            titleLabel = new Label();
            titleLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.TextPrimary);
            titleLabel.AddThemeFontSizeOverride("font_size", 20);
            tooltipContent.AddChild(titleLabel);
            
            // Description
            descriptionLabel = new Label();
            descriptionLabel.AddThemeColorOverride("font_color", ModernTheme.Colors.TextSecondary);
            descriptionLabel.AddThemeFontSizeOverride("font_size", 14);
            descriptionLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            descriptionLabel.CustomMinimumSize = new Vector2(350, 0);
            tooltipContent.AddChild(descriptionLabel);
            
            // Separator
            var separator = new HSeparator();
            separator.AddThemeColorOverride("separation", ModernTheme.Colors.BackgroundSurface);
            tooltipContent.AddChild(separator);
            
            // Button container
            buttonContainer = new HBoxContainer();
            buttonContainer.AddThemeConstantOverride("separation", 8);
            buttonContainer.Alignment = BoxContainer.AlignmentMode.End;
            tooltipContent.AddChild(buttonContainer);
            
            // Skip button
            skipButton = new Button();
            skipButton.Text = "Skip Tour";
            skipButton.AddThemeColorOverride("font_color", ModernTheme.Colors.TextMuted);
            skipButton.Flat = true;
            skipButton.Pressed += SkipOnboarding;
            buttonContainer.AddChild(skipButton);
            
            // Spacer
            var spacer = new Control();
            spacer.CustomMinimumSize = new Vector2(100, 0);
            spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            buttonContainer.AddChild(spacer);
            
            // Back button
            backButton = new Button();
            backButton.Text = "Back";
            backButton.AddThemeStyleboxOverride("normal", ModernTheme.NeumorphicButton.CreateNormal());
            backButton.AddThemeStyleboxOverride("hover", ModernTheme.NeumorphicButton.CreateHover());
            backButton.AddThemeStyleboxOverride("pressed", ModernTheme.NeumorphicButton.CreatePressed());
            backButton.Pressed += PreviousStep;
            buttonContainer.AddChild(backButton);
            
            // Next button
            nextButton = new Button();
            nextButton.Text = "Next";
            nextButton.AddThemeStyleboxOverride("normal", CreatePrimaryButtonStyle());
            nextButton.AddThemeStyleboxOverride("hover", CreatePrimaryButtonHoverStyle());
            nextButton.AddThemeStyleboxOverride("pressed", CreatePrimaryButtonPressedStyle());
            nextButton.Pressed += NextStep;
            buttonContainer.AddChild(nextButton);
            
            // Initialize steps
            InitializeSteps();
        }
        
        private void InitializeSteps()
        {
            steps = new List<OnboardingStep>
            {
                new OnboardingStep
                {
                    Title = "Welcome to Lenia! 👋",
                    Description = "Lenia is a continuous cellular automaton that creates beautiful, life-like patterns. Let's take a quick tour to get you started.",
                    TargetPath = null, // No specific target for welcome
                    HighlightSize = Vector2.Zero
                },
                
                new OnboardingStep
                {
                    Title = "The Simulation Canvas",
                    Description = "This is where the magic happens! The canvas displays the Lenia simulation. You can draw patterns, zoom, and pan to explore.",
                    TargetPath = "VBoxContainer/MiddleSection/SimulationCanvas",
                    HighlightSize = new Vector2(600, 400)
                },
                
                new OnboardingStep
                {
                    Title = "Drawing Tools",
                    Description = "Use these tools to interact with the simulation. The brush adds cells, the eraser removes them, and the picker samples values.",
                    TargetPath = "VBoxContainer/MiddleSection/LeftToolbar",
                    HighlightSize = new Vector2(60, 200)
                },
                
                new OnboardingStep
                {
                    Title = "Simulation Controls",
                    Description = "Control the simulation with these buttons. Play/pause, step through frames, or reset to start fresh.",
                    TargetPath = "VBoxContainer/HeaderBar/SimulationControls",
                    HighlightSize = new Vector2(200, 50)
                },
                
                new OnboardingStep
                {
                    Title = "Parameters Panel",
                    Description = "Fine-tune the simulation behavior here. Adjust growth parameters, kernel settings, and more to create unique patterns.",
                    TargetPath = "VBoxContainer/MiddleSection/RightSidebar",
                    HighlightSize = new Vector2(300, 400)
                },
                
                new OnboardingStep
                {
                    Title = "Pattern Gallery",
                    Description = "Browse and load pre-made patterns from the gallery. It's a great way to explore what Lenia can create!",
                    TargetPath = "VBoxContainer/HeaderBar/GalleryButton",
                    HighlightSize = new Vector2(100, 40)
                },
                
                new OnboardingStep
                {
                    Title = "You're Ready! 🎉",
                    Description = "That's all for now! Press F1 anytime to see keyboard shortcuts, or click the help button for more tutorials. Have fun exploring!",
                    TargetPath = null,
                    HighlightSize = Vector2.Zero
                }
            };
        }
        
        public void StartOnboarding()
        {
            if (isActive) return;
            
            isActive = true;
            currentStep = 0;
            Visible = true;
            
            // Animate entrance
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
            tween.TweenCallback(Callable.From(() => ShowStep(currentStep)));
        }
        
        private void ShowStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= steps.Count) return;
            
            var step = steps[stepIndex];
            
            // Update content
            stepLabel.Text = $"Step {stepIndex + 1} of {steps.Count}";
            titleLabel.Text = step.Title;
            descriptionLabel.Text = step.Description;
            
            // Update buttons
            backButton.Visible = stepIndex > 0;
            nextButton.Text = stepIndex == steps.Count - 1 ? "Finish" : "Next";
            
            // Position spotlight and tooltip
            if (!string.IsNullOrEmpty(step.TargetPath))
            {
                var targetNode = GetNode<Control>(step.TargetPath);
                if (targetNode != null)
                {
                    HighlightControl(targetNode, step.HighlightSize);
                    PositionTooltip(targetNode);
                }
            }
            else
            {
                // Center tooltip for steps without targets
                ClearHighlight();
                CenterTooltip();
            }
            
            // Animate tooltip entrance
            AnimateTooltipEntrance();
        }
        
        private void HighlightControl(Control target, Vector2 highlightSize)
        {
            // Create a spotlight effect around the target control
            var globalPos = target.GlobalPosition;
            var size = highlightSize != Vector2.Zero ? highlightSize : target.Size;
            
            // Update dimmer with a hole for the spotlight
            var viewport = GetViewport().GetVisibleRect();
            
            // For now, just position a transparent area
            // In a full implementation, you'd use a shader or multiple panels
            spotlightMask.Position = globalPos - new Vector2(20, 20);
            spotlightMask.Size = size + new Vector2(40, 40);
            
            // Add glow effect
            var glowStyle = new StyleBoxFlat();
            glowStyle.DrawCenter = false;
            glowStyle.BorderWidthLeft = 3;
            glowStyle.BorderWidthTop = 3;
            glowStyle.BorderWidthRight = 3;
            glowStyle.BorderWidthBottom = 3;
            glowStyle.BorderColor = ModernTheme.Colors.PrimaryGlow;
            glowStyle.SetCornerRadiusAll(8);
            spotlightMask.AddThemeStyleboxOverride("panel", glowStyle);
        }
        
        private void ClearHighlight()
        {
            spotlightMask.Position = Vector2.Zero;
            spotlightMask.Size = Vector2.Zero;
        }
        
        private void PositionTooltip(Control target)
        {
            var viewport = GetViewport().GetVisibleRect();
            var targetGlobalPos = target.GlobalPosition;
            var targetSize = target.Size;
            var tooltipSize = tooltipPanel.Size;
            
            // Try to position to the right of the target
            var pos = targetGlobalPos + new Vector2(targetSize.X + 20, 0);
            
            // Adjust if it goes off-screen
            if (pos.X + tooltipSize.X > viewport.Size.X - 20)
            {
                // Position to the left instead
                pos.X = targetGlobalPos.X - tooltipSize.X - 20;
            }
            
            if (pos.Y + tooltipSize.Y > viewport.Size.Y - 20)
            {
                pos.Y = viewport.Size.Y - tooltipSize.Y - 20;
            }
            
            pos.Y = Mathf.Max(20, pos.Y);
            pos.X = Mathf.Max(20, pos.X);
            
            tooltipPanel.Position = pos;
        }
        
        private void CenterTooltip()
        {
            var viewport = GetViewport().GetVisibleRect();
            tooltipPanel.Position = (viewport.Size - tooltipPanel.Size) / 2;
        }
        
        private void AnimateTooltipEntrance()
        {
            tooltipPanel.Scale = Vector2.One * 0.9f;
            tooltipPanel.Modulate = new Color(1, 1, 1, 0);
            tooltipPanel.PivotOffset = tooltipPanel.Size / 2;
            
            var tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(tooltipPanel, "scale", Vector2.One, 0.3f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(tooltipPanel, "modulate:a", 1.0f, 0.3f);
        }
        
        private void NextStep()
        {
            if (currentStep < steps.Count - 1)
            {
                currentStep++;
                ShowStep(currentStep);
            }
            else
            {
                CompleteOnboarding();
            }
        }
        
        private void PreviousStep()
        {
            if (currentStep > 0)
            {
                currentStep--;
                ShowStep(currentStep);
            }
        }
        
        private void SkipOnboarding()
        {
            CompleteOnboarding();
        }
        
        private void CompleteOnboarding()
        {
            isActive = false;
            
            // Save that onboarding has been completed
            var config = new ConfigFile();
            config.SetValue("onboarding", "completed", true);
            config.Save("user://settings.cfg");
            
            // Animate exit
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.3f);
            tween.TweenCallback(Callable.From(() =>
            {
                Visible = false;
                EmitSignal(SignalName.OnboardingCompleted);
            }));
        }
        
        public static bool ShouldShowOnboarding()
        {
            var config = new ConfigFile();
            var err = config.Load("user://settings.cfg");
            if (err != Error.Ok)
            {
                return true; // Show onboarding if no config exists
            }
            
            return !config.GetValue("onboarding", "completed", false).AsBool();
        }
        
        private StyleBoxFlat CreateOnboardingTooltipStyle()
        {
            var style = ModernTheme.CreateGlassPanel(12.0f, 2.0f);
            style.BgColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);
            style.ShadowSize = 12;
            style.ShadowColor = new Color(0, 0, 0, 0.6f);
            return style;
        }
        
        private StyleBoxFlat CreatePrimaryButtonStyle()
        {
            var style = ModernTheme.NeumorphicButton.CreateNormal();
            style.BgColor = ModernTheme.Colors.Primary;
            return style;
        }
        
        private StyleBoxFlat CreatePrimaryButtonHoverStyle()
        {
            var style = ModernTheme.NeumorphicButton.CreateHover();
            style.BgColor = ModernTheme.Colors.PrimaryLight;
            return style;
        }
        
        private StyleBoxFlat CreatePrimaryButtonPressedStyle()
        {
            var style = ModernTheme.NeumorphicButton.CreatePressed();
            style.BgColor = ModernTheme.Colors.PrimaryDark;
            return style;
        }
        
        private class OnboardingStep
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string TargetPath { get; set; }
            public Vector2 HighlightSize { get; set; }
        }
    }
}