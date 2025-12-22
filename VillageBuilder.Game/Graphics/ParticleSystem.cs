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
        Success
    }

    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Life;
        public float Size;
    }

    public class ParticleSystem
    {
        private List<Particle> _particles = new();

        public void Emit(Vector2 position, ParticleType type)
        {
            for (int i = 0; i < 10; i++)
            {
                var particle = new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)(Random.Shared.NextDouble() - 0.5) * 2,
                        (float)(Random.Shared.NextDouble() - 0.5) * 2
                    ),
                    Color = GetParticleColor(type),
                    Life = 1.0f,
                    Size = 4.0f
                };
                _particles.Add(particle);
            }
        }

        public void Update(float deltaTime)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Position += p.Velocity * deltaTime * 50;
                p.Life -= deltaTime;
                p.Size *= 0.98f;

                if (p.Life <= 0)
                    _particles.RemoveAt(i);
            }
        }

        public void Render()
        {
            foreach (var p in _particles)
            {
                var color = new Color(p.Color.R, p.Color.G, p.Color.B, (byte)(p.Life * 255));
                Raylib.DrawCircleV(p.Position, p.Size, color);
            }
        }

        private Color GetParticleColor(ParticleType type)
        {
            return type switch
            {
                ParticleType.Smoke => Color.Gray,
                ParticleType.Fire => Color.Orange,
                ParticleType.Build => Color.Blue,
                ParticleType.Error => Color.Red,
                ParticleType.Success => Color.Green,
                _ => Color.White
            };
        }
    }
}
