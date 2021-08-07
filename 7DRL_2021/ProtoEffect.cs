using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _7DRL_2021
{
    abstract class ProtoEffect
    {
        public static Random Random = new Random();

        public Scene Scene;

        public bool Timeless;

        protected ProtoEffect(Scene scene)
        {
            Scene = scene;
            Scene.ProtoEffects.AddLater(this);
        }

        public float GetTimeMod()
        {
            return Timeless ? 1 : Scene.TimeModCurrent;
        }

        public bool Destroyed { get; set; }

        public void Destroy()
        {
            Destroyed = true;
        }

        public abstract void Update();
    }

    abstract class ScreenFlash : ProtoEffect
    {
        public abstract ColorMatrix ScreenColor
        {
            get;
        }
        public Slider Frame;
        public bool Delete = true;

        public ScreenFlash(Scene scene, float time) : base(scene)
        {
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += 1;
            if (Frame.Done && Delete)
            {
                this.Destroy();
            }
        }
    }

    class ScreenBellWraiths : ScreenFlash
    {
        public override ColorMatrix ScreenColor => ColorMatrix.Lerp(ColorMatrix.Identity, ColorMatrix.Tint(Color.Red), 0.5f);
        BehaviorBellTower BellTower;
        float MinTime;
        float MaxTime;

        public ScreenBellWraiths(Scene scene, BehaviorBellTower bellTower, float minTime, float maxTime) : base(scene, 0)
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
                new ScreenGlitchFlash(Scene, slide => BehaviorBellTower.Params.WithIntensity((1 - slide) * 0.3f), 10);
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
        public LerpFloat Lerp;

        public override ColorMatrix ScreenColor => ColorMatrix.Lerp(ColorMatrix.Identity, ColorFunction(), Lerp);

        public ScreenFade(Scene world, Func<ColorMatrix> color, float start, bool delete) : base(world, 1)
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

    abstract class ScreenGlitch : ProtoEffect
    {
        public abstract GlitchParams Glitch
        {
            get;
        }

        public ScreenGlitch(Scene scene) : base(scene)
        {
        }
    }

    class ScreenGlitchFlash : ScreenGlitch
    {
        public Slider Frame;
        Func<float, GlitchParams> GlitchFunction;

        public override GlitchParams Glitch => GlitchFunction(Frame.Slide);

        public ScreenGlitchFlash(Scene scene, Func<float, GlitchParams> glitch, int time) : base(scene)
        {
            GlitchFunction = glitch;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += 1;
            if (Frame.Done)
            {
                this.Destroy();
            }
        }
    }

    class ScreenGlitchFade : ScreenGlitch
    {
        public Slider Frame;
        Func<float, GlitchParams> FadeInFunction;
        Func<float, GlitchParams> FadeOutFunction;
        int TimeIn;
        int TimeOut;

        public override GlitchParams Glitch => Frame.Time < TimeIn ? FadeInFunction(Frame.GetSubSlide(0, TimeIn)) : FadeOutFunction(Frame.GetSubSlide(TimeIn, TimeIn + TimeOut));

        public ScreenGlitchFade(Scene scene, Func<float, GlitchParams> fadeIn, Func<float, GlitchParams> fadeOut, int timeIn, int timeOut) : base(scene)
        {
            FadeInFunction = fadeIn;
            FadeOutFunction = fadeOut;
            TimeIn = timeIn;
            TimeOut = timeOut;
            Frame = new Slider(timeIn + timeOut);
        }

        public override void Update()
        {
            Frame += 1;
            if (Frame.Done)
            {
                this.Destroy();
            }
        }
    }

    abstract class ScreenShake : ProtoEffect
    {
        public Vector2 Offset;

        public ScreenShake(Scene scene) : base(scene)
        {
        }
    }

    class ScreenShakeRandom : ScreenShake
    {
        float Amount;
        LerpHelper.Delegate Lerp;
        Slider Frame;

        public ScreenShakeRandom(Scene scene, float amount, int time, LerpHelper.Delegate lerp) : base(scene)
        {
            Lerp = lerp;
            Amount = amount;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += 1;
            if (Frame.Done)
            {
                this.Destroy();
            }

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
        Slider Frame;

        public ScreenShakeJerk(Scene scene, Vector2 jerk, int time) : base(scene)
        {
            Jerk = jerk;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += 1;
            if (Frame.Done)
            {
                this.Destroy();
            }

            float amount = (1 - Frame.Slide);
            Offset = Jerk * amount;
        }
    }

    abstract class TimeWarp : ProtoEffect
    {
        public abstract float TimeMod { get; }

        public TimeWarp(Scene scene) : base(scene)
        {
        }
    }

    class TimeFade : TimeWarp
    {
        float TimeModStart;
        LerpHelper.Delegate Lerp;
        Slider Frame;

        public override float TimeMod => (float)Lerp(TimeModStart, 1, Frame.Slide);

        public TimeFade(Scene scene, float timeMod, LerpHelper.Delegate lerp, int time) : base(scene)
        {
            TimeModStart = timeMod;
            Lerp = lerp;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += 1;
            if (Frame.Done)
            {
                this.Destroy();
            }
        }
    }

    class HitStop : TimeWarp
    {
        float TimeModStop;
        Slider Frame;

        public override float TimeMod => TimeModStop;

        public HitStop(Scene scene, float timeMod, int time) : base(scene)
        {
            TimeModStop = timeMod;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += 1;
            if (Frame.Done)
            {
                this.Destroy();
            }
        }
    }

    abstract class ProtoEffectDrawable : ProtoEffect
    {
        protected ProtoEffectDrawable(Scene scene) : base(scene)
        {
        }

        public abstract void Draw(Scene scene, Vector2 pos);
    }


    class BigStar : ProtoEffectDrawable
    {
        public SpriteReference Sprite;

        public LerpFloat Angle = new LerpFloat(0);
        public LerpFloat Scale = new LerpFloat(0);
        public LerpFloat ScaleFlicker = new LerpFloat(0);
        public LerpFloat Flicker = new LerpFloat(0);
        public LerpBoolean ShouldDestroy = new LerpBoolean(false);
        public float ScaleFlickerTime;
        public float FlickerTime;
        public LerpHelper.Delegate FlickerLerp = LerpHelper.Linear;
        public float FlickerScale = 0.5f;

        public Color Color = Color.White;

        public BigStar(Scene world, SpriteReference sprite) : base(world)
        {
            Sprite = sprite;
        }

        public override void Update()
        {
            Angle.Update(GetTimeMod());
            Scale.Update(GetTimeMod());
            ScaleFlicker.Update(GetTimeMod());
            Flicker.Update(GetTimeMod());
            ShouldDestroy.Update(GetTimeMod());
            ScaleFlickerTime += ScaleFlicker;
            if (ScaleFlickerTime < 0 || ScaleFlickerTime > 1)
            {
                ScaleFlickerTime = Util.PositiveMod(ScaleFlickerTime, 1);
            }
            FlickerTime += Flicker;
            if (FlickerTime < 0 || FlickerTime > 1)
            {
                FlickerTime = Util.PositiveMod(FlickerTime, 1);
            }
            if (ShouldDestroy)
                Destroy();
        }

        public override void Draw(Scene scene, Vector2 pos)
        {
            if(FlickerTime < 0.5f)
                scene.DrawSpriteExt(Sprite, 0, pos - Sprite.Middle, Sprite.Middle, Angle, new Vector2(Scale * (float)FlickerLerp(1, FlickerScale, ScaleFlickerTime)), SpriteEffects.None, Color, 0);
        }
    }

    class Wave : ProtoEffectDrawable
    {
        static SamplerState SamplerState = new SamplerState()
        {
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Clamp,
            Filter = TextureFilter.Point,
        };

        public SpriteReference WaveSprite;
        public int Precision = 20;

        public float Radius;
        public float Thickness;
        public float StartRadius;
        public LerpHelper.Delegate InnerLerp;
        public LerpHelper.Delegate OuterLerp;
        public ColorMatrix Color = ColorMatrix.Identity;

        public float Start => (float)InnerLerp(StartRadius - Thickness, 1, Frame.Slide);
        public float End => (float)OuterLerp(StartRadius, 1, Frame.Slide);

        Slider Frame;

        public Wave(Scene scene, SpriteReference sprite, int time) : base(scene)
        {
            WaveSprite = sprite;
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += GetTimeMod();
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(Scene scene, Vector2 pos)
        {
            scene.DrawCircle(WaveSprite, SamplerState, pos, 100, 0, MathHelper.TwoPi, Radius, 0, Precision, Start, End, Color, scene.NonPremultiplied);
        }
    }

    class Explosion : ProtoEffectDrawable
    {
        SpriteReference Sprite;
        Vector2 Position;
        public float Angle;
        public LerpFloat AngleSpeed = new LerpFloat(0);
        public LerpFloat Size = new LerpFloat(1);

        public Color Color = Color.White;

        Slider Frame;

        public Explosion(Scene world, SpriteReference sprite, int time) : base(world)
        {
            Sprite = sprite;
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

        public override void Draw(Scene scene, Vector2 pos)
        {
            int subImage = scene.AnimationFrame(Sprite, Frame.Slide);
            float size = Size;
            scene.DrawSpriteExt(Sprite, subImage, pos - Sprite.Middle, Sprite.Middle, Angle, new Vector2(size), SpriteEffects.None, Color, 0);
        }
    }

    class SlashEffect : ProtoEffectDrawable
    {
        public float Width;
        public float Angle;
        public Color Color = Color.White;

        Slider Frame;

        public SlashEffect(Scene world, float width, int time) : base(world)
        {
            Width = width;
            Angle = Random.NextAngle();
            Frame = new Slider(time);
        }

        public override void Update()
        {
            Frame += GetTimeMod();
            if (Frame.Done)
                Destroy();
        }

        public override void Draw(Scene scene, Vector2 pos)
        {

            float width = (float)LerpHelper.QuarticOut(Width, 0, Frame.Slide);
            scene.SpriteBatch.Draw(scene.Pixel, pos, scene.Pixel.Bounds, Color, Angle, new Vector2(0.5f, 0.5f), new Vector2(width * 2, 10000), SpriteEffects.None, 0);
        }
    }
}
