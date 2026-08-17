using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils
{
    //TODO: probably should make this interopable

    /// <summary>
    /// Caches draw operations for a shape into a <see cref="VirtualRenderTarget"/> for reuse.
    /// <para/>
    /// <b>Use only for expensive shapes! (ex.: large circle)<br/>For simple shapes (like a rect), this will be more expensive than just redrawing!</b>
    /// </summary>
    /// <typeparam name="TParameter">
    /// The type of a raw bake request.
    /// <br/>
    /// It is converted into a <typeparamref name="TKey"/> by <see cref="Bucket"/>.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The type used as the cache dictionary key. It should be a type with sane value equality.
    /// </typeparam>
    public abstract class ShapeTextureCache<TParameter, TKey> : IDisposable where TKey : notnull
    {
        private readonly Dictionary<TKey, VirtualRenderTarget> cache = [];
        private SpriteBatch? bakeBatch;
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Converts a request into a key, and returns the scale factor to apply to match the original request.
        /// </summary>
        /// <param name="drawScale">The scale to apply to match the originally requested shape.</param>
        protected abstract TKey Bucket(in TParameter request, out float drawScale);

        /// <summary>
        /// Pixel dimensions of the render target needed for a given key.
        /// </summary>
        protected abstract Point GetTargetSize(in TKey key);

        /// <summary>
        /// Returns the name to be passed to VirtualContent.CreateRenderTarget for a given key.
        /// <br/>
        /// Should preferrably be unique in case you wanna debug, but technically not required by <see cref="VirtualContent"/>
        /// </summary>
        protected abstract string GetCachedName(in TKey key);

        /// <summary>Actual draw calls that bake the shape into the target, centered at <paramref name="center"/>.</summary>
        protected abstract void DrawShape(in TKey key, Vector2 center);

        /// <summary>
        /// Release any resources owned by your derived class, if any.
        /// </summary>
        protected virtual void DisposeOwn()
        {

        }

        /// <summary>
        /// Takes in a <typeparamref name="TParameter"/> request, and returns a pre-computed <see cref="VirtualRenderTarget"/> 
        /// </summary>
        /// <param name="request">The parameters of the requested shape</param>
        /// <param name="drawScale">The scale to apply to match the originally requested shape.</param>
        /// <returns></returns>
        public VirtualRenderTarget GetVRT(in TParameter request, out float drawScale)
        {
            ThrowIfDisposed();
            Draw.Circle(default(Vector2), default, default, default, default);
            var key = Bucket(in request, out drawScale);

            if (cache.TryGetValue(key, out var vrt) && vrt.Target != null && !vrt.Target.IsDisposed)
                return vrt;

            vrt = Bake(key);
            cache[key] = vrt;
            return vrt;
        }

        private VirtualRenderTarget Bake(in TKey key)
        {
            var gd = Engine.Instance.GraphicsDevice;
            var size = GetTargetSize(in key);

            var vrt = VirtualContent.CreateRenderTarget(GetCachedName(in key), size.X, size.Y);

            // Snapshot state we're about to disturb.
            var previousTargets = gd.GetRenderTargets();
            var previousBatch = Draw.SpriteBatch;

            bakeBatch ??= new SpriteBatch(gd);

            gd.SetRenderTarget(vrt.Target);
            gd.Clear(Color.Transparent);

            Draw.SpriteBatch = bakeBatch;
            bakeBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var center = new Vector2(size.X / 2f, size.Y / 2f);
            DrawShape(in key, center);

            bakeBatch.End();

            // Restore what was previously active, so the interrupted render pass can continue.
            Draw.SpriteBatch = previousBatch;
            if (previousTargets.Length == 0)
                gd.SetRenderTarget(null);
            else
                gd.SetRenderTargets(previousTargets);

            return vrt;
        }
        /// <summary>
        /// Quick fully parameterized draw helper method for a <see cref="VirtualRenderTarget"/> (not necessarily made by this instance).
        /// </summary>
        public void DrawAt(VirtualRenderTarget vrt, Vector2 position, float scale = 1f, float rotation = 0f, Color? tint = null, SpriteEffects effects = SpriteEffects.None)
        {
            var origin = new Vector2(vrt.Target.Width / 2f, vrt.Target.Height / 2f);
            Draw.SpriteBatch.Draw(
                texture: vrt.Target,
                position: position,
                sourceRectangle: null,
                color: tint ?? Color.White,
                rotation: rotation,
                origin: origin,
                scale: scale,
                effects: effects,
                layerDepth: 0f);
        }
        /// <summary>
        /// Quick fully parameterized draw helper method for a <see cref="VirtualRenderTarget"/> (not necessarily made by this instance).
        /// </summary>
        public void DrawAt(VirtualRenderTarget vrt, Vector2 position, Vector2? scale = null, float rotation = 0f, Color? tint = null, SpriteEffects effects = SpriteEffects.None)
        {
            var origin = new Vector2(vrt.Target.Width / 2f, vrt.Target.Height / 2f);
            Draw.SpriteBatch.Draw(
                texture: vrt.Target,
                position: position,
                sourceRectangle: null,
                color: tint ?? Color.White,
                rotation: rotation,
                origin: origin,
                scale: scale ?? Vector2.One,
                effects: effects,
                layerDepth: 0f);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
        }

        /// <summary>
        /// Only clears resources the base class controls.
        /// <br/>
        /// Use <see cref="DisposeOwn"/> to clean up your own resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            foreach (var vrt in cache.Values)
                vrt?.Dispose();
            cache.Clear();

            bakeBatch?.Dispose();
            bakeBatch = null!;

            DisposeOwn();

            GC.SuppressFinalize(this);
        }
    }
    public readonly record struct CircleRequest(float Radius, int Resolution, bool Filled);
    public readonly record struct CircleKey(int RadiusUnits, int Resolution, bool Filled);

    // example implementation based on my PR to ChroniaHelper
    public sealed class CircleTextureCache : ShapeTextureCache<CircleRequest, CircleKey>, ISingleton<CircleTextureCache>
    {
        // Bucket size in pixels.
        private const float RadiusStep = 0.25f;
        private const int Padding = 6;
        public VirtualRenderTarget GetCircle(float radius, int resolution, bool filled, out float drawScale)
            => GetVRT(new CircleRequest(radius, resolution, filled), out drawScale);
        protected override CircleKey Bucket(in CircleRequest request, out float drawScale)
        {
            int units = Math.Max(1, (int)MathF.Round(request.Radius / RadiusStep));
            float bakedRadius = units * RadiusStep;
            drawScale = request.Radius / bakedRadius;
            return new CircleKey(units, request.Resolution, request.Filled);
        }
        private static float BakedRadius(in CircleKey key) => key.RadiusUnits * RadiusStep;
        protected override Point GetTargetSize(in CircleKey key)
        {
            int size = (int)Math.Ceiling((BakedRadius(in key) + Padding) * 2);
            return new Point(size, size);
        }
        protected override string GetCachedName(in CircleKey key)
            => $"BalintHelper_circle_{(key.Filled ? "fill" : "outline")}_{key.RadiusUnits}_{key.Resolution}";
        protected override void DrawShape(in CircleKey key, Vector2 center)
        {
            float bakedRadius = BakedRadius(in key);
            if (key.Filled)
                Draw.Circle(center, bakedRadius / 2f, Color.White, bakedRadius, key.Resolution);
            else
                Draw.Circle(center, bakedRadius, Color.White, key.Resolution);
        }
    }
}