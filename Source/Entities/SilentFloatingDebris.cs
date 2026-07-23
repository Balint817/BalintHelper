using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.BalintHelper.Entities
{
    [CustomEntity("BalintHelper/SilentFloatingDebris")]
    [TrackedAs(typeof(FloatingDebris))]
    public class SilentFloatingDebris : FloatingDebris
    {
        public event Action<Vector2>? OnExploded;

        public SilentFloatingDebris(Vector2 position, int width, int height) : base(position)
        {
            Collider = new Hitbox(width, height);

            // Remove the PlayerCollider component
            var playerCollider = Get<PlayerCollider>();
            if (playerCollider != null)
            {
                playerCollider.OnCollide = null;
                Remove(playerCollider);
            }
        }

        public SilentFloatingDebris(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        public override void Update()
        {
            // disable physics
        }

        public override void Render()
        {
            // dont draw
        }

        public void TriggerExplodeEvent(Vector2 from)
        {
            OnExploded?.Invoke(from);
        }
    }
}