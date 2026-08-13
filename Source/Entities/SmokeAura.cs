using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Entities
{
    [CustomEntity("BalintHelper/SmokeAura")]
    [Tracked]
    public class SmokeAura : Entity
    {
        private const string LogTag = "BalintHelper/SmokeAura";

        private static readonly Color DefaultColorA = Calc.HexToColor("463759");
        private static readonly Color DefaultColorB = Calc.HexToColor("8f7aa8");

        public static ParticleType P_Smoke;
        public static ParticleType P_Sparkle;

        public static void InitializeParticles()
        {
            if (P_Smoke != null && P_Sparkle != null)
            {
                return;
            }

            P_Smoke = new ParticleType
            {
                Source = GFX.Game["particles/fire"],
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.6f,
                SizeRange = 0.6f,
                Size = 1f,
                SpeedMin = 6f,
                SpeedMax = 18f,
                SpeedMultiplier = 0.98f,
                Direction = -(float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 3f,
                Acceleration = new Vector2(0f, -8f),
                RotationMode = ParticleType.RotationModes.Random
            };

            P_Sparkle = new ParticleType
            {
                Source = GFX.Game["particles/shard"],
                FadeMode = ParticleType.FadeModes.Linear,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                Size = 0.66f,
                SpeedMin = 4f,
                SpeedMax = 10f,
                Direction = -(float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 2f
            };

        }

        private readonly float width;
        private readonly float height;
        private readonly string sliderName;
        private float opacity = 1f;

        private SmokeAuraRenderer? renderer;
        private readonly Color ColorA;
        private readonly Color ColorB;
        public SmokeAura(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            InitializeParticles();
            width = Math.Max(data.Width, 8f);
            height = Math.Max(data.Height, 8f);
            sliderName = data.Attr("sessionSlider", "");
            Depth = -1000000;
            ColorA = data.HexColor("colorA", DefaultColorA);
            ColorB = data.HexColor("colorB", DefaultColorB);

            Tag |= Tags.TransitionUpdate;

            Logger.Log(LogLevel.Info, LogTag,
                $"Constructed at {Position} width={width} height={height} sliderName='{sliderName}'");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (scene == null)
            {
                Logger.Log(LogLevel.Error, LogTag, "Added called with null Scene.");
                RemoveSelf();
                return;
            }

            if (P_Smoke == null || P_Sparkle == null)
            {
                Logger.Log(LogLevel.Error, LogTag,
                    "Added but particles were null");
                RemoveSelf();
                return;
            }

            try
            {
                renderer = SmokeAuraRenderer.FindOrCreate(scene);

                if (renderer == null)
                {
                    Logger.Log(LogLevel.Error, LogTag, "SmokeAuraRenderer.FindOrCreate returned null.");
                    RemoveSelf();
                    return;
                }
                else if (renderer.Smoke == null)
                {
                    Logger.Log(LogLevel.Error, LogTag, "Renderer was assigned but renderer.Smoke is null.");
                    RemoveSelf();
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LogTag, $"Exception while calling SmokeAuraRenderer.FindOrCreate: {ex}");
                renderer = null;
                RemoveSelf();
                return;
            }
        }

        public override void Update()
        {
            base.Update();

            if (Scene == null)
            {
                Logger.Log(LogLevel.Error, LogTag, "Update() called but Scene is null. Skipping this frame.");
                return;
            }

            Level level = SceneAs<Level>();
            if (level == null)
            {
                Logger.Log(LogLevel.Warn, LogTag,
                    $"Update() called but SceneAs<Level>() was null. Skipping this frame.");
                return;
            }

            if (!string.IsNullOrEmpty(sliderName))
            {
                if (level.Session == null)
                {
                    Logger.Log(LogLevel.Error, LogTag, "level.Session is null while trying to read sessionSlider. Skipping slider read.");
                }
                else
                {
                    opacity = Calc.Clamp(level.Session.GetSlider(sliderName), 0f, 1f);
                }
            }

            if (opacity <= 0f)
            {
                return;
            }


            if (renderer == null)
            {
                Logger.Log(LogLevel.Warn, LogTag, "renderer was null in Update(). Attempting to re-acquire.");
                renderer = SmokeAuraRenderer.FindOrCreate(Scene);
            }

            if (renderer == null)
            {
                Logger.Log(LogLevel.Warn, LogTag, "Update() skipped emission: renderer is still null after re-acquire attempt.");
                return;
            }

            if (renderer.Smoke == null)
            {
                Logger.Log(LogLevel.Error, LogTag, "Update() skipped emission: renderer.Smoke is null.");
                return;
            }

            if (P_Smoke == null || P_Sparkle == null)
            {
                Logger.Log(LogLevel.Error, LogTag, "Update() skipped emission: P_Smoke or P_Sparkle is null (InitializeParticles never ran).");
                return;
            }

            float area = width * height;
            int smokeCount = Math.Max(1, (int)(area / 512f));

            if (Scene.OnInterval(0.03f))
            {
                for (int i = 0; i < smokeCount; i++)
                {
                    Vector2 pos = Position + new Vector2(
                        Calc.Random.Range(0f, width),
                        Calc.Random.Range(0f, height)
                    );
                    Color c = Color.Lerp(ColorA, ColorB, Calc.Random.NextFloat()) * opacity;

                    try
                    {
                        renderer.Smoke.Emit(P_Smoke, pos, c);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LogTag, $"Exception in renderer.Smoke.Emit: {ex}");
                    }
                }
            }

            if (Scene.OnInterval(0.12f))
            {
                Vector2 pos = Position + new Vector2(
                    Calc.Random.Range(0f, width),
                    Calc.Random.Range(0f, height * 0.5f)
                );
                if (level.ParticlesFG == null)
                {
                    Logger.Log(LogLevel.Error, LogTag, "level.ParticlesFG is null. Cannot emit sparkle particle.");
                }
                else
                {
                    try
                    {
                        level.ParticlesFG.Emit(P_Sparkle, pos, Color.White * opacity);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, LogTag, $"Exception in ParticlesFG.Emit (sparkle): {ex}");
                    }
                }
            }
        }
    }
    public class SmokeAuraRenderer : Renderer
    {
        private const string LogTag = "BalintHelper/SmokeAuraRenderer";
        public ParticleSystem Smoke { get; private set; }

        private VirtualRenderTarget? smokeBuffer;
        private VirtualRenderTarget? blurTemp;
        private VirtualRenderTarget? blurredSmoke;

        private bool pendingRemoval;

        public SmokeAuraRenderer()
        {
            Smoke = new ParticleSystem(0, 200) { Visible = false };
            Logger.Log(LogLevel.Info, LogTag, "New SmokeAuraRenderer instance and ParticleSystem created.");
        }

        public static SmokeAuraRenderer? FindOrCreate(Scene scene)
        {
            if (scene == null)
            {
                Logger.Log(LogLevel.Error, LogTag, "FindOrCreate called with null Scene.");
                return null;
            }

            var existing = scene.RendererList.Renderers
                .OfType<SmokeAuraRenderer>()
                .FirstOrDefault();

            if (existing != null)
            {
                existing.pendingRemoval = false;
                return existing;
            }

            Logger.Log(LogLevel.Info, LogTag, "No existing SmokeAuraRenderer found in this scene. Creating a new one.");

            var created = new SmokeAuraRenderer();

            try
            {
                scene.Add(created);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LogTag, $"Exception while adding new renderer to scene: {ex}");
                return null;
            }

            return created;
        }

        public static void CompositeForScene(Scene scene)
        {
            if (scene == null)
            {
                return;
            }

            var active = scene.RendererList.Renderers
                .OfType<SmokeAuraRenderer>()
                .FirstOrDefault();

            active?.Composite();
        }

        private void EnsureBuffers()
        {
            if (smokeBuffer == null || smokeBuffer.IsDisposed)
            {
                try
                {
                    smokeBuffer = VirtualContent.CreateRenderTarget("smoke-aura", 320, 180);
                    blurTemp = VirtualContent.CreateRenderTarget("smoke-aura-temp", 320, 180);
                    blurredSmoke = VirtualContent.CreateRenderTarget("smoke-aura-blurred", 320, 180);
                    Logger.Log(LogLevel.Info, LogTag, "Render targets (re)created.");
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, LogTag, $"Exception creating VirtualRenderTargets: {ex}");
                }
            }
        }

        private void DisposeBuffers()
        {
            try
            {
                smokeBuffer?.Dispose();
                blurTemp?.Dispose();
                blurredSmoke?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LogTag, $"Exception disposing render targets: {ex}");
            }
            finally
            {
                smokeBuffer = null;
                blurTemp = null;
                blurredSmoke = null;
            }
        }
        public void HardReset()
        {
            Smoke.Clear();
            DisposeBuffers();
            pendingRemoval = false;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            EnsureBuffers();

            Smoke.Update();

            int liveAuras = scene.Tracker.CountEntities<SmokeAura>();

            if (liveAuras == 0 && !pendingRemoval)
            {
                pendingRemoval = true;
                Logger.Log(LogLevel.Info, LogTag, "No live SmokeAura entities. Scheduling self-removal at end of frame.");

                scene.OnEndOfFrame += () =>
                {
                    if (scene.Tracker.CountEntities<SmokeAura>() == 0)
                    {
                        Logger.Log(LogLevel.Info, LogTag, "Removing renderer and disposing buffers.");
                        scene.Remove(this);
                        DisposeBuffers();
                    }
                    else
                    {
                        pendingRemoval = false;
                    }
                };
            }
            else if (liveAuras > 0)
            {
                pendingRemoval = false;
            }
        }

        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);

            if (smokeBuffer == null || smokeBuffer.IsDisposed || blurTemp == null || blurredSmoke == null)
            {
                Logger.Log(LogLevel.Warn, LogTag, "BeforeRender skipped: one or more render targets are null/disposed.");
                return;
            }

            if (scene is not Level level)
            {
                Logger.Log(LogLevel.Warn, LogTag, $"BeforeRender skipped: Scene is not a Level (got {scene?.GetType()}).");
                return;
            }

            if (level.Camera == null)
            {
                Logger.Log(LogLevel.Error, LogTag, "BeforeRender skipped: level.Camera is null.");
                return;
            }

            try
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(smokeBuffer);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

                Draw.SpriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.Additive,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    null,
                    level.Camera.Matrix
                );

                Smoke.Render();

                Draw.SpriteBatch.End();

                GaussianBlur.Blur(
                    smokeBuffer,
                    blurTemp,
                    blurredSmoke,
                    0f,
                    true,
                    GaussianBlur.Samples.Nine,
                    1f,
                    GaussianBlur.Direction.Both,
                    1.5f
                );
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LogTag, $"Exception during BeforeRender draw/blur pass: {ex}");
            }
        }

        public void Composite()
        {
            if (blurredSmoke == null || blurredSmoke.IsDisposed)
            {
                Logger.Log(LogLevel.Warn, LogTag, "Composite skipped: blurredSmoke is null/disposed.");
                return;
            }

            try
            {
                Matrix matrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;

                Draw.SpriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.Additive,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    null,
                    matrix
                );

                Draw.SpriteBatch.Draw((RenderTarget2D)blurredSmoke, Vector2.Zero, Color.White);

                Draw.SpriteBatch.End();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LogTag, $"Exception during Composite: {ex}");
            }
        }
    }
}