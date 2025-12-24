using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;
using System.Numerics;

namespace VillageBuilder.Game.Graphics
{
    public enum ParticleType
    {
        Smoke,
        Fire,
        Build,
        Error,
        Success,
        Rain,
        Snow,
        ChimneySmoke,
        Sparkle
    }

    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Life;
        public float MaxLife; // Track original lifetime for better fading
        public float Size;
        public float RotationSpeed; // For swirling effect
        public float CurrentRotation;
        public ParticleType Type;
        public char SmokeCharacter; // For ASCII smoke rendering
    }

    public class ParticleSystem
    {
        private List<Particle> _particles = new();
        private const int MaxParticles = 1000; // Limit for performance

        public void Emit(Vector2 position, ParticleType type, int count = 10)
        {
            // Don't exceed max particles
            if (_particles.Count >= MaxParticles) return;

            for (int i = 0; i < count && _particles.Count < MaxParticles; i++)
            {
                var particle = CreateParticle(position, type);
                _particles.Add(particle);
            }
        }

        /// <summary>
        /// Emit weather particles across the visible screen area
        /// </summary>
        public void EmitWeatherParticles(ParticleType type, int screenWidth, int screenHeight, Camera2D camera)
        {
            if (type != ParticleType.Rain && type != ParticleType.Snow) return;

            // Calculate world-space visible area
            float worldLeft = camera.Target.X - (screenWidth / 2f) / camera.Zoom;
            float worldTop = camera.Target.Y - (screenHeight / 2f) / camera.Zoom;
            float worldRight = camera.Target.X + (screenWidth / 2f) / camera.Zoom;
            float worldBottom = camera.Target.Y + (screenHeight / 2f) / camera.Zoom;

            // Emit particles across top of visible area
            int particlesToEmit = type == ParticleType.Rain ? 5 : 3;
            for (int i = 0; i < particlesToEmit && _particles.Count < MaxParticles; i++)
            {
                var x = (float)(worldLeft + Random.Shared.NextDouble() * (worldRight - worldLeft));
                var y = worldTop - 20; // Start above visible area
                Emit(new Vector2(x, y), type, 1);
            }
        }

        private Particle CreateParticle(Vector2 position, ParticleType type)
        {
            return type switch
            {
                ParticleType.Rain => new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)(Random.Shared.NextDouble() * 0.5 - 0.25), // Slight horizontal drift
                        (float)(Random.Shared.NextDouble() * 8 + 12) // Fast downward
                    ),
                        Color = new Color(150, 150, 200, 180),
                            Life = 1.5f,
                            MaxLife = 1.5f,
                            Size = 2.0f,
                            RotationSpeed = 0f,
                            CurrentRotation = 0f,
                            Type = type,
                            SmokeCharacter = ' '
                        },
                        ParticleType.Snow => new Particle
                        {
                            Position = position,
                            Velocity = new Vector2(
                                (float)(Random.Shared.NextDouble() * 2 - 1), // Gentle drift
                                (float)(Random.Shared.NextDouble() * 2 + 1) // Slow fall
                            ),
                            Color = Color.White,
                            Life = 3.0f,
                            MaxLife = 3.0f,
                            Size = 3.0f,
                            RotationSpeed = 0f,
                            CurrentRotation = 0f,
                            Type = type,
                            SmokeCharacter = ' '
                        },
                ParticleType.ChimneySmoke => new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)(Random.Shared.NextDouble() * 1.0 - 0.5), // More horizontal variation
                        (float)(Random.Shared.NextDouble() * -2.0 - 1.0) // Stronger upward
                    ),
                    Color = new Color(
                        (byte)(160 + Random.Shared.Next(60)), // Vary gray tones (160-220)
                        (byte)(160 + Random.Shared.Next(60)),
                        (byte)(160 + Random.Shared.Next(60)),
                        (byte)(80 + Random.Shared.Next(60))  // Much more transparent (80-140)
                    ),
                    Life = (float)(3.0 + Random.Shared.NextDouble() * 2.0), // 3-5 seconds
                    MaxLife = (float)(3.0 + Random.Shared.NextDouble() * 2.0),
                    Size = 11.0f, // Smaller font size (was 14)
                    RotationSpeed = (float)(Random.Shared.NextDouble() * 2.0 - 1.0), // Swirl
                    CurrentRotation = (float)(Random.Shared.NextDouble() * Math.PI * 2),
                    Type = type,
                    SmokeCharacter = GetRandomSmokeCharacter(0) // Start with dense smoke
                },
                                _ => new Particle
                                {
                                    Position = position,
                                    Velocity = new Vector2(
                                        (float)(Random.Shared.NextDouble() - 0.5) * 2,
                                        (float)(Random.Shared.NextDouble() - 0.5) * 2
                                    ),
                                    Color = GetParticleColor(type),
                                    Life = 1.0f,
                                    MaxLife = 1.0f,
                                    Size = 4.0f,
                                    RotationSpeed = 0f,
                                    CurrentRotation = 0f,
                                    Type = type,
                                    SmokeCharacter = ' '
                                }
                            };
                        }

                        /// <summary>
                        /// Get a smoke character based on density (age factor)
                        /// 0.0 = dense smoke, 1.0 = dissipated
                        /// </summary>
                        private static char GetRandomSmokeCharacter(float ageFactor)
                        {
                            // Smoke characters arranged by density (dense -> light)
                            // ▓ = Dense, ▒ = Medium, ░ = Light, ∙·˙ = Dissipating, spaces = Gone

                            if (ageFactor < 0.2f)
                            {
                                // Young smoke - dense and billowy
                                char[] denseChars = { '▓', '▒', '█' };
                                return denseChars[Random.Shared.Next(denseChars.Length)];
                            }
                            else if (ageFactor < 0.4f)
                            {
                                // Maturing smoke - medium density
                                char[] mediumChars = { '▒', '▒', '▓', '░' };
                                return mediumChars[Random.Shared.Next(mediumChars.Length)];
                            }
                            else if (ageFactor < 0.6f)
                            {
                                // Aging smoke - getting lighter
                                char[] lightChars = { '░', '▒', '░', '∙' };
                                return lightChars[Random.Shared.Next(lightChars.Length)];
                            }
                            else if (ageFactor < 0.8f)
                            {
                                // Dissipating smoke - very light
                                char[] wisps = { '░', '∙', '·', '˙', '○' };
                                return wisps[Random.Shared.Next(wisps.Length)];
                            }
                            else
                            {
                                // Almost gone - tiny particles
                                char[] particles = { '·', '˙', '∙', ' ', ' ' };
                                return particles[Random.Shared.Next(particles.Length)];
                            }
                        }

        public void Update(float deltaTime)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];

                // Apply velocity with time scaling
                p.Position += p.Velocity * deltaTime * 50;
                p.Life -= deltaTime;

                // Realistic smoke behavior
                if (p.Type == ParticleType.ChimneySmoke)
                {
                    // Calculate age factor (0 = new, 1 = dying)
                    float ageFactor = 1.0f - (p.Life / p.MaxLife);

                    // Smoke rises but slows down over time (buoyancy loss)
                    p.Velocity.Y *= 0.985f; // Gradually slow upward movement

                    // Add wind turbulence (increases with age)
                    float turbulence = ageFactor * 0.8f;
                    p.Velocity.X += (float)(Random.Shared.NextDouble() - 0.5) * turbulence;

                    // Apply rotation for swirling effect
                    p.CurrentRotation += p.RotationSpeed * deltaTime;

                    // Update smoke character based on age (transitions from dense -> light)
                    // Only update character occasionally to avoid constant flicker
                    if (Random.Shared.Next(100) < 15) // 15% chance per frame
                    {
                        p.SmokeCharacter = GetRandomSmokeCharacter(ageFactor);
                    }
                }
                else
                {
                    p.Size *= 0.98f; // Others shrink
                }

                if (p.Life <= 0)
                    _particles.RemoveAt(i);
            }
        }

        public void Render()
        {
            foreach (var p in _particles)
            {
                // Calculate alpha based on remaining life
                float lifeRatio = p.Life / p.MaxLife;
                byte alpha;

                if (p.Type == ParticleType.ChimneySmoke)
                {
                    // Smoke starts transparent and fades even more
                    // Apply additional transparency multiplier for subtler effect
                    alpha = (byte)(lifeRatio * p.Color.A * 0.7f); // 70% of base alpha
                }
                else
                {
                    alpha = (byte)(lifeRatio * p.Color.A);
                }

                var color = new Color(p.Color.R, p.Color.G, p.Color.B, alpha);

                // Render based on particle type
                switch (p.Type)
                {
                    case ParticleType.Rain:
                        // Render as vertical line for rain
                        Raylib.DrawLineEx(p.Position, 
                            new Vector2(p.Position.X, p.Position.Y + p.Size * 3), 
                            1.0f, color);
                        break;

                    case ParticleType.ChimneySmoke:
                        // Render smoke as ASCII character for organic, billowing look
                        string smokeChar = p.SmokeCharacter.ToString();
                        GraphicsConfig.DrawConsoleText(
                            smokeChar,
                            (int)p.Position.X,
                            (int)p.Position.Y,
                            (int)p.Size,
                            color
                        );
                        break;

                    case ParticleType.Snow:
                        // Render as circle for snow
                        Raylib.DrawCircleV(p.Position, p.Size, color);
                        break;

                    default:
                        // Default circle rendering
                        Raylib.DrawCircleV(p.Position, p.Size, color);
                        break;
                }
            }
        }

        public int GetParticleCount() => _particles.Count;

                private Color GetParticleColor(ParticleType type)
                {
                    return type switch
                    {
                        ParticleType.Smoke => Color.Gray,
                        ParticleType.Fire => Color.Orange,
                        ParticleType.Build => Color.Blue,
                        ParticleType.Error => Color.Red,
                        ParticleType.Success => Color.Green,
                        ParticleType.Rain => new Color(150, 150, 200, 180),
                        ParticleType.Snow => Color.White,
                        ParticleType.ChimneySmoke => new Color(200, 200, 200, 150),
                        ParticleType.Sparkle => Color.Yellow,
                        _ => Color.White
                    };
                }
            }
        }
