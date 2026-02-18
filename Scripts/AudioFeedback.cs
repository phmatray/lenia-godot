using Godot;
using System;

public partial class AudioFeedback : Node2D
{
    [Signal]
    public delegate void SoundTriggeredEventHandler(string soundType, float intensity);
    
    [Export] public bool AudioEnabled = true;
    [Export] public float VolumeMultiplier = 0.3f;
    [Export] public float PopulationThreshold = 0.1f;
    [Export] public float GrowthThreshold = 0.05f;
    
    private AudioStreamPlayer2D populationTone;
    private AudioStreamPlayer2D growthTone;
    private AudioStreamPlayer2D paintSound;
    private AudioStreamPlayer2D patternSound;
    private AudioStreamGenerator populationGenerator;
    private AudioStreamGenerator growthGenerator;
    private AudioStreamGeneratorPlayback populationPlayback;
    private AudioStreamGeneratorPlayback growthPlayback;
    
    private float previousPopulation = 0.0f;
    private float previousGrowthRate = 0.0f;
    private float baseFrequency = 220.0f; // A3 note
    
    public override void _Ready()
    {
        SetupAudioNodes();
        CreateGenerativeAudio();
    }
    
    private void SetupAudioNodes()
    {
        // Population tone - continuous ambient sound based on total population
        populationTone = new AudioStreamPlayer2D();
        populationGenerator = new AudioStreamGenerator();
        populationGenerator.MixRate = 44100;
        populationGenerator.BufferLength = 0.1f;
        populationTone.Stream = populationGenerator;
        populationTone.VolumeDb = -20.0f;
        populationTone.PitchScale = 1.0f;
        AddChild(populationTone);
        
        // Growth tone - harmonic overlay based on growth rate
        growthTone = new AudioStreamPlayer2D();
        growthGenerator = new AudioStreamGenerator();
        growthGenerator.MixRate = 44100;
        growthGenerator.BufferLength = 0.1f;
        growthTone.Stream = growthGenerator;
        growthTone.VolumeDb = -25.0f;
        AddChild(growthTone);
        
        // Paint sound - immediate feedback for user interaction
        paintSound = new AudioStreamPlayer2D();
        var paintGenerator = new AudioStreamGenerator();
        paintGenerator.MixRate = 44100;
        paintGenerator.BufferLength = 0.05f;
        paintSound.Stream = paintGenerator;
        paintSound.VolumeDb = -15.0f;
        AddChild(paintSound);
        
        // Pattern sound - melodic feedback for pattern changes
        patternSound = new AudioStreamPlayer2D();
        var patternGenerator = new AudioStreamGenerator();
        patternGenerator.MixRate = 44100;
        patternGenerator.BufferLength = 0.2f;
        patternSound.Stream = patternGenerator;
        patternSound.VolumeDb = -18.0f;
        AddChild(patternSound);
    }
    
    private void CreateGenerativeAudio()
    {
        if (!AudioEnabled) return;
        
        populationTone.Play();
        growthTone.Play();
        
        populationPlayback = (AudioStreamGeneratorPlayback)populationTone.GetStreamPlayback();
        growthPlayback = (AudioStreamGeneratorPlayback)growthTone.GetStreamPlayback();
    }
    
    public void UpdateAudio(float[,] grid, int gridWidth, int gridHeight, float deltaTime)
    {
        if (!AudioEnabled || populationPlayback == null || growthPlayback == null) return;
        
        // Calculate population metrics
        float totalPopulation = 0.0f;
        float totalGrowth = 0.0f;
        int activeCells = 0;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                float cellValue = grid[x, y];
                totalPopulation += cellValue;
                
                if (cellValue > 0.1f)
                {
                    activeCells++;
                    totalGrowth += Mathf.Abs(cellValue - previousPopulation / (gridWidth * gridHeight));
                }
            }
        }
        
        float populationDensity = totalPopulation / (gridWidth * gridHeight);
        float growthRate = totalGrowth / Math.Max(activeCells, 1);
        
        // Generate population tone (ambient drone)
        if (populationDensity > PopulationThreshold)
        {
            float frequency = baseFrequency * (1.0f + populationDensity * 2.0f);
            float amplitude = Mathf.Clamp(populationDensity * VolumeMultiplier, 0.0f, 0.3f);
            GenerateTone(populationPlayback, frequency, amplitude, "sine");
        }
        
        // Generate growth tone (harmonic overlay)
        if (growthRate > GrowthThreshold)
        {
            float frequency = baseFrequency * 1.5f * (1.0f + growthRate * 3.0f);
            float amplitude = Mathf.Clamp(growthRate * VolumeMultiplier * 0.5f, 0.0f, 0.2f);
            GenerateTone(growthPlayback, frequency, amplitude, "triangle");
        }
        
        previousPopulation = totalPopulation;
        previousGrowthRate = growthRate;
        
        EmitSignal(SignalName.SoundTriggered, "ambient", populationDensity + growthRate);
    }
    
    public void PlayPaintSound(float intensity, Vector2 position)
    {
        if (!AudioEnabled) return;
        
        var paintPlayback = (AudioStreamGeneratorPlayback)paintSound.GetStreamPlayback();
        if (paintPlayback == null) return;
        
        paintSound.Position = position;
        paintSound.Play();
        
        // Create a brief, pleasant paint sound
        float frequency = baseFrequency * 2.0f * (1.0f + intensity);
        float amplitude = Mathf.Clamp(intensity * VolumeMultiplier * 0.8f, 0.1f, 0.4f);
        
        var tween = CreateTween();
        tween.TweenMethod(Callable.From<float>(amp => GenerateTone(paintPlayback, frequency, amp, "sine")), 
                         amplitude, 0.0f, 0.15f);
        
        EmitSignal(SignalName.SoundTriggered, "paint", intensity);
    }
    
    public void PlayPatternSound(string patternName)
    {
        if (!AudioEnabled) return;
        
        var patternPlayback = (AudioStreamGeneratorPlayback)patternSound.GetStreamPlayback();
        if (patternPlayback == null) return;
        
        patternSound.Play();
        
        // Different melodies for different patterns
        var frequencies = GetPatternMelody(patternName);
        PlayMelody(patternPlayback, frequencies);
        
        EmitSignal(SignalName.SoundTriggered, "pattern", 1.0f);
    }
    
    private float[] GetPatternMelody(string patternName)
    {
        return patternName.ToLower() switch
        {
            "orbium" => new float[] { 440.0f, 554.37f, 659.25f, 880.0f }, // A4, C#5, E5, A5
            "random" => new float[] { 329.63f, 392.0f, 523.25f, 392.0f }, // E4, G4, C5, G4
            "ring" => new float[] { 523.25f, 659.25f, 783.99f, 659.25f }, // C5, E5, G5, E5
            "cross" => new float[] { 392.0f, 493.88f, 587.33f, 493.88f }, // G4, B4, D5, B4
            "blob" => new float[] { 261.63f, 329.63f, 392.0f, 261.63f }, // C4, E4, G4, C4
            _ => new float[] { baseFrequency, baseFrequency * 1.25f, baseFrequency * 1.5f }
        };
    }
    
    private void PlayMelody(AudioStreamGeneratorPlayback playback, float[] frequencies)
    {
        var tween = CreateTween();
        float noteDuration = 0.3f;
        
        for (int i = 0; i < frequencies.Length; i++)
        {
            float delay = i * noteDuration;
            tween.TweenCallback(Callable.From(() => GenerateTone(playback, frequencies[i], VolumeMultiplier * 0.4f, "sine")))
                 .SetDelay(delay);
        }
    }
    
    private void GenerateTone(AudioStreamGeneratorPlayback playback, float frequency, float amplitude, string waveType)
    {
        if (playback == null) return;
        
        int frames = playback.GetFramesAvailable();
        if (frames == 0) return;
        
        var buffer = new Vector2[frames];
        
        for (int i = 0; i < frames; i++)
        {
            float phase = (float)(Time.GetUnixTimeFromSystem() * frequency + i) / 44100.0f;
            float sample = waveType switch
            {
                "sine" => Mathf.Sin(phase * Mathf.Pi * 2.0f),
                "triangle" => Mathf.Asin(Mathf.Sin(phase * Mathf.Pi * 2.0f)) * 2.0f / Mathf.Pi,
                "square" => Mathf.Sign(Mathf.Sin(phase * Mathf.Pi * 2.0f)),
                _ => Mathf.Sin(phase * Mathf.Pi * 2.0f)
            };
            
            sample *= amplitude;
            buffer[i] = new Vector2(sample, sample); // Stereo
        }
        
        playback.PushBuffer(buffer);
    }
    
    public void SetAudioEnabled(bool enabled)
    {
        AudioEnabled = enabled;
        
        if (!enabled)
        {
            populationTone?.Stop();
            growthTone?.Stop();
            paintSound?.Stop();
            patternSound?.Stop();
        }
        else
        {
            CreateGenerativeAudio();
        }
    }
    
    public void SetVolume(float volume)
    {
        VolumeMultiplier = Mathf.Clamp(volume, 0.0f, 1.0f);
        
        if (populationTone != null)
            populationTone.VolumeDb = Mathf.LinearToDb(VolumeMultiplier * 0.3f);
        if (growthTone != null)
            growthTone.VolumeDb = Mathf.LinearToDb(VolumeMultiplier * 0.2f);
        if (paintSound != null)
            paintSound.VolumeDb = Mathf.LinearToDb(VolumeMultiplier * 0.5f);
        if (patternSound != null)
            patternSound.VolumeDb = Mathf.LinearToDb(VolumeMultiplier * 0.4f);
    }
}