using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.BalintHelper.Entities
{
    [CustomEntity("BalintHelper/SilentFloatingDebris")]
    [Tracked(false)]
    public class SilentFloatingDebris : FloatingDebris
    {
        public event Action<Vector2>? OnExploded;

        public SilentFloatingDebris(Vector2 position, int width, int height) : base(position)
        {
            Collider = new Hitbox(width, height);

            // Remove the PlayerCollider component so the player doesn't bump it around
            PlayerCollider playerCollider = Get<PlayerCollider>();
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

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Forcefully inject this subclass instance into the vanilla FloatingDebris tracker list so Puffer can find it.
            if (scene.Tracker.Entities.TryGetValue(typeof(FloatingDebris), out var list))
            {
                list.Add(this);
            }
        }

        public override void Removed(Scene scene)
        {
            if (scene.Tracker.Entities.TryGetValue(typeof(FloatingDebris), out var list))
            {
                list.Remove(this);
            }
            base.Removed(scene);
        }

        public override void Update()
        {
            // Leaving this empty to disable all physics/movement
        }

        public override void Render()
        {
            // Leaving this empty to disable in-game drawing
        }

        public void TriggerExplodeEvent(Vector2 from)
        {
            OnExploded?.Invoke(from);
        }
    }
}