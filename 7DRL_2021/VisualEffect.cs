using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    abstract class LerpValue<T>
    {
        public T Start, End;
        public Slider Frame = new Slider(1, 1);
        protected LerpHelper.Delegate Lerp;

        public T Value => Get(Slide);
        public float Slide => Frame.Slide;

        public void Set(T value)
        {
            Set(value, value, LerpHelper.Linear, 1);
        }

        public void Set(T newValue, LerpHelper.Delegate lerp, float time)
        {
            Set(Value, newValue, lerp, time);
        }

        public void Set(T oldValue, T newValue, LerpHelper.Delegate lerp, float time)
        {
            Start = oldValue;
            End = newValue;
            Frame = new Slider(time);
            Lerp = lerp;
        }

        public void Update(float time = 1)
        {
            Frame += time;
        }

        public abstract T Get(float slide);

        public static implicit operator T(LerpValue<T> x)
        {
            return x.Value;
        }
    }

    class LerpFloat : LerpValue<float>
    {
        public LerpFloat(float value)
        {
            Set(value);
        }

        public override float Get(float slide) => (float)Lerp(Start, End, slide);
    }

    class LerpVector2 : LerpValue<Vector2>
    {
        public LerpVector2(Vector2 value)
        {
            Set(value);
        }

        public override Vector2 Get(float slide) => Vector2.Lerp(Start, End, (float)Lerp(0, 1, slide));
    }

    class LerpColor : LerpValue<Color>
    {
        public LerpColor(Color value)
        {
            Set(value);
        }

        public override Color Get(float slide) => Color.Lerp(Start, End, (float)Lerp(0, 1, slide));
    }

    class LerpBoolean : LerpValue<bool>
    {
        public LerpBoolean(bool value)
        {
            Set(value);
        }

        public override bool Get(float slide) => slide >= 1 ? End : Start;
    }

    abstract class VisualEffect : IDrawable
    {
        public static Random Random = new Random();

        public SceneGame World { get; set; }
        public Map Map { get; set; }
        public double DrawOrder => 0;
        public bool Destroyed { get; set; }

        public bool Timeless;
        public Slider Frame;

        public VisualEffect(SceneGame world)
        {
            World = world;
            World.VisualEffects.AddLater(this);
        }

        public float GetTimeMod()
        {
            return Timeless ? 1 : World.TimeMod;
        }

        public void Destroy()
        {
            Destroyed = true;
        }

        public virtual void OnDestroy()
        {
            //NOOP
        }

        public virtual void Update()
        {
            Frame += 1;
        }

        public abstract IEnumerable<DrawPass> GetDrawPasses();

        public abstract void Draw(SceneGame scene, DrawPass pass);

        public virtual bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return true;
        }

        public void DrawIcon(SceneGame sceneGame, Vector2 vector2)
        {
            //NOOP
        }
    }

    abstract class ScreenFlash : VisualEffect
    {
        public abstract ColorMatrix ScreenColor
        {
            get;
        }
        public bool Delete = true;

        public ScreenFlash(SceneGame world, float time) : base(world)
        {
            Frame = new Slider(time);
        }

        public override void Update()
        {
            base.Update();
            if (Frame.Done && Delete)
            {
                this.Destroy();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            //NOOP
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            return Enumerable.Empty<DrawPass>();
        }
    }

    class ScreenBellWraiths : ScreenFlash
    {
        public override ColorMatrix ScreenColor => ColorMatrix.Lerp(ColorMatrix.Identity, ColorMatrix.Tint(Color.Red), 0.5f);
        BehaviorBellTower BellTower;
        float MinTime;
        float MaxTime;

        public ScreenBellWraiths(SceneGame world, BehaviorBellTower bellTower, float minTime, float maxTime) : base(world, 0)
        {
            BellTower = bellTower;
            MinTime = minTime;
            MaxTime = maxTime;
            Frame = new Slider(Random.NextFloat(minTime, maxTime));
        }

        public override void Update()
        {
            if (BellTower.Removed)
                Destroy();
            Frame += 1;
            if (Frame.Done)
            {
                new ScreenGlitchFlash(World, slide => BehaviorBellTower.Params.WithIntensity((1-slide) * 0.3f), 10);
                Frame.Reset();
                Frame.EndTime = Random.NextFloat(MinTime, MaxTime);
            }
        }
    }

    class ScreenFlashSimple : ScreenFlash
    {
        public virtual Vector2 Position
        {
            get;
            set;
        }
        ColorMatrix FlashColor;
        LerpHelper.Delegate ColorLerp;

        public override ColorMatrix ScreenColor => ColorMatrix.Lerp(ColorMatrix.Identity, FlashColor, (float)ColorLerp(1, 0, Frame.Slide));

        public ScreenFlashSimple(SceneGame world, ColorMatrix color, LerpHelper.Delegate colorLerp, int time) : base(world, time)
        {
            FlashColor = color;
            ColorLerp = colorLerp;
        }
    }

    class ScreenFade : ScreenFlash
    {
        Func<ColorMatrix> ColorFunction;
        LerpFloat Lerp;

        public override ColorMatrix ScreenColor => ColorMatrix.Lerp(ColorMatrix.Identity, ColorFunction(), Lerp);

        public ScreenFade(SceneGame world, Func<ColorMatrix> color, float start, bool delete) : base(world, 1)
        {
            ColorFunction = color;
            Lerp = new LerpFloat(start);
            Delete = false;
        }

        public override void Update()
        {
            Lerp.Update();
        }
    }

    abstract class ScreenGlitch : VisualEffect
    {
        public abstract GlitchParams Glitch
        {
            get;
        }

        public ScreenGlitch(SceneGame world) : base(world)
        {
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            //NOOP
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            return Enumerable.Empty<DrawPass>();
        }
    }

    class ScreenGlitchFlash : ScreenGlitch
    {
        Func<float, GlitchParams> GlitchFunction;

        public override GlitchParams Glitch => GlitchFunction(Frame.Slide);

        public ScreenGlitchFlash(SceneGame world, Func<float, GlitchParams> glitch, int time) : base(world)
        {
            GlitchFunction = glitch;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            base.Update();
            if (Frame.Done)
            {
                this.Destroy();
            }
        }
    }

    class ScreenGlitchFade : ScreenGlitch
    {
        Func<float, GlitchParams> FadeInFunction;
        Func<float, GlitchParams> FadeOutFunction;
        int TimeIn;
        int TimeOut;

        public override GlitchParams Glitch => Frame.Time < TimeIn ? FadeInFunction(Frame.GetSubSlide(0, TimeIn)) : FadeOutFunction(Frame.GetSubSlide(TimeIn, TimeIn + TimeOut));

        public ScreenGlitchFade(SceneGame world, Func<float, GlitchParams> fadeIn, Func<float, GlitchParams> fadeOut, int timeIn, int timeOut) : base(world)
        {
            FadeInFunction = fadeIn;
            FadeOutFunction = fadeOut;
            TimeIn = timeIn;
            TimeOut = timeOut;
            Frame = new Slider(timeIn + timeOut);
        }

        public override void Update()
        {
            base.Update();
            if (Frame.Done)
            {
                this.Destroy();
            }
        }
    }

    abstract class ScreenShake : VisualEffect
    {
        public Vector2 Offset;

        public ScreenShake(SceneGame world, int time) : base(world)
        {
            Frame = new Slider(time);
        }

        public override void Update()
        {
            base.Update();
            if (Frame.Done)
            {
                this.Destroy();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            //NOOP
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            return Enumerable.Empty<DrawPass>();
        }
    }

    class ScreenShakeRandom : ScreenShake
    {
        float Amount;
        LerpHelper.Delegate Lerp;

        public ScreenShakeRandom(SceneGame world, float amount, int time, LerpHelper.Delegate lerp) : base(world, time)
        {
            Lerp = lerp;
            Amount = amount;
        }

        public override void Update()
        {
            base.Update();

            double amount = Lerp(Amount, 0, Frame.Slide);
            double shakeAngle = Random.NextDouble() * Math.PI * 2;
            int x = (int)Math.Round(Math.Cos(shakeAngle) * amount);
            int y = (int)Math.Round(Math.Sin(shakeAngle) * amount);
            Offset = new Vector2(x, y);
        }
    }

    class ScreenShakeJerk : ScreenShake
    {
        Vector2 Jerk;

        public ScreenShakeJerk(SceneGame world, Vector2 jerk, int time) : base(world, time)
        {
            Jerk = jerk;
        }

        public override void Update()
        {
            base.Update();

            float amount = (1 - Frame.Slide);
            Offset = Jerk * amount;
        }
    }

    abstract class TimeWarp : VisualEffect
    {
        public abstract float TimeMod { get; }

        public TimeWarp(SceneGame world, int time) : base(world)
        {
            Frame = new Slider(time);
        }

        public override void Update()
        {
            base.Update();
            if (Frame.Done)
            {
                this.Destroy();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            //NOOP
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            return Enumerable.Empty<DrawPass>();
        }
    }

    class TimeFade : TimeWarp
    {
        float TimeModStart;
        LerpHelper.Delegate Lerp;

        public override float TimeMod => (float)Lerp(TimeModStart, 1, Frame.Slide);

        public TimeFade(SceneGame world, float timeMod, LerpHelper.Delegate lerp, int time) : base(world, time)
        {
            TimeModStart = timeMod;
            Lerp = lerp;
        }
    }

    class HitStop : TimeWarp
    {
        float TimeModStop;

        public override float TimeMod => TimeModStop;

        public HitStop(SceneGame world, float timeMod, int time) : base(world, time)
        {
            TimeModStop = timeMod;
        }
    }

    class SparkParticle : VisualEffect
    {
        SpriteReference Sprite;
        Vector2 Position;
        public Vector2 Velocity;
        public LerpHelper.Delegate VelocityLerp = LerpHelper.Linear;
        public float Size;
        public Color Color = Color.White;
        public DrawPass DrawPass = DrawPass.Effect;

        private Vector2 Offset => Vector2.Lerp(Vector2.Zero, Velocity, (float)VelocityLerp(0, 1, Frame.Slide));
        public Vector2 CurrentPosition => Position + Offset;

        public SparkParticle(SceneGame world, SpriteReference sprite, Vector2 pos, int time) : base(world)
        {
            Sprite = sprite;
            Position = pos;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += GetTimeMod();
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            float size = (float)LerpHelper.CubicIn(1, 0, Frame.Slide);
            float length = (float)LerpHelper.Linear(2, 1, Frame.Slide);
            float angle = Util.VectorToAngle(Vector2.Lerp(Vector2.Zero, Velocity, (float)VelocityLerp(0, 1, Frame.Slide)) - Vector2.Lerp(Vector2.Zero, Velocity, (float)VelocityLerp(0, 1, Frame.Slide - 0.01)));
            scene.DrawSpriteExt(Sprite, 0, CurrentPosition - Sprite.Middle, Sprite.Middle, angle, new Vector2(1, length) * size, SpriteEffects.None, Color, 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass;
        }
    }

    class CutterParticle : VisualEffect
    {
        SpriteReference Sprite;
        Vector2 Position;
        public float Angle;
        public float RotationStart, RotationEnd;
        public float SizeStart, SizeEnd;
        public LerpHelper.Delegate RotationLerp = LerpHelper.Linear;
        public LerpHelper.Delegate ScaleLerp = LerpHelper.Linear;
        public Color Color = Color.White;
        public DrawPass DrawPass = DrawPass.Effect;

        public Vector2 CurrentPosition => Position;

        public CutterParticle(SceneGame world, SpriteReference sprite, Vector2 pos, int time) : base(world)
        {
            Sprite = sprite;
            Position = pos;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += GetTimeMod();
            Angle += World.TimeMod * (float)RotationLerp(RotationStart, RotationEnd, Frame.Slide);
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            float size = (float)ScaleLerp(SizeStart, SizeEnd, Frame.Slide);
            scene.DrawSpriteExt(Sprite, 0, CurrentPosition - Sprite.Middle, Sprite.Middle, Angle, new Vector2(size), SpriteEffects.None, Color, 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass;
        }
    }

    class TrailParticle : VisualEffect
    {
        SpriteReference Sprite;
        int SubImage;
        Vector2 Position;
        public float Angle;
        public Color Color = Color.White;
        public DrawPass DrawPass = DrawPass.Effect;
        public LerpHelper.Delegate FadeLerp = LerpHelper.Linear;

        public TrailParticle(SceneGame world, SpriteReference sprite, Vector2 pos, int time) : base(world)
        {
            Sprite = sprite;
            SubImage = Random.Next(1000);
            Position = pos;
            Angle = Random.NextAngle();
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += GetTimeMod();
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var fade = (float)FadeLerp(1, 0, Frame.Slide);
            scene.DrawSpriteExt(Sprite, SubImage, Position - Sprite.Middle, Sprite.Middle, Angle, new Vector2(1), SpriteEffects.None, Color.WithAlpha(fade), 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass;
        }
    }

    class SmokeParticle : VisualEffect
    {
        SpriteReference Sprite;
        int SubImage;
        Vector2 Position;
        public float Angle;
        public float AngleSpeedStart;
        public float AngleSpeedEnd;
        public LerpHelper.Delegate AngleSpeedLerp = LerpHelper.Linear;
        public Vector2 StartVelocity;
        public Vector2 EndVelocity;
        public LerpHelper.Delegate StartVelocityLerp = LerpHelper.Linear;
        public LerpHelper.Delegate EndVelocityLerp = LerpHelper.Linear;
        public float Size = 1;
        public LerpHelper.Delegate SizeLerp = LerpHelper.QuadraticIn;
        public Color Color = Color.White;
        public DrawPass DrawPass = DrawPass.Effect;

        public float StartTime;
        public float EndTime;
        public float DissipateTime;
        public float FlickerTime = float.PositiveInfinity;

        private float StartSlide => Util.ReverseClamp(Frame.Slide, 0, StartTime);
        private float EndSlide => Util.ReverseClamp(Frame.Slide, 1 - EndTime, 1);

        private Vector2 OffsetStart => Vector2.Lerp(Vector2.Zero, StartVelocity, (float)StartVelocityLerp(0, 1, StartSlide));
        private Vector2 OffsetEnd => Vector2.Lerp(Vector2.Zero, EndVelocity, (float)EndVelocityLerp(0, 1, EndSlide));
        public Vector2 CurrentPosition => Position + OffsetStart + OffsetEnd;

        public SmokeParticle(SceneGame world, SpriteReference sprite, Vector2 pos, int time) : base(world)
        {
            Sprite = sprite;
            SubImage = Random.Next(1000);
            Position = pos;
            Angle = Random.NextAngle();
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += GetTimeMod();
            Angle += (float)AngleSpeedLerp(AngleSpeedStart, AngleSpeedEnd, Frame.Slide);
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            float dissipateSlide = Util.ReverseClamp(Frame.Slide, 1 - DissipateTime, 1);

            float size = (float)SizeLerp(Size, 0, dissipateSlide);
            float flicker = 1f;
            if (Frame.Time > FlickerTime)
                flicker = (Frame.Time % 4) / 4;
            if (Sprite == null)
                scene.SpriteBatch.Draw(scene.Pixel, CurrentPosition, null, Color.WithAlpha(flicker), Angle, Vector2.One, 1, SpriteEffects.None, 0);
            else
                scene.DrawSpriteExt(Sprite, SubImage, CurrentPosition - Sprite.Middle, Sprite.Middle, Angle, new Vector2(size), SpriteEffects.None, Color.WithAlpha(flicker), 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass;
        }
    }

    class SmokeParticleTimeless : VisualEffect
    {
        SpriteReference Sprite;
        int SubImage;
        Vector2 Position;
        public float Angle;
        public float AngleSpeedStart;
        public float AngleSpeedEnd;
        public LerpHelper.Delegate AngleSpeedLerp = LerpHelper.Linear;
        public Vector2 StartVelocity;
        public Vector2 EndVelocity;
        public LerpHelper.Delegate StartVelocityLerp = LerpHelper.Linear;
        public LerpHelper.Delegate EndVelocityLerp = LerpHelper.Linear;
        public float Size = 1;
        public LerpHelper.Delegate SizeLerp = LerpHelper.QuadraticIn;
        public Color Color = Color.White;
        public DrawPass DrawPass = DrawPass.Effect;

        public float StartTime;
        public float EndTime;
        public float DissipateTime;
        public float FlickerTime = float.PositiveInfinity;

        private float StartSlide => Util.ReverseClamp(Frame.Slide, 0, StartTime);
        private float EndSlide => Util.ReverseClamp(Frame.Slide, 1 - EndTime, 1);

        private Vector2 OffsetStart => Vector2.Lerp(Vector2.Zero, StartVelocity, (float)StartVelocityLerp(0, 1, StartSlide));
        private Vector2 OffsetEnd => Vector2.Lerp(Vector2.Zero, EndVelocity, (float)EndVelocityLerp(0, 1, EndSlide));
        public Vector2 CurrentPosition => Position + OffsetStart + OffsetEnd;

        public SmokeParticleTimeless(SceneGame world, SpriteReference sprite, Vector2 pos, int time) : base(world)
        {
            Sprite = sprite;
            SubImage = Random.Next(1000);
            Position = pos;
            Angle = Random.NextAngle();
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += 1;
            Angle += (float)AngleSpeedLerp(AngleSpeedStart, AngleSpeedEnd, Frame.Slide);
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            float dissipateSlide = Util.ReverseClamp(Frame.Slide, 1 - DissipateTime, 1);

            float size = (float)SizeLerp(Size, 0, dissipateSlide);
            float flicker = 1f;
            if (Frame.Time > FlickerTime)
                flicker = (Frame.Time % 4) / 4;
            if (Sprite == null)
                scene.SpriteBatch.Draw(scene.Pixel, CurrentPosition, null, Color.WithAlpha(flicker), Angle, Vector2.One, 1, SpriteEffects.None, 0);
            else
                scene.DrawSpriteExt(Sprite, SubImage, CurrentPosition - Sprite.Middle, Sprite.Middle, Angle, new Vector2(size), SpriteEffects.None, Color.WithAlpha(flicker), 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass;
        }
    }

    class ExplosionParticle : VisualEffect
    {
        SpriteReference Sprite;
        Vector2 Position;
        public float Angle;
        public LerpFloat AngleSpeed = new LerpFloat(0);
        public LerpFloat Size = new LerpFloat(1);
        public virtual Vector2 CurrentPosition => Position;

        public Color Color = Color.White;
        public DrawPass DrawPass = DrawPass.Effect;

        public ExplosionParticle(SceneGame world, SpriteReference sprite, Vector2 pos, int time) : base(world)
        {
            Sprite = sprite;
            Position = pos;
            Angle = Random.NextAngle();
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += GetTimeMod();
            Angle += GetTimeMod() * AngleSpeed;
            AngleSpeed.Update(GetTimeMod());
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            int subImage = scene.AnimationFrame(Sprite, Frame.Time, Frame.EndTime);
            float size = Size;
            if (Sprite == null)
                scene.SpriteBatch.Draw(scene.Pixel, CurrentPosition, null, Color, Angle, Vector2.One, 1, SpriteEffects.None, 0);
            else
                scene.DrawSpriteExt(Sprite, subImage, CurrentPosition - Sprite.Middle, Sprite.Middle, Angle, new Vector2(size), SpriteEffects.None, Color, 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass;
        }
    }

    class ExplosionParticleAnchored : ExplosionParticle
    {
        Func<Vector2> Anchor;

        public override Vector2 CurrentPosition => Anchor();

        public ExplosionParticleAnchored(SceneGame world, SpriteReference sprite, Func<Vector2> pos, int time) : base(world, sprite, Vector2.Zero, time)
        {
            Anchor = pos;
        }
    }

    class Strike : VisualEffect
    {
        Vector2 Start;
        Vector2 End;
        ISlider Slider;

        DrawPass DrawPass = DrawPass.EffectAdditive;
        Color Color = Color.White;

        public Strike(SceneGame world, Vector2 start, Vector2 end, ISlider slider) : base(world)
        {
            Start = start;
            End = end;
            Slider = slider;
        }

        public override void Update()
        {
            if (Slider.Slide >= 1)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var sprite = SpriteLoader.Instance.AddSprite("content/effect_bash");
            float angle = Util.VectorToAngle(End - Start);
            var pos = Vector2.Lerp(Start, End, (float)LerpHelper.QuadraticIn(0.3, 1, Slider.Slide));
            var middle = new Vector2(sprite.Width / 2, sprite.Height / 4);
            var size = (float)LerpHelper.CubicOut(0.3, 1, Slider.Slide);
            var lengthMod = (float)LerpHelper.CubicIn(1, 0.3, Slider.Slide);
            scene.DrawSpriteExt(sprite, 0, pos - middle, middle, angle, new Vector2(1, lengthMod) * size, SpriteEffects.None, Color, 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass;
        }
    }

    class Wave : VisualEffect
    {
        static SamplerState SamplerState = new SamplerState()
        {
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Clamp,
            Filter = TextureFilter.Point,
        };

        public SpriteReference WaveSprite;
        public int Precision = 20;
        public Vector2 Position;

        public float Radius;
        public float Thickness;
        public float StartRadius;
        public LerpHelper.Delegate InnerLerp;
        public LerpHelper.Delegate OuterLerp;
        public DrawPass DrawPass = DrawPass.Effect;
        public ColorMatrix Color = ColorMatrix.Identity;

        public float Start => (float)InnerLerp(StartRadius - Thickness, 1, Frame.Slide);
        public float End => (float)OuterLerp(StartRadius, 1, Frame.Slide);

        public Wave(SceneGame world, int time) : base(world)
        {
            Frame = new Slider(time);
        }

        public override void Update()
        {
            base.Update();
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            scene.DrawCircle(WaveSprite, SamplerState, Position, 100, 0, MathHelper.TwoPi, Radius, 0, Precision, Start, End, Color, scene.NonPremultiplied);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass;
        }
    }

    class BloodStain : VisualEffect
    {
        SpriteReference Sprite;
        int SubImage;
        Vector2 Position;
        float Scale;
        float Angle;

        public Vector2 WorldPosition => Vector2.Transform(Position, Matrix.Invert(World.WorldTransform));

        public BloodStain(SceneGame world, SpriteReference sprite, int subImage, Vector2 pos, float scale, float angle, float time) : base(world)
        {
            Sprite = sprite;
            SubImage = subImage;
            Position = Vector2.Transform(pos, world.WorldTransform);
            Scale = scale;
            Angle = angle;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            base.Update();
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            Color color = new Color(215, 63, 36);
            float slide = Frame.GetSubSlide(Frame.EndTime * 0.9f, Frame.EndTime);
            switch (pass)
            {
                case DrawPass.BloodAdditive:
                    color = Color.Lerp(Color.Gray, Color.Black, slide);
                    break;
                case DrawPass.BloodMultiply:
                    color = Color.Lerp(color, Color.White, (float)LerpHelper.ExponentialIn(0, 1, slide));
                    break;
            }
         
            scene.DrawSpriteExt(Sprite, SubImage, Position - Sprite.Middle, Sprite.Middle, Angle, new Vector2(Scale), SpriteEffects.None, color, 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            return Enumerable.Empty<DrawPass>();
        }
    }

    class AoEVisual : VisualEffect
    {
        public Vector2 AnchorStart;
        public Vector2 AnchorEnd;
        public MapTile Target;
        public IEnumerable<MapTile> Tiles;
        public Func<bool> ShouldDestroy = () => false;

        public AoEVisual(SceneGame world, Vector2 anchorStart) : base(world)
        {
            AnchorStart = anchorStart;
        }

        public void Set(MapTile target)
        {
            if (target != null)
            {
                AnchorEnd = target.VisualTarget;
                Target = target;
                Tiles = new[] { target };
            }
        }

        public void Set(MapTile target, IEnumerable<MapTile> area)
        {
            AnchorEnd = target?.VisualTarget ?? Vector2.Zero;
            Target = target;
            Tiles = area;
        }

        public static Func<float, Vector2> GetStraight(Vector2 start, Vector2 end)
        {
            return (slide) => Vector2.Lerp(start, end, slide);
        }

        public override void Update()
        {
            if (ShouldDestroy())
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            if (Tiles != null)
            {
                var cursor = SpriteLoader.Instance.AddSprite("content/effect_area");

                scene.PushSpriteBatch(blendState: BlendState.Additive);
                float flash = 0.5f + (float)Math.Sin(scene.Frame / 10f) * 0.5f;
                Color color = Color.Lerp(Color.IndianRed, Color.Orange, flash);
                foreach (var tile in Tiles)
                {
                    var pos = new Rectangle(16 * tile.X, 16 * tile.Y, 16, 16);
                    scene.SpriteBatch.Draw(cursor.Texture, pos, new Rectangle(scene.Frame, 0, 16, 16), color.WithAlpha(0.3f));
                }
                scene.PopSpriteBatch();
            }
            if (Target != null)
                DrawTarget(scene);
        }

        private void DrawTarget(SceneGame scene)
        {
            var line = SpriteLoader.Instance.AddSprite("content/line_dotted");
            var cursor = SpriteLoader.Instance.AddSprite("content/effect_area_target");

            var start = AnchorStart;
            var end = AnchorEnd;

            scene.PushSpriteBatch(blendState: BlendState.Additive);
            float flash = 0.5f + (float)Math.Sin(scene.Frame / 10f) * 0.5f;
            Color color = Color.Lerp(Color.IndianRed, Color.Orange, flash);

            var curve = GetStraight(start, end);
            scene.DrawBeamCurve(line, curve, 100, (slide) => 0.5f, 2, scene.Frame, 0.0f, 1.0f, ColorMatrix.Tint(color), BlendState.Additive);
            var pos = Target.GetVisualPosition();
            scene.DrawSprite(cursor, (int)(scene.Frame * 0.25f), pos, SpriteEffects.None, color, 0);
            scene.PopSpriteBatch();
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.UIWorld;
        }
    }

    class Slash : VisualEffect
    {
        static SamplerState SamplerState = new SamplerState()
        {
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Border,
            BorderColor = Color.Transparent,
            Filter = TextureFilter.Point,
        };

        Func<Vector2> Anchor;
        Func<float> AnchorAngle;
        float Start, End;
        float Radius;
        LerpFloat Thickness = new LerpFloat(0);
        ISlider Slider;

        public Slash(SceneGame world, Func<Vector2> anchor, Func<float> anchorAngle, float radius) : base(world)
        {
            Anchor = anchor;
            AnchorAngle = anchorAngle;
            Radius = radius;
        }

        public override void Update()
        {
            if (Slider.Slide >= 1)
                Destroy();
        }

        public void Perform(float start, float end, float thickness, LerpHelper.Delegate lerp, ISlider slider)
        {
            Start = start;
            End = end;
            Thickness.Set(thickness, 0, lerp, 1);
            Slider = slider;
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            float angle = AnchorAngle();
            float start = angle + Start;
            float end = angle + End;
            float thickness = Thickness.Get(Slider.Slide);
            float radiusMin = Radius - thickness / 2;
            float radiusMax = Radius + thickness / 2;
            scene.DrawCircle(null, SamplerState, Anchor(), 50, start, end, radiusMax, 0, 1, radiusMin / radiusMax, 1f, ColorMatrix.Identity, BlendState.Additive);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.EffectAdditive;
        }
    }

    class BigStar : VisualEffect
    {
        public SpriteReference Sprite;
        public Func<Vector2> Anchor;

        public LerpFloat Angle = new LerpFloat(0);
        public LerpFloat Scale = new LerpFloat(0);
        public LerpFloat Flicker = new LerpFloat(0);
        public LerpBoolean ShouldDestroy = new LerpBoolean(false);
        public float FlickerTime;
        public LerpHelper.Delegate FlickerLerp = LerpHelper.Linear;

        public Color Color = Color.White;
        public DrawPass DrawPass;

        public BigStar(SceneGame world, SpriteReference sprite, Func<Vector2> anchor) : base(world)
        {
            Sprite = sprite;
            Anchor = anchor;
        }

        public override void Update()
        {
            Angle.Update();
            Scale.Update();
            Flicker.Update();
            ShouldDestroy.Update();
            FlickerTime += Flicker;
            if (FlickerTime < 0 || FlickerTime > 1)
            {
                FlickerTime = Util.PositiveMod(FlickerTime, 1);
            }
            if (ShouldDestroy)
                Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            scene.DrawSpriteExt(Sprite, 0, Anchor() - Sprite.Middle, Sprite.Middle, Angle, new Vector2(Scale * (float)FlickerLerp(1, 0.5, FlickerTime)), SpriteEffects.None, Color, 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass;
        }
    }

    class Score : VisualEffect
    {
        Vector2 Position;
        int Amount;
        bool Big;
        public LerpFloat Flicker = new LerpFloat(0);
        public float FlickerTime;

        public Score(SceneGame world, Vector2 position, int amount, bool big, int time) : base(world)
        {
            Position = position;
            Amount = amount;
            Big = big;
            Frame = new Slider(time);
        }

        public void FromWorld()
        {
            Position = Vector2.Transform(Position, World.WorldTransform);
        }

        private Color GetTextColor(int index)
        {
            string text = Amount.ToString();
            float width = 8;
            float center = (float)LerpHelper.Quadratic(-width, text.Length + width, Frame.GetSubSlide(0, 30));
            float dist = Math.Abs(index - center);
            return Color.Lerp(Color.Gold, Color.Black, (float)LerpHelper.QuadraticIn(0, 1, MathHelper.Clamp(dist / width, 0, 1)));
        }

        public override void Update()
        {
            base.Update();
            if (Frame.Done)
                Destroy();
            Flicker.Update();
            FlickerTime += Flicker;
            if (FlickerTime < 0 || FlickerTime > 1)
            {
                FlickerTime = Util.PositiveMod(FlickerTime, 1);
            }
            if(Frame.Slide > 0.8f)
            {
                Flicker.Set(0.5f, LerpHelper.Linear, Frame.EndTime * 0.2f);
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var text = Amount.ToString();
            var textParameters = new TextParameters();
            textParameters.SetColor(index => Color.White, GetTextColor);
          
            if(Big)
            {
                textParameters.SetBold(true);
            }
            else
            {
                text = Game.ConvertToSmallPixelText(text);
            }

            scene.PushSpriteBatch(transform: Matrix.CreateTranslation(new Vector3(-Position,0)) * Matrix.CreateScale(new Vector3(2, 2, 0)) * Matrix.CreateTranslation(new Vector3(Position, 0)));
            var offset = new Vector2(0, -16 * (float)LerpHelper.QuadraticOut(0, 1, Frame.GetSubSlide(0, 20)));
            if (FlickerTime < 0.5f)
                scene.DrawText(text, Position + offset, Alignment.Center, textParameters);
            scene.PopSpriteBatch();
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.UI;
        }
    }

    class ScoreBlood : VisualEffect
    {
        Vector2 Start;
        Vector2 End;
        Vector2 OffsetStart;
        Vector2 OffsetEnd;
        int Score;
        float Angle;

        public SoundEffectInstance Sound;

        public ScoreBlood(SceneGame world, Vector2 start, Vector2 end, Vector2 offsetStart, Vector2 offsetEnd, int score, int time) : base(world)
        {
            Start = start;
            End = end;
            OffsetStart = offsetStart;
            OffsetEnd = offsetEnd;
            Score = score;
            Angle = Random.NextAngle();
            Frame = new Slider(time);
        }

        public override void Update()
        {
            base.Update();
            if(Frame.Done)
            {
                World.AddWorldScore(Score, End, Score > 500 ? ScoreType.Big : ScoreType.Small);
                Sound?.Play();
                Destroy();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var sprite = SpriteLoader.Instance.AddSprite("content/effect_blood_score");
            var offsetStart = OffsetStart * (float)LerpHelper.QuadraticIn(0, 1, Frame.Slide);
            var offsetEnd = OffsetEnd * (float)LerpHelper.QuadraticIn(1, 0, Frame.Slide);
            var position = Vector2.Lerp(Start + offsetStart,End + offsetEnd,(float)LerpHelper.Quadratic(0, 1, Frame.Slide));
            scene.DrawSpriteExt(sprite, scene.Frame / 5, position - sprite.Middle, sprite.Middle, Angle, Vector2.One, SpriteEffects.None, new Color(215, 63, 36), 0);
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Effect;
        }
    }
}
