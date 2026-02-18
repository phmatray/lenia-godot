using Godot;
using System;
using System.Collections.Generic;

public partial class ParticleEffects : Node2D
{
    [Export] public bool EffectsEnabled = true;
    [Export] public float ParticleIntensity = 0.7f;
    [Export] public int MaxParticles = 500;
    
    private class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public float MaxLife;
        public Color Color;
        public float Size;
        public ParticleType Type;
    }
    
    public enum ParticleType
    {
        Growth,
        Decay,
        Movement,
        Paint,
        Birth,
        Death
    }
    
    private List<Particle> particles;
    private float[,] previousGrid;
    private Vector2 gridScale;
    private Random random;
    private LeniaSimulation simulation;
    
    // Particle pools for performance
    private Queue<Particle> particlePool;
    
    public override void _Ready()
    {
        particles = new List<Particle>();
        particlePool = new Queue<Particle>();
        random = new Random();
        
        // Pre-allocate particle pool
        for (int i = 0; i < MaxParticles * 2; i++)
        {
            particlePool.Enqueue(new Particle());
        }
    }
    
    public void Initialize(LeniaSimulation sim)
    {
        simulation = sim;
        if (sim != null)
        {
            previousGrid = new float[sim.GridWidth, sim.GridHeight];
            gridScale = new Vector2(1.0f, 1.0f); // Will be updated based on display scale
        }
    }
    
    public void SetGridScale(Vector2 scale)
    {
        gridScale = scale;
    }
    
    public override void _Process(double delta)
    {
        if (!EffectsEnabled || simulation == null) return;
        
        UpdateParticles((float)delta);
        
        // Spawn new particles based on grid changes
        if (Engine.GetProcessFrames() % 3 == 0) // Every 3 frames for performance
        {
            AnalyzeGridChanges();
        }
    }
    
    public override void _Draw()
    {
        if (!EffectsEnabled) return;
        
        foreach (var particle in particles)
        {
            DrawParticle(particle);
        }
    }
    
    private void UpdateParticles(float delta)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var particle = particles[i];
            
            // Update particle physics
            particle.Position += particle.Velocity * delta;
            particle.Life -= delta;
            
            // Apply particle-specific behaviors
            UpdateParticleByType(particle, delta);
            
            // Remove dead particles
            if (particle.Life <= 0)
            {
                particles.RemoveAt(i);
                ReturnParticleToPool(particle);
            }
        }
        
        QueueRedraw();
    }
    
    private void UpdateParticleByType(Particle particle, float delta)
    {
        float lifeRatio = particle.Life / particle.MaxLife;
        
        switch (particle.Type)
        {
            case ParticleType.Growth:
                // Particles grow and fade as they represent expanding life
                particle.Size = (1.0f - lifeRatio) * 3.0f + 1.0f;
                particle.Color = particle.Color with { A = lifeRatio * 0.8f };
                particle.Velocity *= 0.95f; // Slow down over time
                break;
                
            case ParticleType.Decay:
                // Particles shrink and fall as they represent dying cells
                particle.Size = lifeRatio * 2.0f;
                particle.Color = particle.Color with { A = lifeRatio * 0.6f };
                particle.Velocity += new Vector2(0, 20.0f * delta); // Gravity effect
                break;
                
            case ParticleType.Movement:
                // Trail particles for moving creatures
                particle.Size = lifeRatio * 1.5f;
                particle.Color = particle.Color with { A = lifeRatio * 0.7f };
                break;
                
            case ParticleType.Paint:
                // Immediate feedback for user painting
                particle.Size = (1.0f - lifeRatio) * 4.0f + 0.5f;
                particle.Color = particle.Color with { A = lifeRatio };
                break;
                
            case ParticleType.Birth:
                // Explosive birth particles
                particle.Size = (1.0f - lifeRatio) * 2.0f + 1.0f;
                particle.Color = particle.Color with { A = lifeRatio * 0.9f };
                particle.Velocity *= 0.98f;
                break;
                
            case ParticleType.Death:
                // Implosion death particles
                particle.Size = lifeRatio * 3.0f;
                particle.Color = particle.Color with { A = lifeRatio * 0.5f };
                particle.Velocity *= 1.02f; // Slight acceleration inward
                break;
        }
    }
    
    private void AnalyzeGridChanges()
    {
        if (simulation == null || previousGrid == null) return;
        
        var currentGrid = simulation.GetCurrentGrid();
        if (currentGrid == null) return;
        
        for (int x = 0; x < simulation.GridWidth; x += 2) // Sample every other cell for performance
        {
            for (int y = 0; y < simulation.GridHeight; y += 2)
            {
                float current = currentGrid[x, y];
                float previous = previousGrid[x, y];
                float change = current - previous;
                
                // Spawn particles based on significant changes
                if (Mathf.Abs(change) > 0.05f)
                {
                    Vector2 worldPos = new Vector2(x * gridScale.X, y * gridScale.Y);
                    
                    if (change > 0.05f) // Growth
                    {
                        if (previous < 0.1f && current > 0.3f) // Birth
                        {
                            SpawnBirthParticles(worldPos, current);
                        }
                        else // Regular growth
                        {
                            SpawnGrowthParticles(worldPos, change);
                        }
                    }
                    else if (change < -0.05f) // Decay
                    {
                        if (previous > 0.3f && current < 0.1f) // Death
                        {
                            SpawnDeathParticles(worldPos, previous);
                        }
                        else // Regular decay
                        {
                            SpawnDecayParticles(worldPos, -change);
                        }
                    }
                }
                
                // Detect movement (patterns with high activity)
                if (current > 0.5f && GetNeighborActivity(currentGrid, x, y) > 2.0f)
                {
                    SpawnMovementParticles(new Vector2(x * gridScale.X, y * gridScale.Y), current);
                }
                
                previousGrid[x, y] = current;
            }
        }
    }
    
    private float GetNeighborActivity(float[,] grid, int centerX, int centerY)
    {
        float activity = 0.0f;
        int samples = 0;
        
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int x = centerX + dx;
                int y = centerY + dy;
                if (x >= 0 && x < simulation.GridWidth && y >= 0 && y < simulation.GridHeight)
                {
                    activity += grid[x, y];
                    samples++;
                }
            }
        }
        
        return samples > 0 ? activity / samples : 0.0f;
    }
    
    private void SpawnGrowthParticles(Vector2 position, float intensity)
    {
        int count = Mathf.RoundToInt(intensity * ParticleIntensity * 3);
        count = Mathf.Min(count, 5); // Limit per spawn
        
        for (int i = 0; i < count; i++)
        {
            var particle = GetParticleFromPool();
            if (particle == null) return;
            
            particle.Position = position + GetRandomOffset(5.0f);
            particle.Velocity = GetRandomVelocity(30.0f);
            particle.Life = particle.MaxLife = 1.0f + (float)random.NextDouble() * 0.5f;
            particle.Color = new Color(0.4f, 1.0f, 0.6f, 0.8f); // Green growth
            particle.Size = 1.0f + intensity * 2.0f;
            particle.Type = ParticleType.Growth;
            
            particles.Add(particle);
        }
    }
    
    private void SpawnDecayParticles(Vector2 position, float intensity)
    {
        int count = Mathf.RoundToInt(intensity * ParticleIntensity * 2);
        count = Mathf.Min(count, 4);
        
        for (int i = 0; i < count; i++)
        {
            var particle = GetParticleFromPool();
            if (particle == null) return;
            
            particle.Position = position + GetRandomOffset(3.0f);
            particle.Velocity = GetRandomVelocity(20.0f) + new Vector2(0, 10.0f);
            particle.Life = particle.MaxLife = 0.8f + (float)random.NextDouble() * 0.4f;
            particle.Color = new Color(1.0f, 0.4f, 0.2f, 0.6f); // Orange decay
            particle.Size = 0.5f + intensity * 1.5f;
            particle.Type = ParticleType.Decay;
            
            particles.Add(particle);
        }
    }
    
    private void SpawnMovementParticles(Vector2 position, float intensity)
    {
        if (random.NextDouble() > 0.3) return; // Don't spawn every frame
        
        var particle = GetParticleFromPool();
        if (particle == null) return;
        
        particle.Position = position + GetRandomOffset(2.0f);
        particle.Velocity = GetRandomVelocity(15.0f);
        particle.Life = particle.MaxLife = 0.6f;
        particle.Color = new Color(0.6f, 0.8f, 1.0f, 0.7f); // Blue movement
        particle.Size = 1.0f + intensity;
        particle.Type = ParticleType.Movement;
        
        particles.Add(particle);
    }
    
    private void SpawnBirthParticles(Vector2 position, float intensity)
    {
        int count = Mathf.RoundToInt(ParticleIntensity * 8);
        count = Mathf.Min(count, 12);
        
        for (int i = 0; i < count; i++)
        {
            var particle = GetParticleFromPool();
            if (particle == null) return;
            
            float angle = (float)random.NextDouble() * Mathf.Pi * 2;
            float speed = 40.0f + (float)random.NextDouble() * 30.0f;
            
            particle.Position = position;
            particle.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            particle.Life = particle.MaxLife = 1.2f + (float)random.NextDouble() * 0.8f;
            particle.Color = new Color(1.0f, 1.0f, 0.4f, 0.9f); // Bright yellow birth
            particle.Size = 2.0f + intensity;
            particle.Type = ParticleType.Birth;
            
            particles.Add(particle);
        }
    }
    
    private void SpawnDeathParticles(Vector2 position, float intensity)
    {
        int count = Mathf.RoundToInt(ParticleIntensity * 6);
        count = Mathf.Min(count, 10);
        
        for (int i = 0; i < count; i++)
        {
            var particle = GetParticleFromPool();
            if (particle == null) return;
            
            float angle = (float)random.NextDouble() * Mathf.Pi * 2;
            float speed = 15.0f + (float)random.NextDouble() * 10.0f;
            
            particle.Position = position + GetRandomOffset(8.0f);
            particle.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed * -1; // Inward motion
            particle.Life = particle.MaxLife = 1.0f + (float)random.NextDouble() * 0.5f;
            particle.Color = new Color(0.8f, 0.2f, 0.8f, 0.7f); // Purple death
            particle.Size = 1.5f + intensity * 0.5f;
            particle.Type = ParticleType.Death;
            
            particles.Add(particle);
        }
    }
    
    public void SpawnPaintParticles(Vector2 position, float intensity, bool isErase = false)
    {
        if (!EffectsEnabled) return;
        
        int count = Mathf.RoundToInt(intensity * ParticleIntensity * 5);
        count = Mathf.Min(count, 8);
        
        for (int i = 0; i < count; i++)
        {
            var particle = GetParticleFromPool();
            if (particle == null) return;
            
            particle.Position = position + GetRandomOffset(10.0f);
            particle.Velocity = GetRandomVelocity(25.0f);
            particle.Life = particle.MaxLife = 0.5f + (float)random.NextDouble() * 0.3f;
            particle.Color = isErase ? 
                new Color(1.0f, 0.3f, 0.3f, 0.8f) : // Red for erase
                new Color(0.3f, 0.8f, 1.0f, 0.8f);   // Cyan for paint
            particle.Size = 2.0f + intensity * 2.0f;
            particle.Type = ParticleType.Paint;
            
            particles.Add(particle);
        }
    }
    
    private void DrawParticle(Particle particle)
    {
        switch (particle.Type)
        {
            case ParticleType.Growth:
            case ParticleType.Birth:
                // Draw as expanding circles
                DrawCircle(particle.Position, particle.Size, particle.Color);
                break;
                
            case ParticleType.Decay:
            case ParticleType.Death:
                // Draw as fading squares
                var rect = new Rect2(particle.Position - Vector2.One * particle.Size / 2, 
                                   Vector2.One * particle.Size);
                DrawRect(rect, particle.Color);
                break;
                
            case ParticleType.Movement:
                // Draw as streaks
                var endPos = particle.Position - particle.Velocity.Normalized() * particle.Size * 2;
                DrawLine(particle.Position, endPos, particle.Color, particle.Size * 0.5f);
                break;
                
            case ParticleType.Paint:
                // Draw as sparkles
                DrawCircle(particle.Position, particle.Size, particle.Color);
                // Add sparkle effect
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * Mathf.Pi / 2;
                    var sparklePos = particle.Position + 
                        new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * particle.Size;
                    DrawCircle(sparklePos, particle.Size * 0.3f, particle.Color with { A = particle.Color.A * 0.5f });
                }
                break;
        }
    }
    
    private Vector2 GetRandomOffset(float maxDistance)
    {
        float angle = (float)random.NextDouble() * Mathf.Pi * 2;
        float distance = (float)random.NextDouble() * maxDistance;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
    }
    
    private Vector2 GetRandomVelocity(float maxSpeed)
    {
        float angle = (float)random.NextDouble() * Mathf.Pi * 2;
        float speed = (float)random.NextDouble() * maxSpeed;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
    }
    
    private Particle GetParticleFromPool()
    {
        if (particlePool.Count > 0)
        {
            return particlePool.Dequeue();
        }
        
        // Pool exhausted, create new if under limit
        if (particles.Count < MaxParticles)
        {
            return new Particle();
        }
        
        return null; // Too many particles
    }
    
    private void ReturnParticleToPool(Particle particle)
    {
        // Reset particle state
        particle.Life = 0;
        particle.MaxLife = 0;
        particle.Velocity = Vector2.Zero;
        particle.Color = Colors.White;
        particle.Size = 1.0f;
        
        particlePool.Enqueue(particle);
    }
    
    public void SetEffectsEnabled(bool enabled)
    {
        EffectsEnabled = enabled;
        if (!enabled)
        {
            // Clear all particles
            foreach (var particle in particles)
            {
                ReturnParticleToPool(particle);
            }
            particles.Clear();
            QueueRedraw();
        }
    }
    
    public void SetParticleIntensity(float intensity)
    {
        ParticleIntensity = Mathf.Clamp(intensity, 0.0f, 2.0f);
    }
    
    public void ClearAllParticles()
    {
        foreach (var particle in particles)
        {
            ReturnParticleToPool(particle);
        }
        particles.Clear();
        QueueRedraw();
    }
}