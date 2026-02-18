using Godot;

namespace Lenia.UI
{
    /// <summary>
    /// Initializes and configures all modern UI components
    /// </summary>
    public partial class UIInitializer : Node
    {
        private AnimationSystem animationSystem;
        private AccessibilityManager accessibilityManager;
        private KeyboardShortcutsOverlay shortcutsOverlay;
        private OnboardingFlow onboardingFlow;
        
        public override void _Ready()
        {
            GD.Print("Initializing modern UI system...");
            
            // Create singleton systems
            CreateAnimationSystem();
            CreateAccessibilityManager();
            CreateTooltipSystem();
            CreateOverlays();
            
            // Apply modern theme to entire UI
            ApplyModernTheme();
            
            // Initialize components
            InitializeButtons();
            InitializePanels();
            
            // Apply visual bug fixes after UI is loaded
            CallDeferred(nameof(ApplyVisualFixes));
            
            // Check if onboarding should be shown
            if (OnboardingFlow.ShouldShowOnboarding())
            {
                GetTree().CreateTimer(1.0).Timeout += () => onboardingFlow?.StartOnboarding();
            }
            
            GD.Print("Modern UI system initialized successfully!");
        }
        
        private void ApplyVisualFixes()
        {
            GD.Print("Applying visual bug fixes...");
            VisualBugFixes.ApplyVisualFixes(GetTree().Root);
            GD.Print("Visual bug fixes applied!");
        }
        
        private void CreateAnimationSystem()
        {
            animationSystem = new AnimationSystem();
            animationSystem.Name = "AnimationSystem";
            AddChild(animationSystem);
        }
        
        private void CreateAccessibilityManager()
        {
            accessibilityManager = new AccessibilityManager();
            accessibilityManager.Name = "AccessibilityManager";
            AddChild(accessibilityManager);
        }
        
        private void CreateTooltipSystem()
        {
            TooltipHelper.Initialize(GetTree().Root);
        }
        
        private void CreateOverlays()
        {
            // Create keyboard shortcuts overlay
            shortcutsOverlay = new KeyboardShortcutsOverlay();
            shortcutsOverlay.Name = "KeyboardShortcutsOverlay";
            GetTree().Root.AddChild(shortcutsOverlay);
            
            // Create onboarding flow
            onboardingFlow = new OnboardingFlow();
            onboardingFlow.Name = "OnboardingFlow";
            GetTree().Root.AddChild(onboardingFlow);
        }
        
        private void ApplyModernTheme()
        {
            // Apply to root for global effect
            var root = GetTree().Root.GetChild(0); // Get main scene root
            if (root is Control control)
            {
                ModernTheme.ApplyTheme(control);
            }
        }
        
        private void InitializeButtons()
        {
            // Find all buttons and enhance them
            foreach (var node in GetTree().GetNodesInGroup("ui_buttons"))
            {
                if (node is Button button && !(button is ModernButton))
                {
                    EnhanceButton(button);
                }
            }
        }
        
        private void InitializePanels()
        {
            // Find all panels and add entrance animations
            foreach (var node in GetTree().GetNodesInGroup("ui_panels"))
            {
                if (node is Control panel)
                {
                    // Add entrance animation with staggered delay
                    var index = panel.GetIndex();
                    AnimationSystem.AnimateEntrance(panel, 
                        AnimationSystem.EntranceType.FadeScale, 
                        index * 0.1f);
                }
            }
        }
        
        private void EnhanceButton(Button button)
        {
            // Add hover animations
            button.MouseEntered += () => AnimationSystem.AnimateHover(button, true);
            button.MouseExited += () => AnimationSystem.AnimateHover(button, false);
            
            // Add click feedback
            button.ButtonDown += () =>
            {
                AnimationSystem.AnimateRipple(button, button.GetLocalMousePosition());
            };
            
            // Add accessibility
            AccessibilityManager.RegisterControl(button, button.Text, button.TooltipText);
        }
        
        public override void _Input(InputEvent @event)
        {
            // Global keyboard shortcuts
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F1)
            {
                shortcutsOverlay?.Show();
                GetViewport().SetInputAsHandled();
            }
        }
        
        /// <summary>
        /// Show the keyboard shortcuts overlay
        /// </summary>
        public static void ShowKeyboardShortcuts()
        {
            var overlay = (KeyboardShortcutsOverlay)((SceneTree)Engine.GetMainLoop()).Root.GetNodeOrNull("KeyboardShortcutsOverlay");
            overlay?.Show();
        }
        
        /// <summary>
        /// Start the onboarding flow
        /// </summary>
        public static void StartOnboarding()
        {
            var flow = (OnboardingFlow)((SceneTree)Engine.GetMainLoop()).Root.GetNodeOrNull("OnboardingFlow");
            flow?.StartOnboarding();
        }
        
        /// <summary>
        /// Apply a pulse effect to a control (for notifications)
        /// </summary>
        public static void PulseControl(Control control, Color? color = null)
        {
            AnimationSystem.AnimatePulse(control, 2, color);
        }
        
        /// <summary>
        /// Apply a shake effect to a control (for errors)
        /// </summary>
        public static void ShakeControl(Control control)
        {
            AnimationSystem.AnimateShake(control);
        }
    }
}