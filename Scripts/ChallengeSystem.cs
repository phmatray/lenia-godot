using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ChallengeSystem : Control
{
    [Signal]
    public delegate void ChallengeCompletedEventHandler(string challengeId, float score);
    
    [Signal]
    public delegate void NewRecordEventHandler(string challengeId, float newRecord);
    
    [Signal]
    public delegate void AchievementUnlockedEventHandler(string achievementId);
    
    public enum ChallengeType
    {
        Survival,      // Keep pattern alive for X time
        Efficiency,    // Achieve goal with minimal parameters
        Speed,         // Complete objective as fast as possible
        Creativity,    // User-voted creative challenges
        Discovery,     // Find specific pattern behaviors
        Optimization   // Maximize/minimize certain metrics
    }
    
    public enum ChallengeDifficulty
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4,
        Master = 5
    }
    
    public class Challenge
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Objective { get; set; }
        public ChallengeType Type { get; set; }
        public ChallengeDifficulty Difficulty { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public Func<LeniaSimulation, ChallengeMetrics, bool> CompletionCheck { get; set; }
        public Func<LeniaSimulation, ChallengeMetrics, float> ScoreCalculation { get; set; }
        public System.Action<LeniaSimulation> SetupAction { get; set; }
        public float TimeLimit { get; set; } = 0.0f; // 0 = no limit
        public float BestScore { get; set; } = 0.0f;
        public bool IsCompleted { get; set; }
        public List<string> UnlockedBy { get; set; } = new List<string>(); // Prerequisites
        public List<string> Unlocks { get; set; } = new List<string>(); // What this unlocks
        public int RewardPoints { get; set; } = 100;
    }
    
    public class ChallengeMetrics
    {
        public float Duration { get; set; }
        public float PopulationStability { get; set; }
        public float PopulationVariance { get; set; }
        public float MaxPopulation { get; set; }
        public float MinPopulation { get; set; }
        public float AveragePopulation { get; set; }
        public int PatternChanges { get; set; }
        public float MovementDistance { get; set; }
        public float ParameterEfficiency { get; set; }
        public List<float> PopulationHistory { get; set; } = new List<float>();
        public Dictionary<string, float> CustomMetrics { get; set; } = new Dictionary<string, float>();
    }
    
    public class Achievement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public bool IsUnlocked { get; set; }
        public Func<ChallengeSystem, bool> UnlockCondition { get; set; }
        public int Points { get; set; } = 50;
    }
    
    private Dictionary<string, Challenge> challenges;
    private Dictionary<string, Achievement> achievements;
    private Challenge currentChallenge;
    private ChallengeMetrics currentMetrics;
    private LeniaSimulation simulation;
    private float challengeStartTime;
    private bool isChallengeActive = false;
    
    // UI Elements
    private Panel challengePanel;
    private Label challengeTitle;
    private RichTextLabel challengeDescription;
    private Label objectiveLabel;
    private Label timeLabel;
    private Label scoreLabel;
    private ProgressBar progressBar;
    private Button startButton;
    private Button stopButton;
    private VBoxContainer challengeList;
    private TabContainer categoryTabs;
    
    // Leaderboard system
    private Dictionary<string, List<ChallengeResult>> leaderboards;
    
    public class ChallengeResult
    {
        public string PlayerName { get; set; } = "Player";
        public float Score { get; set; }
        public float Duration { get; set; }
        public string Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
    
    public override void _Ready()
    {
        challenges = new Dictionary<string, Challenge>();
        achievements = new Dictionary<string, Achievement>();
        leaderboards = new Dictionary<string, List<ChallengeResult>>();
        
        SetupUI();
        CreateBuiltInChallenges();
        CreateAchievements();
        LoadProgress();
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
    }
    
    private void SetupUI()
    {
        // Main challenge panel
        challengePanel = new Panel();
        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.08f, 0.1f, 0.15f, 0.95f);
        panelStyle.SetBorderWidthAll(2);
        panelStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.8f);
        panelStyle.SetCornerRadiusAll(10);
        challengePanel.AddThemeStyleboxOverride("panel", panelStyle);
        challengePanel.Size = new Vector2(800, 600);
        challengePanel.Position = new Vector2(50, 50);
        challengePanel.Visible = false;
        AddChild(challengePanel);
        
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 15);
        challengePanel.AddChild(vbox);
        
        // Header
        var header = new Label();
        header.Text = "🏆 Lenia Challenge Center";
        header.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        header.AddThemeFontSizeOverride("font_size", 20);
        header.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(header);
        
        // Challenge categories
        categoryTabs = new TabContainer();
        categoryTabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(categoryTabs);
        
        CreateCategoryTabs();
        
        // Current challenge info (when active)
        CreateChallengeActiveUI(vbox);
    }
    
    private void CreateCategoryTabs()
    {
        foreach (ChallengeType type in System.Enum.GetValues<ChallengeType>())
        {
            var scrollContainer = new ScrollContainer();
            scrollContainer.Name = GetCategoryDisplayName(type);
            categoryTabs.AddChild(scrollContainer);
            
            var gridContainer = new GridContainer();
            gridContainer.Columns = 2;
            gridContainer.AddThemeConstantOverride("h_separation", 15);
            gridContainer.AddThemeConstantOverride("v_separation", 15);
            scrollContainer.AddChild(gridContainer);
        }
    }
    
    private void CreateChallengeActiveUI(VBoxContainer parent)
    {
        var activePanel = new Panel();
        activePanel.CustomMinimumSize = new Vector2(0, 120);
        activePanel.Visible = false;
        var activeStyle = new StyleBoxFlat();
        activeStyle.BgColor = new Color(0.1f, 0.15f, 0.1f, 0.8f);
        activeStyle.SetCornerRadiusAll(8);
        activePanel.AddThemeStyleboxOverride("panel", activeStyle);
        parent.AddChild(activePanel);
        
        var activeVBox = new VBoxContainer();
        activePanel.AddChild(activeVBox);
        
        challengeTitle = new Label();
        challengeTitle.AddThemeColorOverride("font_color", new Color(0.9f, 1.0f, 0.9f));
        challengeTitle.AddThemeFontSizeOverride("font_size", 14);
        activeVBox.AddChild(challengeTitle);
        
        objectiveLabel = new Label();
        objectiveLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.9f, 0.8f));
        objectiveLabel.AddThemeFontSizeOverride("font_size", 11);
        activeVBox.AddChild(objectiveLabel);
        
        var statsHBox = new HBoxContainer();
        activeVBox.AddChild(statsHBox);
        
        timeLabel = new Label();
        timeLabel.Text = "Time: 0:00";
        statsHBox.AddChild(timeLabel);
        
        scoreLabel = new Label();
        scoreLabel.Text = "Score: 0";
        statsHBox.AddChild(scoreLabel);
        
        progressBar = new ProgressBar();
        progressBar.CustomMinimumSize = new Vector2(200, 20);
        statsHBox.AddChild(progressBar);
        
        var buttonHBox = new HBoxContainer();
        activeVBox.AddChild(buttonHBox);
        
        stopButton = new Button();
        stopButton.Text = "Stop Challenge";
        stopButton.Pressed += StopCurrentChallenge;
        buttonHBox.AddChild(stopButton);
    }
    
    private void CreateBuiltInChallenges()
    {
        // Survival Challenges
        challenges["survival_orbium"] = new Challenge
        {
            Id = "survival_orbium",
            Name = "Orbium Endurance",
            Description = "Keep an Orbium alive and moving for 2 minutes without it dying or getting stuck.",
            Objective = "Maintain a living, moving Orbium for 120 seconds",
            Type = ChallengeType.Survival,
            Difficulty = ChallengeDifficulty.Beginner,
            TimeLimit = 120.0f,
            RewardPoints = 100,
            SetupAction = (sim) => {
                sim.OrbiumPattern();
                sim.DeltaTime = 0.1f;
                sim.KernelRadius = 13.0f;
            },
            CompletionCheck = (sim, metrics) => metrics.Duration >= 120.0f && IsPatternAlive(sim),
            ScoreCalculation = (sim, metrics) => metrics.Duration * GetPatternHealthScore(sim)
        };
        
        challenges["population_stability"] = new Challenge
        {
            Id = "population_stability",
            Name = "Stable Ecosystem",
            Description = "Create a pattern that maintains stable population levels for 300 seconds.",
            Objective = "Keep population variance under 10% for 5 minutes",
            Type = ChallengeType.Survival,
            Difficulty = ChallengeDifficulty.Intermediate,
            TimeLimit = 300.0f,
            RewardPoints = 200,
            CompletionCheck = (sim, metrics) => 
                metrics.Duration >= 300.0f && metrics.PopulationVariance < 0.1f,
            ScoreCalculation = (sim, metrics) => 
                300.0f / (1.0f + metrics.PopulationVariance * 10.0f)
        };
        
        // Speed Challenges
        challenges["rapid_growth"] = new Challenge
        {
            Id = "rapid_growth",
            Name = "Explosive Growth",
            Description = "Achieve 50% grid population as quickly as possible starting from a single cell.",
            Objective = "Reach 50% population coverage in minimum time",
            Type = ChallengeType.Speed,
            Difficulty = ChallengeDifficulty.Advanced,
            TimeLimit = 60.0f,
            RewardPoints = 300,
            SetupAction = (sim) => {
                ClearGrid(sim);
                sim.SetGridValue(sim.GridWidth / 2, sim.GridHeight / 2, 1.0f);
            },
            CompletionCheck = (sim, metrics) => GetPopulationPercentage(sim) >= 0.5f,
            ScoreCalculation = (sim, metrics) => 100.0f / (metrics.Duration + 1.0f)
        };
        
        // Efficiency Challenges
        challenges["minimal_orbium"] = new Challenge
        {
            Id = "minimal_orbium",
            Name = "Minimalist Orbium",
            Description = "Create a self-propelling pattern using the most restrictive parameters possible.",
            Objective = "Create moving life with delta_time < 0.05 and kernel_radius < 8",
            Type = ChallengeType.Efficiency,
            Difficulty = ChallengeDifficulty.Expert,
            RewardPoints = 500,
            CompletionCheck = (sim, metrics) => 
                sim.DeltaTime < 0.05f && sim.KernelRadius < 8.0f && 
                metrics.MovementDistance > 10.0f && metrics.Duration > 30.0f,
            ScoreCalculation = (sim, metrics) => 
                metrics.MovementDistance / (sim.DeltaTime * sim.KernelRadius * 100.0f)
        };
        
        // Discovery Challenges
        challenges["oscillator_hunter"] = new Challenge
        {
            Id = "oscillator_hunter",
            Name = "Oscillator Hunter",
            Description = "Discover a pattern that oscillates with a period between 5-15 simulation steps.",
            Objective = "Find a stable oscillating pattern",
            Type = ChallengeType.Discovery,
            Difficulty = ChallengeDifficulty.Intermediate,
            TimeLimit = 180.0f,
            RewardPoints = 250,
            CompletionCheck = (sim, metrics) => DetectOscillation(metrics),
            ScoreCalculation = (sim, metrics) => CalculateOscillationScore(metrics)
        };
        
        // Creative Challenges
        challenges["symmetry_master"] = new Challenge
        {
            Id = "symmetry_master",
            Name = "Symmetry Master",
            Description = "Create the most beautiful symmetrical pattern that remains stable.",
            Objective = "Design an aesthetically pleasing symmetrical stable pattern",
            Type = ChallengeType.Creativity,
            Difficulty = ChallengeDifficulty.Advanced,
            TimeLimit = 300.0f,
            RewardPoints = 400,
            CompletionCheck = (sim, metrics) => 
                DetectSymmetry(sim) && metrics.PopulationStability > 0.8f,
            ScoreCalculation = (sim, metrics) => 
                GetSymmetryScore(sim) * metrics.PopulationStability * 100.0f
        };
        
        // Master Challenges
        challenges["chaos_tamer"] = new Challenge
        {
            Id = "chaos_tamer",
            Name = "Chaos Tamer",
            Description = "Start with complete chaos and guide it to create stable, organized patterns.",
            Objective = "Transform random noise into organized life",
            Type = ChallengeType.Optimization,
            Difficulty = ChallengeDifficulty.Master,
            TimeLimit = 600.0f,
            RewardPoints = 1000,
            UnlockedBy = new List<string> { "survival_orbium", "population_stability" },
            SetupAction = (sim) => {
                sim.RandomPattern();
                // More chaotic than usual
                var grid = sim.GetCurrentGrid();
                var random = new RandomNumberGenerator();
                random.Randomize();
                for (int x = 0; x < sim.GridWidth; x++)
                {
                    for (int y = 0; y < sim.GridHeight; y++)
                    {
                        grid[x, y] = random.Randf();
                    }
                }
            },
            CompletionCheck = (sim, metrics) => 
                GetOrganizationScore(sim) > 0.7f && metrics.Duration > 120.0f,
            ScoreCalculation = (sim, metrics) => 
                GetOrganizationScore(sim) * 1000.0f / (metrics.Duration / 60.0f)
        };
    }
    
    private void CreateAchievements()
    {
        achievements["first_challenge"] = new Achievement
        {
            Id = "first_challenge",
            Name = "First Steps",
            Description = "Complete your first challenge",
            Points = 50,
            UnlockCondition = (system) => system.GetCompletedChallengesCount() >= 1
        };
        
        achievements["speed_demon"] = new Achievement
        {
            Id = "speed_demon",
            Name = "Speed Demon",
            Description = "Complete 3 speed challenges",
            Points = 150,
            UnlockCondition = (system) => system.GetCompletedChallengesByType(ChallengeType.Speed) >= 3
        };
        
        achievements["survivor"] = new Achievement
        {
            Id = "survivor",
            Name = "Survivor",
            Description = "Complete 5 survival challenges",
            Points = 200,
            UnlockCondition = (system) => system.GetCompletedChallengesByType(ChallengeType.Survival) >= 5
        };
        
        achievements["perfectionist"] = new Achievement
        {
            Id = "perfectionist",
            Name = "Perfectionist",
            Description = "Achieve perfect scores on 3 different challenges",
            Points = 300,
            UnlockCondition = (system) => system.GetPerfectScoresCount() >= 3
        };
        
        achievements["master_explorer"] = new Achievement
        {
            Id = "master_explorer",
            Name = "Master Explorer",
            Description = "Complete challenges in all categories",
            Points = 500,
            UnlockCondition = (system) => system.HasCompletedAllCategories()
        };
    }
    
    public void ShowChallengeCenter()
    {
        RefreshChallengeList();
        challengePanel.Visible = true;
    }
    
    private void RefreshChallengeList()
    {
        foreach (ChallengeType type in System.Enum.GetValues<ChallengeType>())
        {
            RefreshCategoryDisplay(type);
        }
    }
    
    private void RefreshCategoryDisplay(ChallengeType category)
    {
        var tabIndex = (int)category;
        if (tabIndex >= categoryTabs.GetChildCount()) return;
        
        var scrollContainer = categoryTabs.GetChild(tabIndex) as ScrollContainer;
        var gridContainer = scrollContainer?.GetChild(0) as GridContainer;
        if (gridContainer == null) return;
        
        // Clear existing challenges
        foreach (Node child in gridContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add challenges for this category
        var categoryChallenges = challenges.Values
            .Where(c => c.Type == category && IsUnlocked(c))
            .OrderBy(c => c.Difficulty)
            .ToList();
        
        foreach (var challenge in categoryChallenges)
        {
            CreateChallengeCard(gridContainer, challenge);
        }
    }
    
    private void CreateChallengeCard(GridContainer parent, Challenge challenge)
    {
        var card = new Panel();
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = challenge.IsCompleted ? 
            new Color(0.1f, 0.2f, 0.1f, 0.9f) : 
            new Color(0.15f, 0.18f, 0.25f, 0.9f);
        cardStyle.SetBorderWidthAll(2);
        cardStyle.BorderColor = GetDifficultyColor(challenge.Difficulty);
        cardStyle.SetCornerRadiusAll(8);
        card.AddThemeStyleboxOverride("panel", cardStyle);
        card.CustomMinimumSize = new Vector2(350, 180);
        parent.AddChild(card);
        
        var cardVBox = new VBoxContainer();
        cardVBox.AddThemeConstantOverride("separation", 8);
        card.AddChild(cardVBox);
        
        // Title row
        var titleRow = new HBoxContainer();
        cardVBox.AddChild(titleRow);
        
        var titleLabel = new Label();
        titleLabel.Text = challenge.Name;
        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.95f, 1.0f));
        titleLabel.AddThemeFontSizeOverride("font_size", 14);
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleRow.AddChild(titleLabel);
        
        if (challenge.IsCompleted)
        {
            var completedIcon = new Label();
            completedIcon.Text = "✓";
            completedIcon.AddThemeColorOverride("font_color", new Color(0.4f, 0.8f, 0.4f));
            completedIcon.AddThemeFontSizeOverride("font_size", 16);
            titleRow.AddChild(completedIcon);
        }
        
        // Difficulty and rewards
        var metaRow = new HBoxContainer();
        cardVBox.AddChild(metaRow);
        
        var difficultyLabel = new Label();
        difficultyLabel.Text = GetDifficultyText(challenge.Difficulty);
        difficultyLabel.AddThemeColorOverride("font_color", GetDifficultyColor(challenge.Difficulty));
        difficultyLabel.AddThemeFontSizeOverride("font_size", 10);
        metaRow.AddChild(difficultyLabel);
        
        var pointsLabel = new Label();
        pointsLabel.Text = $" • {challenge.RewardPoints} pts";
        pointsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.6f));
        pointsLabel.AddThemeFontSizeOverride("font_size", 10);
        metaRow.AddChild(pointsLabel);
        
        if (challenge.TimeLimit > 0)
        {
            var timeLabel = new Label();
            timeLabel.Text = $" • {challenge.TimeLimit:F0}s limit";
            timeLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.6f, 0.6f));
            timeLabel.AddThemeFontSizeOverride("font_size", 10);
            metaRow.AddChild(timeLabel);
        }
        
        // Description
        var descLabel = new Label();
        descLabel.Text = challenge.Description;
        descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 0.9f));
        descLabel.AddThemeFontSizeOverride("font_size", 11);
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        descLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        cardVBox.AddChild(descLabel);
        
        // Objective
        var objectiveLabel = new Label();
        objectiveLabel.Text = $"Objective: {challenge.Objective}";
        objectiveLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.9f, 0.8f));
        objectiveLabel.AddThemeFontSizeOverride("font_size", 10);
        objectiveLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        cardVBox.AddChild(objectiveLabel);
        
        // Best score
        if (challenge.BestScore > 0)
        {
            var scoreLabel = new Label();
            scoreLabel.Text = $"Best Score: {challenge.BestScore:F1}";
            scoreLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1.0f));
            scoreLabel.AddThemeFontSizeOverride("font_size", 10);
            cardVBox.AddChild(scoreLabel);
        }
        
        // Start button
        var startButton = new Button();
        startButton.Text = isChallengeActive ? "Challenge Active" : "Start Challenge";
        startButton.Disabled = isChallengeActive;
        startButton.CustomMinimumSize = new Vector2(0, 30);
        startButton.Pressed += () => StartChallenge(challenge.Id);
        cardVBox.AddChild(startButton);
    }
    
    public void StartChallenge(string challengeId)
    {
        if (!challenges.ContainsKey(challengeId) || isChallengeActive) return;
        
        currentChallenge = challenges[challengeId];
        currentMetrics = new ChallengeMetrics();
        isChallengeActive = true;
        challengeStartTime = (float)Time.GetUnixTimeFromSystem();
        
        // Setup challenge
        currentChallenge.SetupAction?.Invoke(simulation);
        
        // Update UI
        challengeTitle.Text = $"Active: {currentChallenge.Name}";
        objectiveLabel.Text = currentChallenge.Objective;
        
        GD.Print($"Started challenge: {currentChallenge.Name}");
    }
    
    public override void _Process(double delta)
    {
        if (!isChallengeActive || currentChallenge == null) return;
        
        // Update metrics
        UpdateChallengeMetrics((float)delta);
        
        // Update UI
        UpdateChallengeUI();
        
        // Check completion
        if (currentChallenge.CompletionCheck?.Invoke(simulation, currentMetrics) == true)
        {
            CompleteChallenge();
        }
        
        // Check time limit
        if (currentChallenge.TimeLimit > 0 && currentMetrics.Duration >= currentChallenge.TimeLimit)
        {
            if (currentChallenge.CompletionCheck?.Invoke(simulation, currentMetrics) != true)
            {
                FailChallenge();
            }
            else
            {
                CompleteChallenge();
            }
        }
    }
    
    private void UpdateChallengeMetrics(float delta)
    {
        currentMetrics.Duration += delta;
        
        // Calculate population metrics
        float currentPopulation = GetPopulationPercentage(simulation);
        currentMetrics.PopulationHistory.Add(currentPopulation);
        
        if (currentMetrics.PopulationHistory.Count > 1)
        {
            currentMetrics.AveragePopulation = currentMetrics.PopulationHistory.Average();
            currentMetrics.MaxPopulation = currentMetrics.PopulationHistory.Max();
            currentMetrics.MinPopulation = currentMetrics.PopulationHistory.Min();
            
            // Calculate variance
            var variance = currentMetrics.PopulationHistory
                .Select(p => Math.Pow(p - currentMetrics.AveragePopulation, 2))
                .Average();
            currentMetrics.PopulationVariance = (float)Math.Sqrt(variance);
        }
        
        // Calculate stability (how much the population varies)
        if (currentMetrics.PopulationHistory.Count > 10)
        {
            var recent = currentMetrics.PopulationHistory.TakeLast(10).ToList();
            var recentVariance = recent.Select(p => Math.Pow(p - recent.Average(), 2)).Average();
            currentMetrics.PopulationStability = 1.0f - (float)Math.Min(1.0, Math.Sqrt(recentVariance) * 10);
        }
    }
    
    private void UpdateChallengeUI()
    {
        int minutes = (int)(currentMetrics.Duration / 60);
        int seconds = (int)(currentMetrics.Duration % 60);
        timeLabel.Text = $"Time: {minutes}:{seconds:D2}";
        
        float score = currentChallenge.ScoreCalculation?.Invoke(simulation, currentMetrics) ?? 0.0f;
        scoreLabel.Text = $"Score: {score:F1}";
        
        // Update progress bar based on challenge type
        float progress = 0.0f;
        if (currentChallenge.TimeLimit > 0)
        {
            progress = (currentMetrics.Duration / currentChallenge.TimeLimit) * 100.0f;
        }
        else
        {
            // Use completion heuristic
            progress = EstimateProgress() * 100.0f;
        }
        progressBar.Value = Math.Min(100.0f, progress);
    }
    
    private float EstimateProgress()
    {
        // Estimate progress based on challenge type and current metrics
        return currentChallenge.Type switch
        {
            ChallengeType.Survival => Math.Min(1.0f, currentMetrics.Duration / 120.0f),
            ChallengeType.Speed => Math.Min(1.0f, GetPopulationPercentage(simulation) * 2.0f),
            ChallengeType.Discovery => currentMetrics.PatternChanges / 50.0f,
            _ => Math.Min(1.0f, currentMetrics.Duration / 60.0f)
        };
    }
    
    private void CompleteChallenge()
    {
        if (!isChallengeActive) return;
        
        float score = currentChallenge.ScoreCalculation?.Invoke(simulation, currentMetrics) ?? 0.0f;
        
        // Update challenge record
        if (score > currentChallenge.BestScore)
        {
            currentChallenge.BestScore = score;
            EmitSignal(SignalName.NewRecord, currentChallenge.Id, score);
        }
        
        currentChallenge.IsCompleted = true;
        
        // Save result to leaderboard
        SaveChallengeResult(score);
        
        // Check for achievements
        CheckAchievements();
        
        ShowCompletionDialog(score, true);
        StopCurrentChallenge();
        
        EmitSignal(SignalName.ChallengeCompleted, currentChallenge.Id, score);
    }
    
    private void FailChallenge()
    {
        ShowCompletionDialog(0.0f, false);
        StopCurrentChallenge();
    }
    
    private void StopCurrentChallenge()
    {
        isChallengeActive = false;
        currentChallenge = null;
        currentMetrics = null;
        
        // Hide active UI
        // Implementation would hide the active challenge UI
    }
    
    private void ShowCompletionDialog(float score, bool success)
    {
        var dialog = new AcceptDialog();
        
        if (success)
        {
            dialog.Title = "Challenge Complete! 🏆";
            dialog.DialogText = $"Congratulations! You completed '{currentChallenge.Name}'\n\n" +
                              $"Score: {score:F1}\n" +
                              $"Time: {currentMetrics.Duration:F1} seconds\n" +
                              $"Reward: {currentChallenge.RewardPoints} points";
        }
        else
        {
            dialog.Title = "Challenge Failed 😞";
            dialog.DialogText = $"You didn't complete '{currentChallenge.Name}' in time.\n\n" +
                              $"Duration: {currentMetrics.Duration:F1} seconds\n" +
                              $"Don't give up - try again with a different strategy!";
        }
        
        AddChild(dialog);
        dialog.PopupCentered();
        dialog.Confirmed += () => dialog.QueueFree();
    }
    
    private void SaveChallengeResult(float score)
    {
        if (!leaderboards.ContainsKey(currentChallenge.Id))
        {
            leaderboards[currentChallenge.Id] = new List<ChallengeResult>();
        }
        
        var result = new ChallengeResult
        {
            Score = score,
            Duration = currentMetrics.Duration,
            Timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Metadata = new Dictionary<string, object>
            {
                ["population_stability"] = currentMetrics.PopulationStability,
                ["max_population"] = currentMetrics.MaxPopulation,
                ["average_population"] = currentMetrics.AveragePopulation
            }
        };
        
        leaderboards[currentChallenge.Id].Add(result);
        
        // Keep only top 10 results
        leaderboards[currentChallenge.Id] = leaderboards[currentChallenge.Id]
            .OrderByDescending(r => r.Score)
            .Take(10)
            .ToList();
    }
    
    private void CheckAchievements()
    {
        foreach (var achievement in achievements.Values.Where(a => !a.IsUnlocked))
        {
            if (achievement.UnlockCondition(this))
            {
                achievement.IsUnlocked = true;
                EmitSignal(SignalName.AchievementUnlocked, achievement.Id);
                ShowAchievementUnlocked(achievement);
            }
        }
    }
    
    private void ShowAchievementUnlocked(Achievement achievement)
    {
        var notification = new Panel();
        notification.Size = new Vector2(300, 80);
        notification.Position = new Vector2(GetViewport().GetVisibleRect().Size.X - 320, 20);
        
        var notificationStyle = new StyleBoxFlat();
        notificationStyle.BgColor = new Color(0.1f, 0.2f, 0.1f, 0.95f);
        notificationStyle.SetBorderWidthAll(2);
        notificationStyle.BorderColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
        notificationStyle.SetCornerRadiusAll(8);
        notification.AddThemeStyleboxOverride("panel", notificationStyle);
        
        var label = new RichTextLabel();
        label.BbcodeEnabled = true;
        label.Text = $"[center][color=#88ff88][b]Achievement Unlocked![/b][/color]\n{achievement.Name}\n+{achievement.Points} points[/center]";
        label.FitContent = true;
        notification.AddChild(label);
        
        AddChild(notification);
        
        // Animate and remove
        var tween = CreateTween();
        tween.TweenProperty(notification, "modulate:a", 0.0f, 3.0f).SetDelay(2.0f);
        tween.TweenCallback(Callable.From(() => notification.QueueFree()));
    }
    
    // Helper methods for challenge conditions
    private bool IsPatternAlive(LeniaSimulation sim)
    {
        return GetPopulationPercentage(sim) > 0.01f;
    }
    
    private float GetPatternHealthScore(LeniaSimulation sim)
    {
        float population = GetPopulationPercentage(sim);
        return Mathf.Clamp(population * 10.0f, 0.1f, 1.0f);
    }
    
    private float GetPopulationPercentage(LeniaSimulation sim)
    {
        var grid = sim.GetCurrentGrid();
        float total = 0.0f;
        for (int x = 0; x < sim.GridWidth; x++)
        {
            for (int y = 0; y < sim.GridHeight; y++)
            {
                total += grid[x, y];
            }
        }
        return total / (sim.GridWidth * sim.GridHeight);
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
    
    private bool DetectOscillation(ChallengeMetrics metrics)
    {
        // Simplified oscillation detection
        return metrics.PopulationHistory.Count > 50 && 
               metrics.PopulationVariance > 0.1f && 
               metrics.PopulationVariance < 0.3f;
    }
    
    private float CalculateOscillationScore(ChallengeMetrics metrics)
    {
        return 100.0f / (1.0f + Math.Abs(metrics.PopulationVariance - 0.2f) * 10.0f);
    }
    
    private bool DetectSymmetry(LeniaSimulation sim)
    {
        // Simplified symmetry detection
        return true; // Would implement actual symmetry detection
    }
    
    private float GetSymmetryScore(LeniaSimulation sim)
    {
        // Would calculate actual symmetry score
        return 0.8f;
    }
    
    private float GetOrganizationScore(LeniaSimulation sim)
    {
        // Would calculate how organized/structured the pattern is
        return 0.7f;
    }
    
    private bool IsUnlocked(Challenge challenge)
    {
        if (challenge.UnlockedBy.Count == 0) return true;
        
        return challenge.UnlockedBy.All(prereq => 
            challenges.ContainsKey(prereq) && challenges[prereq].IsCompleted);
    }
    
    private Color GetDifficultyColor(ChallengeDifficulty difficulty)
    {
        return difficulty switch
        {
            ChallengeDifficulty.Beginner => new Color(0.4f, 0.8f, 0.4f),
            ChallengeDifficulty.Intermediate => new Color(0.6f, 0.8f, 0.4f),
            ChallengeDifficulty.Advanced => new Color(0.8f, 0.8f, 0.4f),
            ChallengeDifficulty.Expert => new Color(0.8f, 0.6f, 0.4f),
            ChallengeDifficulty.Master => new Color(0.8f, 0.4f, 0.4f),
            _ => new Color(0.5f, 0.5f, 0.5f)
        };
    }
    
    private string GetDifficultyText(ChallengeDifficulty difficulty)
    {
        return difficulty switch
        {
            ChallengeDifficulty.Beginner => "★ Beginner",
            ChallengeDifficulty.Intermediate => "★★ Intermediate", 
            ChallengeDifficulty.Advanced => "★★★ Advanced",
            ChallengeDifficulty.Expert => "★★★★ Expert",
            ChallengeDifficulty.Master => "★★★★★ Master",
            _ => difficulty.ToString()
        };
    }
    
    private string GetCategoryDisplayName(ChallengeType type)
    {
        return type switch
        {
            ChallengeType.Survival => "🛡️ Survival",
            ChallengeType.Speed => "⚡ Speed",
            ChallengeType.Efficiency => "🎯 Efficiency",
            ChallengeType.Discovery => "🔍 Discovery",
            ChallengeType.Creativity => "🎨 Creative",
            ChallengeType.Optimization => "⚙️ Optimization",
            _ => type.ToString()
        };
    }
    
    // Achievement helper methods
    public int GetCompletedChallengesCount()
    {
        return challenges.Values.Count(c => c.IsCompleted);
    }
    
    public int GetCompletedChallengesByType(ChallengeType type)
    {
        return challenges.Values.Count(c => c.Type == type && c.IsCompleted);
    }
    
    public int GetPerfectScoresCount()
    {
        // Would calculate based on challenge-specific perfect score thresholds
        return challenges.Values.Count(c => c.BestScore > c.RewardPoints * 0.95f);
    }
    
    public bool HasCompletedAllCategories()
    {
        var types = System.Enum.GetValues<ChallengeType>();
        return types.All(type => challenges.Values.Any(c => c.Type == type && c.IsCompleted));
    }
    
    private void LoadProgress()
    {
        // Load challenge progress and leaderboards from file
        var saveFile = FileAccess.Open("user://challenge_progress.save", FileAccess.ModeFlags.Read);
        if (saveFile != null)
        {
            // Implementation would load progress
            saveFile.Close();
        }
    }
    
    private void SaveProgress()
    {
        // Save challenge progress and leaderboards to file
        var saveData = new Godot.Collections.Dictionary();
        foreach (var challenge in challenges.Values)
        {
            saveData[challenge.Id] = new Godot.Collections.Dictionary
            {
                ["completed"] = challenge.IsCompleted,
                ["best_score"] = challenge.BestScore
            };
        }
        
        var saveFile = FileAccess.Open("user://challenge_progress.save", FileAccess.ModeFlags.Write);
        if (saveFile != null)
        {
            saveFile.StoreString(Json.Stringify(saveData));
            saveFile.Close();
        }
    }
}