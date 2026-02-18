using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TutorialSystem : Control
{
    [Signal]
    public delegate void TutorialCompletedEventHandler(string tutorialId);
    
    [Signal]
    public delegate void TutorialStepCompletedEventHandler(string tutorialId, int stepIndex);
    
    public enum TutorialType
    {
        FirstTime,
        Feature,
        Advanced,
        Challenge
    }
    
    public class TutorialStep
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TargetElement { get; set; } // Node path or identifier
        public Vector2 HighlightPosition { get; set; }
        public Vector2 HighlightSize { get; set; }
        public System.Action<LeniaSimulation> SetupAction { get; set; }
        public Func<LeniaSimulation, bool> CompletionCheck { get; set; }
        public float AutoAdvanceDelay { get; set; } = 0.0f; // 0 means manual advance
        public bool ShowSkip { get; set; } = true;
    }
    
    public class Tutorial
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TutorialType Type { get; set; }
        public List<TutorialStep> Steps { get; set; } = new List<TutorialStep>();
        public bool IsCompleted { get; set; }
        public float EstimatedMinutes { get; set; }
        public int Difficulty { get; set; } = 1; // 1-5
    }
    
    private Dictionary<string, Tutorial> tutorials;
    private Tutorial currentTutorial;
    private int currentStepIndex;
    private LeniaSimulation simulation;
    
    // UI Elements
    private Panel tutorialPanel;
    private Label titleLabel;
    private RichTextLabel descriptionLabel;
    private Button nextButton;
    private Button skipButton;
    private Button exitButton;
    private ProgressBar progressBar;
    private Control highlightOverlay;
    private AnimationPlayer animationPlayer;
    
    // Tutorial selection UI
    private Control tutorialSelector;
    private VBoxContainer tutorialList;
    private bool isShowingSelector = false;
    
    public override void _Ready()
    {
        tutorials = new Dictionary<string, Tutorial>();
        SetupUI();
        CreateBuiltInTutorials();
        LoadTutorialProgress();
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
    }
    
    private void SetupUI()
    {
        // Main tutorial panel
        tutorialPanel = new Panel();
        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.1f, 0.12f, 0.18f, 0.95f);
        panelStyle.SetBorderWidthAll(2);
        panelStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.8f);
        panelStyle.SetCornerRadiusAll(10);
        tutorialPanel.AddThemeStyleboxOverride("panel", panelStyle);
        tutorialPanel.Size = new Vector2(400, 300);
        tutorialPanel.Position = new Vector2(50, 50);
        tutorialPanel.Visible = false;
        AddChild(tutorialPanel);
        
        // Tutorial content
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 15);
        tutorialPanel.AddChild(vbox);
        
        // Title
        titleLabel = new Label();
        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        titleLabel.AddThemeFontSizeOverride("font_size", 16);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(titleLabel);
        
        // Progress bar
        progressBar = new ProgressBar();
        progressBar.CustomMinimumSize = new Vector2(0, 8);
        progressBar.ShowPercentage = false;
        vbox.AddChild(progressBar);
        
        // Description
        descriptionLabel = new RichTextLabel();
        descriptionLabel.BbcodeEnabled = true;
        descriptionLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        descriptionLabel.AddThemeFontSizeOverride("normal_font_size", 12);
        vbox.AddChild(descriptionLabel);
        
        // Button container
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        buttonContainer.AddThemeConstantOverride("separation", 10);
        vbox.AddChild(buttonContainer);
        
        // Skip button
        skipButton = new Button();
        skipButton.Text = "Skip Tutorial";
        skipButton.AddThemeFontSizeOverride("font_size", 10);
        skipButton.Pressed += OnSkipPressed;
        buttonContainer.AddChild(skipButton);
        
        // Next button
        nextButton = new Button();
        nextButton.Text = "Next";
        nextButton.AddThemeFontSizeOverride("font_size", 12);
        nextButton.Pressed += OnNextPressed;
        buttonContainer.AddChild(nextButton);
        
        // Exit button
        exitButton = new Button();
        exitButton.Text = "Exit";
        exitButton.AddThemeFontSizeOverride("font_size", 10);
        exitButton.Pressed += OnExitPressed;
        buttonContainer.AddChild(exitButton);
        
        // Highlight overlay
        highlightOverlay = new Control();
        highlightOverlay.Visible = false;
        highlightOverlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(highlightOverlay);
        
        // Animation player
        animationPlayer = new AnimationPlayer();
        AddChild(animationPlayer);
        
        // Tutorial selector
        CreateTutorialSelector();
    }
    
    private void CreateTutorialSelector()
    {
        tutorialSelector = new Panel();
        var selectorStyle = new StyleBoxFlat();
        selectorStyle.BgColor = new Color(0.08f, 0.1f, 0.15f, 0.98f);
        selectorStyle.SetBorderWidthAll(2);
        selectorStyle.BorderColor = new Color(0.2f, 0.4f, 0.7f, 0.9f);
        selectorStyle.SetCornerRadiusAll(12);
        tutorialSelector.AddThemeStyleboxOverride("panel", selectorStyle);
        tutorialSelector.Size = new Vector2(600, 500);
        tutorialSelector.Position = new Vector2(100, 100);
        tutorialSelector.Visible = false;
        AddChild(tutorialSelector);
        
        var selectorVBox = new VBoxContainer();
        selectorVBox.AddThemeConstantOverride("separation", 20);
        tutorialSelector.AddChild(selectorVBox);
        
        // Header
        var header = new Label();
        header.Text = "🎓 Lenia Academy - Choose Your Learning Path";
        header.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        header.AddThemeFontSizeOverride("font_size", 18);
        header.HorizontalAlignment = HorizontalAlignment.Center;
        selectorVBox.AddChild(header);
        
        // Tutorial categories
        var scrollContainer = new ScrollContainer();
        scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        selectorVBox.AddChild(scrollContainer);
        
        tutorialList = new VBoxContainer();
        tutorialList.AddThemeConstantOverride("separation", 10);
        scrollContainer.AddChild(tutorialList);
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close";
        closeButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        closeButton.Pressed += () => tutorialSelector.Visible = false;
        selectorVBox.AddChild(closeButton);
    }
    
    private void CreateBuiltInTutorials()
    {
        // First Time Tutorial
        var firstTime = new Tutorial
        {
            Id = "first_time",
            Name = "Welcome to Lenia",
            Description = "Learn the basics of this fascinating artificial life simulation",
            Type = TutorialType.FirstTime,
            EstimatedMinutes = 5.0f,
            Difficulty = 1
        };
        
        firstTime.Steps.AddRange(new[]
        {
            new TutorialStep
            {
                Title = "Welcome to the World of Lenia!",
                Description = "[center][color=#88aaff][b]Welcome to Lenia![/b][/color][/center]\n\n" +
                            "Lenia is a continuous cellular automaton that creates beautiful, " +
                            "life-like patterns. Unlike Conway's Game of Life with its binary states, " +
                            "Lenia uses continuous values to create smooth, organic behaviors.\n\n" +
                            "[color=#aaffaa]Click 'Next' to begin your journey![/color]",
                AutoAdvanceDelay = 0.0f
            },
            new TutorialStep
            {
                Title = "The Play Controls",
                Description = "[b]Basic Controls[/b]\n\n" +
                            "• [color=#88ff88]▶ Play/Pause[/color]: Start or stop the simulation\n" +
                            "• [color=#88ff88]Step[/color]: Advance one frame when paused\n" +
                            "• [color=#88ff88]Reset[/color]: Restart with the current pattern\n" +
                            "• [color=#88ff88]Speed Slider[/color]: Control simulation speed\n\n" +
                            "Try clicking the play/pause button to see the simulation in action!",
                TargetElement = "HeaderBar/PlayPauseButton",
                CompletionCheck = (sim) => !sim.IsPaused
            },
            new TutorialStep
            {
                Title = "Painting Life",
                Description = "[b]Interactive Painting[/b]\n\n" +
                            "You can paint life directly onto the canvas!\n\n" +
                            "• [color=#88ff88]Left Click[/color]: Paint life\n" +
                            "• [color=#88ff88]Right Click[/color]: Erase\n" +
                            "• Adjust brush size and intensity in the right panel\n\n" +
                            "[color=#ffaa88]Try painting somewhere on the simulation![/color]",
                CompletionCheck = (sim) => HasUserInteracted(sim)
            },
            new TutorialStep
            {
                Title = "Explore Patterns",
                Description = "[b]Pattern Library[/b]\n\n" +
                            "The right sidebar contains preset patterns to explore:\n\n" +
                            "• [color=#88ff88]Orbium[/color]: The classic self-propelling creature\n" +
                            "• [color=#88ff88]Random[/color]: Chaotic starting points\n" +
                            "• [color=#88ff88]Ring[/color]: Creates wave-like patterns\n\n" +
                            "[color=#ffaa88]Try loading the Orbium pattern![/color]",
                CompletionCheck = (sim) => IsOrbiumLoaded(sim)
            },
            new TutorialStep
            {
                Title = "Parameter Experimentation",
                Description = "[b]Customize the Physics[/b]\n\n" +
                            "The right panel lets you modify Lenia's 'physics':\n\n" +
                            "• [color=#88ff88]Growth Mean[/color]: Where life thrives\n" +
                            "• [color=#88ff88]Growth Sigma[/color]: How narrow the life zone is\n" +
                            "• [color=#88ff88]Kernel Radius[/color]: How far cells influence each other\n\n" +
                            "[color=#aaffaa]Small changes can create dramatically different behaviors![/color]"
            },
            new TutorialStep
            {
                Title = "You're Ready to Explore!",
                Description = "[center][color=#88aaff][b]Congratulations![/b][/color][/center]\n\n" +
                            "You now know the basics of Lenia. Here are some things to try:\n\n" +
                            "• Experiment with different patterns\n" +
                            "• Adjust parameters to see how they affect behavior\n" +
                            "• Paint your own starting conditions\n" +
                            "• Take screenshots of interesting discoveries\n\n" +
                            "[color=#ffaa88]The universe of artificial life awaits your exploration![/color]"
            }
        });
        
        tutorials[firstTime.Id] = firstTime;
        
        // Advanced Features Tutorial
        var advanced = new Tutorial
        {
            Id = "advanced_features",
            Name = "Advanced Features",
            Description = "Discover powerful tools for serious Lenia exploration",
            Type = TutorialType.Advanced,
            EstimatedMinutes = 8.0f,
            Difficulty = 3
        };
        
        advanced.Steps.AddRange(new[]
        {
            new TutorialStep
            {
                Title = "Gallery and Screenshots",
                Description = "[b]Capturing Your Discoveries[/b]\n\n" +
                            "The gallery system helps you save and organize your findings:\n\n" +
                            "• [color=#88ff88]Screenshot[/color]: Capture the current state\n" +
                            "• [color=#88ff88]Gallery[/color]: Browse your saved images\n" +
                            "• Metadata is automatically saved with each screenshot\n\n" +
                            "[color=#ffaa88]Try taking a screenshot now![/color]",
                CompletionCheck = (sim) => HasTakenScreenshot()
            },
            new TutorialStep
            {
                Title = "Color Schemes",
                Description = "[b]Visualization Options[/b]\n\n" +
                            "Different color schemes can reveal different aspects:\n\n" +
                            "• [color=#ff8888]Heat[/color]: Classic temperature-like visualization\n" +
                            "• [color=#8888ff]Plasma[/color]: Beautiful electric-like colors\n" +
                            "• [color=#88ff88]Rainbow[/color]: Full spectrum visualization\n\n" +
                            "[color=#ffaa88]Try switching color schemes![/color]"
            },
            new TutorialStep
            {
                Title = "Performance Modes",
                Description = "[b]Balancing Quality and Speed[/b]\n\n" +
                            "Adjust grid resolution based on your needs:\n\n" +
                            "• [color=#88ff88]Fast (128×128)[/color]: Smooth performance\n" +
                            "• [color=#88ff88]Balanced (192×192)[/color]: Good compromise\n" +
                            "• [color=#88ff88]Quality (256×256)[/color]: Maximum detail\n\n" +
                            "Higher resolution shows more detail but runs slower."
            }
        });
        
        tutorials[advanced.Id] = advanced;
        
        // Pattern Creation Challenge
        var challenge = new Tutorial
        {
            Id = "pattern_challenge",
            Name = "Create a Stable Oscillator",
            Description = "Challenge: Create a pattern that oscillates without moving",
            Type = TutorialType.Challenge,
            EstimatedMinutes = 10.0f,
            Difficulty = 4
        };
        
        challenge.Steps.AddRange(new[]
        {
            new TutorialStep
            {
                Title = "Challenge: Stable Oscillator",
                Description = "[center][color=#ffaa88][b]Challenge Mode![/b][/color][/center]\n\n" +
                            "Your mission: Create a pattern that oscillates in place without drifting.\n\n" +
                            "[b]Requirements:[/b]\n" +
                            "• Pattern must oscillate (change over time)\n" +
                            "• Pattern must remain in roughly the same location\n" +
                            "• Pattern must be stable for at least 50 steps\n\n" +
                            "[color=#aaffaa]Hint: Try symmetrical starting patterns![/color]",
                SetupAction = (sim) => {
                    sim.ResizeGrid(128, 128);
                    ClearGrid(sim);
                }
            },
            new TutorialStep
            {
                Title = "Build Your Pattern",
                Description = "[b]Design Phase[/b]\n\n" +
                            "Use the painting tools to create your oscillator:\n\n" +
                            "• Start with a symmetrical pattern\n" +
                            "• Try circles, rings, or cross shapes\n" +
                            "• Experiment with different intensities\n\n" +
                            "[color=#ffaa88]Paint your pattern, then test it![/color]",
                CompletionCheck = (sim) => HasUserPainted(sim)
            },
            new TutorialStep
            {
                Title = "Test and Refine",
                Description = "[b]Testing Phase[/b]\n\n" +
                            "Run your pattern and observe:\n\n" +
                            "• Does it oscillate?\n" +
                            "• Does it stay in place?\n" +
                            "• Is it stable?\n\n" +
                            "If not, try adjusting:\n" +
                            "• Pattern shape\n" +
                            "• Growth parameters\n" +
                            "• Kernel radius\n\n" +
                            "[color=#aaffaa]Keep iterating until you succeed![/color]",
                CompletionCheck = (sim) => IsStableOscillator(sim)
            }
        });
        
        tutorials[challenge.Id] = challenge;
    }
    
    public void ShowTutorialSelector()
    {
        GD.Print("[TutorialSystem] ShowTutorialSelector called");
        GD.Print($"[TutorialSystem] Tutorial count: {tutorials.Count}");
        GD.Print($"[TutorialSystem] TutorialSelector visible before: {tutorialSelector.Visible}");
        
        RefreshTutorialList();
        tutorialSelector.Visible = true;
        isShowingSelector = true;
        
        GD.Print($"[TutorialSystem] TutorialSelector visible after: {tutorialSelector.Visible}");
        GD.Print($"[TutorialSystem] isShowingSelector set to: {isShowingSelector}");
        GD.Print("[TutorialSystem] ShowTutorialSelector completed");
    }
    
    private void RefreshTutorialList()
    {
        // Clear existing items
        foreach (Node child in tutorialList.GetChildren())
        {
            child.QueueFree();
        }
        
        // Group tutorials by type
        var groupedTutorials = tutorials.Values.GroupBy(t => t.Type).OrderBy(g => g.Key);
        
        foreach (var group in groupedTutorials)
        {
            // Category header
            var categoryHeader = new Label();
            categoryHeader.Text = GetCategoryDisplayName(group.Key);
            categoryHeader.AddThemeColorOverride("font_color", new Color(0.8f, 0.9f, 1.0f));
            categoryHeader.AddThemeFontSizeOverride("font_size", 14);
            tutorialList.AddChild(categoryHeader);
            
            // Tutorials in category
            foreach (var tutorial in group.OrderBy(t => t.Difficulty))
            {
                CreateTutorialCard(tutorial);
            }
            
            // Spacer
            var spacer = new Control();
            spacer.CustomMinimumSize = new Vector2(0, 10);
            tutorialList.AddChild(spacer);
        }
    }
    
    private void CreateTutorialCard(Tutorial tutorial)
    {
        var card = new Panel();
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = tutorial.IsCompleted ? 
            new Color(0.1f, 0.2f, 0.1f, 0.8f) : 
            new Color(0.15f, 0.18f, 0.25f, 0.8f);
        cardStyle.SetBorderWidthAll(1);
        cardStyle.BorderColor = tutorial.IsCompleted ? 
            new Color(0.4f, 0.8f, 0.4f, 0.6f) : 
            new Color(0.3f, 0.4f, 0.6f, 0.6f);
        cardStyle.SetCornerRadiusAll(6);
        card.AddThemeStyleboxOverride("panel", cardStyle);
        card.CustomMinimumSize = new Vector2(0, 80);
        tutorialList.AddChild(card);
        
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 15);
        card.AddChild(hbox);
        
        // Info section
        var infoVBox = new VBoxContainer();
        infoVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(infoVBox);
        
        // Title with completion status
        var titleRow = new HBoxContainer();
        infoVBox.AddChild(titleRow);
        
        var titleLabel = new Label();
        titleLabel.Text = tutorial.Name;
        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        titleLabel.AddThemeFontSizeOverride("font_size", 13);
        titleRow.AddChild(titleLabel);
        
        if (tutorial.IsCompleted)
        {
            var completedLabel = new Label();
            completedLabel.Text = " ✓";
            completedLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.8f, 0.4f));
            titleRow.AddChild(completedLabel);
        }
        
        // Description
        var descLabel = new Label();
        descLabel.Text = tutorial.Description;
        descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 0.9f));
        descLabel.AddThemeFontSizeOverride("font_size", 10);
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        infoVBox.AddChild(descLabel);
        
        // Metadata
        var metaRow = new HBoxContainer();
        infoVBox.AddChild(metaRow);
        
        // Difficulty stars
        var difficultyLabel = new Label();
        difficultyLabel.Text = "Difficulty: ";
        difficultyLabel.AddThemeFontSizeOverride("font_size", 9);
        metaRow.AddChild(difficultyLabel);
        
        for (int i = 1; i <= 5; i++)
        {
            var star = new Label();
            star.Text = i <= tutorial.Difficulty ? "★" : "☆";
            star.AddThemeColorOverride("font_color", i <= tutorial.Difficulty ? 
                new Color(1.0f, 0.8f, 0.2f) : new Color(0.4f, 0.4f, 0.4f));
            star.AddThemeFontSizeOverride("font_size", 9);
            metaRow.AddChild(star);
        }
        
        // Time estimate
        var timeLabel = new Label();
        timeLabel.Text = $" • ~{tutorial.EstimatedMinutes:F0} min";
        timeLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.7f, 0.8f));
        timeLabel.AddThemeFontSizeOverride("font_size", 9);
        metaRow.AddChild(timeLabel);
        
        // Start button
        var startButton = new Button();
        startButton.Text = tutorial.IsCompleted ? "Replay" : "Start";
        startButton.CustomMinimumSize = new Vector2(80, 35);
        startButton.AddThemeFontSizeOverride("font_size", 11);
        startButton.Pressed += () => StartTutorial(tutorial.Id);
        hbox.AddChild(startButton);
    }
    
    public void StartTutorial(string tutorialId)
    {
        GD.Print($"[TutorialSystem] StartTutorial called with ID: {tutorialId}");
        GD.Print($"[TutorialSystem] Call stack trace: {System.Environment.StackTrace}");
        GD.Print($"[TutorialSystem] Current time: {Time.GetUnixTimeFromSystem()}");
        GD.Print($"[TutorialSystem] Current tutorial panel visible: {tutorialPanel?.Visible}");
        GD.Print($"[TutorialSystem] Current tutorial selector visible: {tutorialSelector?.Visible}");
        GD.Print($"[TutorialSystem] Current tutorial in progress: {currentTutorial?.Name ?? "None"}");
        
        if (!tutorials.ContainsKey(tutorialId))
        {
            GD.PrintErr($"[TutorialSystem] ERROR: Tutorial not found: {tutorialId}");
            GD.Print($"[TutorialSystem] Available tutorials: {string.Join(", ", tutorials.Keys)}");
            return;
        }
        
        GD.Print($"[TutorialSystem] Tutorial found: {tutorials[tutorialId].Name}");
        
        currentTutorial = tutorials[tutorialId];
        currentStepIndex = 0;
        
        GD.Print($"[TutorialSystem] Setting tutorial selector visible to false (was: {tutorialSelector.Visible})");
        tutorialSelector.Visible = false;
        
        GD.Print($"[TutorialSystem] Setting tutorial panel visible to true (was: {tutorialPanel.Visible})");
        tutorialPanel.Visible = true;
        
        // Setup first step
        GD.Print($"[TutorialSystem] Calling ShowCurrentStep()");
        ShowCurrentStep();
        
        GD.Print($"[TutorialSystem] StartTutorial completed successfully: {currentTutorial.Name}");
        GD.Print("=====================================");
    }
    
    private void ShowCurrentStep()
    {
        GD.Print($"[TutorialSystem] ShowCurrentStep called");
        GD.Print($"[TutorialSystem] Current tutorial: {currentTutorial?.Name ?? "null"}");
        GD.Print($"[TutorialSystem] Current step index: {currentStepIndex}");
        
        if (currentTutorial == null || currentStepIndex >= currentTutorial.Steps.Count)
        {
            GD.PrintErr($"[TutorialSystem] ShowCurrentStep aborted - invalid state");
            GD.PrintErr($"[TutorialSystem] - currentTutorial is null: {currentTutorial == null}");
            GD.PrintErr($"[TutorialSystem] - stepIndex >= step count: {currentStepIndex >= (currentTutorial?.Steps.Count ?? 0)}");
            return;
        }
        
        var step = currentTutorial.Steps[currentStepIndex];
        GD.Print($"[TutorialSystem] Showing step: {step.Title}");
        
        // Update UI
        titleLabel.Text = step.Title;
        descriptionLabel.Text = step.Description;
        progressBar.Value = (currentStepIndex + 1) / (float)currentTutorial.Steps.Count * 100;
        
        GD.Print($"[TutorialSystem] UI updated - progress: {progressBar.Value}%");
        
        // Update button text
        nextButton.Text = currentStepIndex < currentTutorial.Steps.Count - 1 ? "Next" : "Complete";
        skipButton.Visible = step.ShowSkip;
        
        // Execute setup action
        if (step.SetupAction != null)
        {
            GD.Print($"[TutorialSystem] Executing step setup action");
            step.SetupAction?.Invoke(simulation);
        }
        
        // Show highlight if target specified
        if (!string.IsNullOrEmpty(step.TargetElement))
        {
            GD.Print($"[TutorialSystem] Showing highlight for: {step.TargetElement}");
            ShowHighlight(step.TargetElement, step.HighlightPosition, step.HighlightSize);
        }
        else
        {
            highlightOverlay.Visible = false;
        }
        
        // Auto-advance if specified
        if (step.AutoAdvanceDelay > 0)
        {
            GD.Print($"[TutorialSystem] Setting auto-advance timer: {step.AutoAdvanceDelay}s");
            GetTree().CreateTimer(step.AutoAdvanceDelay).Timeout += OnNextPressed;
        }
        
        GD.Print($"[TutorialSystem] ShowCurrentStep completed");
    }
    
    private void ShowHighlight(string targetElement, Vector2 position, Vector2 size)
    {
        // Implementation would highlight UI elements
        highlightOverlay.Visible = true;
        highlightOverlay.QueueRedraw();
    }
    
    private void OnNextPressed()
    {
        if (currentTutorial == null) return;
        
        var currentStep = currentTutorial.Steps[currentStepIndex];
        
        // Check completion condition if specified
        if (currentStep.CompletionCheck != null && !currentStep.CompletionCheck(simulation))
        {
            // Show hint or wait for completion
            ShowCompletionHint();
            return;
        }
        
        EmitSignal(SignalName.TutorialStepCompleted, currentTutorial.Id, currentStepIndex);
        
        currentStepIndex++;
        
        if (currentStepIndex >= currentTutorial.Steps.Count)
        {
            CompleteTutorial();
        }
        else
        {
            ShowCurrentStep();
        }
    }
    
    private void OnSkipPressed()
    {
        if (currentTutorial == null) return;
        
        // Skip to end
        currentStepIndex = currentTutorial.Steps.Count;
        CompleteTutorial();
    }
    
    private void OnExitPressed()
    {
        ExitTutorial();
    }
    
    private void CompleteTutorial()
    {
        if (currentTutorial == null) return;
        
        currentTutorial.IsCompleted = true;
        SaveTutorialProgress();
        
        EmitSignal(SignalName.TutorialCompleted, currentTutorial.Id);
        
        // Show completion message
        ShowCompletionMessage();
        
        ExitTutorial();
    }
    
    private void ExitTutorial()
    {
        tutorialPanel.Visible = false;
        highlightOverlay.Visible = false;
        currentTutorial = null;
        currentStepIndex = 0;
    }
    
    private void ShowCompletionHint()
    {
        // Flash the next button or show a hint
        var tween = CreateTween();
        tween.TweenProperty(nextButton, "modulate", Colors.Yellow, 0.2f);
        tween.TweenProperty(nextButton, "modulate", Colors.White, 0.2f);
        tween.TweenProperty(nextButton, "modulate", Colors.Yellow, 0.2f);
        tween.TweenProperty(nextButton, "modulate", Colors.White, 0.2f);
    }
    
    private void ShowCompletionMessage()
    {
        var popup = new AcceptDialog();
        popup.DialogText = $"Congratulations! You've completed '{currentTutorial.Name}'!\n\n" +
                          "You've earned new insights into the fascinating world of Lenia.";
        popup.Title = "Tutorial Complete! 🎉";
        AddChild(popup);
        popup.PopupCentered();
        popup.Confirmed += () => popup.QueueFree();
    }
    
    private void SaveTutorialProgress()
    {
        var saveData = new Godot.Collections.Dictionary();
        foreach (var tutorial in tutorials.Values)
        {
            saveData[tutorial.Id] = tutorial.IsCompleted;
        }
        
        var saveFile = FileAccess.Open("user://tutorial_progress.save", FileAccess.ModeFlags.Write);
        if (saveFile != null)
        {
            saveFile.StoreString(Json.Stringify(saveData));
            saveFile.Close();
        }
    }
    
    private void LoadTutorialProgress()
    {
        var saveFile = FileAccess.Open("user://tutorial_progress.save", FileAccess.ModeFlags.Read);
        if (saveFile == null) return;
        
        var jsonString = saveFile.GetAsText();
        saveFile.Close();
        
        var json = new Json();
        var parseResult = json.Parse(jsonString);
        
        if (parseResult == Error.Ok)
        {
            var saveData = json.Data.AsGodotDictionary();
            foreach (var tutorial in tutorials.Values)
            {
                if (saveData.ContainsKey(tutorial.Id))
                {
                    tutorial.IsCompleted = saveData[tutorial.Id].AsBool();
                }
            }
        }
    }
    
    private string GetCategoryDisplayName(TutorialType type)
    {
        return type switch
        {
            TutorialType.FirstTime => "🌟 Getting Started",
            TutorialType.Feature => "🔧 Features & Tools",
            TutorialType.Advanced => "🎯 Advanced Techniques",
            TutorialType.Challenge => "🏆 Challenges",
            _ => type.ToString()
        };
    }
    
    // Helper methods for tutorial conditions
    private bool HasUserInteracted(LeniaSimulation sim)
    {
        // Would track if user has painted or interacted
        return true; // Simplified for demo
    }
    
    private bool IsOrbiumLoaded(LeniaSimulation sim)
    {
        // Would check if Orbium pattern is currently loaded
        return true; // Simplified for demo
    }
    
    private bool HasTakenScreenshot()
    {
        // Would check if user has taken a screenshot
        return true; // Simplified for demo
    }
    
    private void ClearGrid(LeniaSimulation sim)
    {
        var grid = sim.GetCurrentGrid();
        for (int x = 0; x < sim.GridWidth; x++)
        {
            for (int y = 0; y < sim.GridHeight; y++)
            {
                grid[x, y] = 0.0f;
            }
        }
    }
    
    private bool HasUserPainted(LeniaSimulation sim)
    {
        // Would check if user has painted something
        return true; // Simplified for demo
    }
    
    private bool IsStableOscillator(LeniaSimulation sim)
    {
        // Would analyze the pattern to see if it's a stable oscillator
        return true; // Simplified for demo
    }
    
    public bool ShouldShowFirstTimeTutorial()
    {
        var shouldShow = !tutorials["first_time"].IsCompleted;
        GD.Print($"[TutorialSystem] ShouldShowFirstTimeTutorial called - result: {shouldShow}");
        GD.Print($"[TutorialSystem] First time tutorial completed: {tutorials["first_time"].IsCompleted}");
        GD.Print($"[TutorialSystem] Call from: {System.Environment.StackTrace}");
        return shouldShow;
    }
    
    public void ShowQuickTip(string message, Vector2 position)
    {
        // Show a quick tooltip-style tip
        var tip = new Panel();
        tip.Size = new Vector2(200, 60);
        tip.Position = position;
        
        var tipStyle = new StyleBoxFlat();
        tipStyle.BgColor = new Color(0.1f, 0.15f, 0.2f, 0.9f);
        tipStyle.SetCornerRadiusAll(8);
        tip.AddThemeStyleboxOverride("panel", tipStyle);
        
        var label = new Label();
        label.Text = message;
        label.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        label.AddThemeFontSizeOverride("font_size", 11);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        tip.AddChild(label);
        
        AddChild(tip);
        
        // Auto-remove after 3 seconds
        var timer = GetTree().CreateTimer(3.0);
        timer.Timeout += () => tip.QueueFree();
    }
}