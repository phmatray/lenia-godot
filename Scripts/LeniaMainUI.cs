using Godot;
using Lenia.UI;

public partial class LeniaMainUI : Control
{
    private LeniaSimulation simulation;
    private HeaderBar headerBar;
    private LeftToolbar leftToolbar;
    private SimulationCanvas simulationCanvas;
    private RightSidebar rightSidebar;
    private StatusBar statusBar;
    
    // New fun features
    private AudioFeedback audioFeedback;
    private ParticleEffects particleEffects;
    private PatternLibrary patternLibrary;
    private TimelapseRecorder timelapseRecorder;
    private TutorialSystem tutorialSystem;
    private ChallengeSystem challengeSystem;
    
    // Modern UI components
    private UIInitializer uiInitializer;
    private KeyboardShortcutsOverlay shortcutsOverlay;
    private OnboardingFlow onboardingFlow;
    
    public override void _Ready()
    {
        GD.Print("LeniaMainUI _Ready called");
        
        // Initialize modern UI system first
        InitializeModernUI();
        
        // Get the simulation node
        simulation = GetNode<LeniaSimulation>("/root/Main/LeniaSimulation");
        if (simulation == null)
        {
            GD.PrintErr("Failed to get LeniaSimulation node!");
            return;
        }
        
        GD.Print("Successfully got LeniaSimulation node");
        
        // Get UI components from the scene
        headerBar = GetNode<HeaderBar>("MarginContainer/VBoxContainer/HeaderBar");
        leftToolbar = GetNode<LeftToolbar>("MarginContainer/VBoxContainer/MiddleSection/LeftToolbar");
        simulationCanvas = GetNode<SimulationCanvas>("MarginContainer/VBoxContainer/MiddleSection/SimulationCanvas");
        rightSidebar = GetNode<RightSidebar>("MarginContainer/VBoxContainer/MiddleSection/RightSidebar");
        statusBar = GetNode<StatusBar>("MarginContainer/VBoxContainer/StatusBar");
        
        // Initialize components with simulation reference
        headerBar.Initialize(simulation);
        simulationCanvas.Initialize(simulation);
        rightSidebar.Initialize(simulation);
        statusBar.Initialize(simulation);
        
        // Initialize new features
        InitializeNewFeatures();
        
        // Initialize modern UI/UX system
        InitializeModernUI();
        
        // Connect signals
        leftToolbar.ToolChanged += OnToolChanged;
    }
    
    private void InitializeModernUI()
    {
        // Create UI initializer
        uiInitializer = new UIInitializer();
        uiInitializer.Name = "UIInitializer";
        AddChild(uiInitializer);
        
        // Get references to overlays if they exist
        shortcutsOverlay = GetNodeOrNull<KeyboardShortcutsOverlay>("/root/KeyboardShortcutsOverlay");
        onboardingFlow = GetNodeOrNull<OnboardingFlow>("/root/OnboardingFlow");
    }
    
    private void ApplyModernEnhancements()
    {
        // Add entrance animations to main UI sections
        if (headerBar != null)
        {
            AnimationSystem.AnimateEntrance(headerBar, AnimationSystem.EntranceType.SlideFromTop, 0.1f);
        }
        
        if (leftToolbar != null)
        {
            AnimationSystem.AnimateEntrance(leftToolbar, AnimationSystem.EntranceType.SlideFromLeft, 0.2f);
        }
        
        if (simulationCanvas != null)
        {
            AnimationSystem.AnimateEntrance(simulationCanvas, AnimationSystem.EntranceType.FadeScale, 0.3f);
        }
        
        if (rightSidebar != null)
        {
            AnimationSystem.AnimateEntrance(rightSidebar, AnimationSystem.EntranceType.SlideFromRight, 0.4f);
        }
        
        if (statusBar != null)
        {
            AnimationSystem.AnimateEntrance(statusBar, AnimationSystem.EntranceType.FadeIn, 0.5f);
        }
        
        // Add tooltips to main controls
        AddModernTooltips();
    }
    
    private void AddModernTooltips()
    {
        // Add tooltips to toolbar buttons if they exist
        var playButton = GetNodeOrNull<Button>("MarginContainer/VBoxContainer/HeaderBar/HBoxContainer/PlayPauseButton");
        if (playButton != null)
        {
            TooltipHelper.AddTooltip(playButton, "Play/Pause", "Toggle simulation playback", null, "Space");
        }
        
        var resetButton = GetNodeOrNull<Button>("MarginContainer/VBoxContainer/HeaderBar/HBoxContainer/ResetButton");
        if (resetButton != null)
        {
            TooltipHelper.AddTooltip(resetButton, "Reset", "Clear the grid and reset simulation", null, "R");
        }
        
        var stepButton = GetNodeOrNull<Button>("MarginContainer/VBoxContainer/HeaderBar/HBoxContainer/StepButton");
        if (stepButton != null)
        {
            TooltipHelper.AddTooltip(stepButton, "Step", "Advance simulation by one frame", null, "S");
        }
    }
    
    private void InitializeNewFeatures()
    {
        // Create and add audio feedback system
        audioFeedback = new AudioFeedback();
        AddChild(audioFeedback);
        
        // Create and add particle effects
        particleEffects = new ParticleEffects();
        simulationCanvas.AddChild(particleEffects); // Add to canvas for proper positioning
        particleEffects.Initialize(simulation);
        
        // Create pattern library (initially hidden)
        patternLibrary = new PatternLibrary();
        AddChild(patternLibrary);
        patternLibrary.Initialize(simulation);
        patternLibrary.Visible = false;
        
        // Create time-lapse recorder
        timelapseRecorder = new TimelapseRecorder();
        AddChild(timelapseRecorder);
        timelapseRecorder.Initialize(simulation);
        
        // Create tutorial system
        tutorialSystem = new TutorialSystem();
        AddChild(tutorialSystem);
        tutorialSystem.Initialize(simulation);
        
        // Create challenge system
        challengeSystem = new ChallengeSystem();
        AddChild(challengeSystem);
        challengeSystem.Initialize(simulation);
        
        // Connect feature signals
        ConnectFeatureSignals();
        
        // Show first-time tutorial if needed - TEMPORARILY DISABLED FOR DEBUGGING
        // if (tutorialSystem.ShouldShowFirstTimeTutorial())
        // {
        //     GD.Print("Auto-starting first-time tutorial in 1 second");
        //     GetTree().CreateTimer(1.0).Timeout += () => {
        //         GD.Print("Auto-starting first-time tutorial now");
        //         tutorialSystem.StartTutorial("first_time");
        //     };
        // }
        GD.Print("Tutorial auto-start is disabled for debugging");
    }
    
    private void ConnectFeatureSignals()
    {
        // Pattern library signals
        patternLibrary.PatternSelected += OnPatternSelected;
        
        // Tutorial signals
        tutorialSystem.TutorialCompleted += OnTutorialCompleted;
        
        // Challenge signals
        challengeSystem.ChallengeCompleted += OnChallengeCompleted;
        challengeSystem.AchievementUnlocked += OnAchievementUnlocked;
        
        // Time-lapse signals
        timelapseRecorder.RecordingStarted += OnRecordingStarted;
        timelapseRecorder.RecordingStopped += OnRecordingStopped;
    }
    
    private void OnToolChanged(LeftToolbar.Tool tool)
    {
        simulationCanvas.SetCurrentTool(tool);
    }
    
    private void OnPatternSelected(string patternId)
    {
        GD.Print($"Pattern selected: {patternId}");
        audioFeedback?.PlayPatternSound(patternId);
    }
    
    private void OnTutorialCompleted(string tutorialId)
    {
        GD.Print($"Tutorial completed: {tutorialId}");
        tutorialSystem.ShowQuickTip("🎉 Tutorial complete! Well done!", new Vector2(400, 100));
    }
    
    private void OnChallengeCompleted(string challengeId, float score)
    {
        GD.Print($"Challenge completed: {challengeId} with score {score:F1}");
        audioFeedback?.PlayPatternSound("success");
    }
    
    private void OnAchievementUnlocked(string achievementId)
    {
        GD.Print($"Achievement unlocked: {achievementId}");
        audioFeedback?.PlayPatternSound("achievement");
    }
    
    private void OnRecordingStarted()
    {
        GD.Print("Time-lapse recording started");
        statusBar?.UpdateStatus("🎬 Recording...");
    }
    
    private void OnRecordingStopped(string videoPath)
    {
        GD.Print($"Time-lapse recording stopped: {videoPath}");
        statusBar?.UpdateStatus("✅ Recording saved");
        
        // Show completion notification
        tutorialSystem?.ShowQuickTip("📹 Time-lapse saved successfully!", new Vector2(400, 150));
    }
    
    public override void _Process(double delta)
    {
        // Update audio feedback with simulation state
        if (audioFeedback != null && simulation != null)
        {
            var grid = simulation.GetCurrentGrid();
            audioFeedback.UpdateAudio(grid, simulation.GridWidth, simulation.GridHeight, simulation.DeltaTime);
        }
        
        // Update particle effects
        if (particleEffects != null && simulation != null)
        {
            // Set proper scale for particles based on simulation canvas
            var canvasSize = simulationCanvas?.Size ?? Vector2.One;
            var gridSize = new Vector2(simulation?.GridWidth ?? 128, simulation?.GridHeight ?? 128);
            particleEffects.SetGridScale(canvasSize / gridSize);
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        // Handle global hotkeys for new features
        if (@event.IsActionPressed("ui_accept") && Input.IsActionPressed("ui_select"))
        {
            // Ctrl+Enter or similar combo to open pattern library
            TogglePatternLibrary();
        }
        else if (@event.IsActionPressed("ui_cancel") && Input.IsActionPressed("ui_select"))
        {
            // Ctrl+Escape to open tutorial system
            tutorialSystem?.ShowTutorialSelector();
        }
        else if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // F1 for help
            if (keyEvent.Keycode == Key.F1)
            {
                shortcutsOverlay?.Show();
                GetViewport().SetInputAsHandled();
            }
            // F2 for onboarding
            else if (keyEvent.Keycode == Key.F2)
            {
                onboardingFlow?.StartOnboarding();
                GetViewport().SetInputAsHandled();
            }
        }
    }
    
    private void TogglePatternLibrary()
    {
        if (patternLibrary != null)
        {
            patternLibrary.Visible = !patternLibrary.Visible;
        }
    }
    
    // Public methods for other components to access features
    public void ShowPatternLibrary()
    {
        GD.Print("ShowPatternLibrary called - showing pattern library panel");
        patternLibrary?.Show();
    }
    
    public void ShowTutorials()
    {
        GD.Print("ShowTutorials called - opening tutorial selector");
        tutorialSystem?.ShowTutorialSelector();
    }
    
    public void ShowChallenges()
    {
        challengeSystem?.ShowChallengeCenter();
    }
    
    public void StartTimelapseRecording()
    {
        timelapseRecorder?.StartRecording();
    }
    
    public void StopTimelapseRecording()
    {
        timelapseRecorder?.StopRecording();
    }
    
    public void ToggleAudio(bool enabled)
    {
        audioFeedback?.SetAudioEnabled(enabled);
    }
    
    public void ToggleParticles(bool enabled)
    {
        particleEffects?.SetEffectsEnabled(enabled);
    }
    
    public void SetAudioVolume(float volume)
    {
        audioFeedback?.SetVolume(volume);
    }
    
    public void SetParticleIntensity(float intensity)
    {
        particleEffects?.SetParticleIntensity(intensity);
    }
    
    // Method to trigger paint particles when user interacts
    public void OnUserPaintAction(Vector2 position, float intensity, bool isErase = false)
    {
        particleEffects?.SpawnPaintParticles(position, intensity, isErase);
        audioFeedback?.PlayPaintSound(intensity, position);
    }
    
}