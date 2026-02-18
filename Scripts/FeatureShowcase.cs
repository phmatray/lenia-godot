using Godot;
using System;

/// <summary>
/// Demonstration class showing how all the fun features integrate together
/// This can be used for testing or as a showcase of capabilities
/// </summary>
public partial class FeatureShowcase : Control
{
    private LeniaMainUI mainUI;
    private LeniaSimulation simulation;
    
    public override void _Ready()
    {
        // Get references
        mainUI = GetNode<LeniaMainUI>("/root/Main/LeniaMainUI");
        simulation = GetNode<LeniaSimulation>("/root/Main/LeniaSimulation");
        
        if (mainUI == null || simulation == null)
        {
            GD.PrintErr("FeatureShowcase: Could not find required components");
            return;
        }
        
        // Wait a moment then start the showcase
        GetTree().CreateTimer(2.0).Timeout += StartShowcase;
    }
    
    private void StartShowcase()
    {
        GD.Print("🎮 Starting Lenia Fun Features Showcase!");
        
        // Step 1: Load an interesting pattern
        ShowcasePatternLoading();
        
        // Step 2: Demonstrate audio feedback
        GetTree().CreateTimer(3.0).Timeout += ShowcaseAudioFeatures;
        
        // Step 3: Show particle effects
        GetTree().CreateTimer(6.0).Timeout += ShowcaseParticleEffects;
        
        // Step 4: Demonstrate recording
        GetTree().CreateTimer(9.0).Timeout += ShowcaseRecording;
        
        // Step 5: Show tutorials
        GetTree().CreateTimer(12.0).Timeout += ShowcaseTutorials;
        
        // Step 6: Display challenges
        GetTree().CreateTimer(15.0).Timeout += ShowcaseChallenges;
    }
    
    private void ShowcasePatternLoading()
    {
        GD.Print("📚 Showcasing Pattern Library...");
        
        // Load the Orbium pattern
        simulation.OrbiumPattern();
        
        // Show pattern library briefly
        mainUI.ShowPatternLibrary();
        GetTree().CreateTimer(2.0).Timeout += () => {
            // Hide pattern library
            var patternLibrary = FindChildOfType<PatternLibrary>(mainUI);
            if (patternLibrary != null)
                patternLibrary.Visible = false;
        };
    }
    
    private void ShowcaseAudioFeatures()
    {
        GD.Print("🔊 Showcasing Audio Features...");
        
        // Ensure audio is enabled
        mainUI.ToggleAudio(true);
        
        // Adjust volume to demonstrate
        mainUI.SetAudioVolume(0.8f);
        
        // The audio system will automatically respond to the simulation
        GD.Print("🎵 Audio feedback is now active - listen for population and growth sounds!");
    }
    
    private void ShowcaseParticleEffects()
    {
        GD.Print("✨ Showcasing Particle Effects...");
        
        // Enable particles with high intensity
        mainUI.ToggleParticles(true);
        mainUI.SetParticleIntensity(1.5f);
        
        // Simulate some paint actions to trigger particle effects
        SimulatePaintActions();
    }
    
    private void SimulatePaintActions()
    {
        // Simulate painting at random locations to show particle effects
        var random = new Random();
        
        for (int i = 0; i < 5; i++)
        {
            GetTree().CreateTimer(i * 0.5f).Timeout += () => {
                var paintPos = new Vector2(
                    random.Next(100, 400),
                    random.Next(100, 300)
                );
                
                // Trigger paint particles
                mainUI.OnUserPaintAction(paintPos, 0.8f, false);
                
                GD.Print($"💫 Paint effect at {paintPos}");
            };
        }
    }
    
    private void ShowcaseRecording()
    {
        GD.Print("🎬 Showcasing Time-lapse Recording...");
        
        // Start recording
        mainUI.StartTimelapseRecording();
        
        GD.Print("📹 Recording started! This will capture the simulation evolution...");
        
        // Stop recording after 3 seconds (for demo purposes)
        GetTree().CreateTimer(3.0).Timeout += () => {
            mainUI.StopTimelapseRecording();
            GD.Print("⏹ Recording stopped and saved!");
        };
    }
    
    private void ShowcaseTutorials()
    {
        GD.Print("🎓 Showcasing Tutorial System...");
        
        // Show tutorial selector briefly
        mainUI.ShowTutorials();
        
        GetTree().CreateTimer(3.0).Timeout += () => {
            // Hide tutorial system
            var tutorialSystem = FindChildOfType<TutorialSystem>(mainUI);
            if (tutorialSystem != null)
            {
                var selector = tutorialSystem.FindChild("*", true, false);
                if (selector is Control control)
                    control.Visible = false;
            }
            
            GD.Print("📖 Tutorial system demonstrated - users can learn interactively!");
        };
    }
    
    private void ShowcaseChallenges()
    {
        GD.Print("🏆 Showcasing Challenge System...");
        
        // Show challenge center briefly
        mainUI.ShowChallenges();
        
        GetTree().CreateTimer(3.0).Timeout += () => {
            // Hide challenge system
            var challengeSystem = FindChildOfType<ChallengeSystem>(mainUI);
            if (challengeSystem != null)
            {
                var center = challengeSystem.FindChild("*", true, false);
                if (center is Control control)
                    control.Visible = false;
            }
            
            GD.Print("🎯 Challenge system demonstrated - users can compete and achieve goals!");
            CompleteShowcase();
        };
    }
    
    private void CompleteShowcase()
    {
        GD.Print("🎉 Fun Features Showcase Complete!");
        GD.Print("🌟 All features are now active and working together:");
        GD.Print("  📚 Pattern Library - Rich collection of preset patterns");
        GD.Print("  🔊 Audio Feedback - Real-time generative audio");
        GD.Print("  ✨ Particle Effects - Dynamic visual feedback");
        GD.Print("  🎬 Time-lapse Recording - Capture evolution videos");
        GD.Print("  🎓 Interactive Tutorials - Guided learning experiences");
        GD.Print("  🏆 Challenge System - Competitive gameplay elements");
        GD.Print("");
        GD.Print("🚀 Lenia is now a fully interactive, engaging experience!");
        
        // Clean up
        QueueFree();
    }
    
    /// <summary>
    /// Helper method to find child of specific type
    /// </summary>
    private T FindChildOfType<T>(Node parent) where T : class
    {
        foreach (Node child in parent.GetChildren())
        {
            if (child is T found)
                return found;
                
            var nested = FindChildOfType<T>(child);
            if (nested != null)
                return nested;
        }
        return null;
    }
    
    public override void _Input(InputEvent @event)
    {
        // Allow user to skip showcase with Escape
        if (@event.IsActionPressed("ui_cancel"))
        {
            GD.Print("⏭ Showcase skipped by user");
            CompleteShowcase();
        }
    }
}