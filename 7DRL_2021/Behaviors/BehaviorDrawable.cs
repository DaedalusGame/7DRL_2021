using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Drawables;

namespace _7DRL_2021.Behaviors
{
    class FlashHelper
    {
        class FlashEffect
        {
            public Slider Frame;
            public Func<float, ColorMatrix> FlashFunction;

            public FlashEffect(Func<float, ColorMatrix> flashFunction, int time)
            {
                FlashFunction = flashFunction;
                Frame = new Slider(time);
            }
        }

        List<FlashEffect> Effects = new List<FlashEffect>();
        public ColorMatrix ColorMatrix
        {
            get
            {
                ColorMatrix colorMatrix = ColorMatrix.Identity;
                foreach (var effect in Effects)
                {
                    colorMatrix *= effect.FlashFunction(effect.Frame.Slide);
                }
                return colorMatrix;
            }
        }

        public void AddFlash(Func<float, ColorMatrix> flashFunction, int time)
        {
            Effects.Add(new FlashEffect(flashFunction, time));
        }

        public void AddFlash(ColorMatrix color, int time)
        {
            AddFlash((slide) => color, time);
        }

        public void AddFlash(ColorMatrix color, LerpHelper.Delegate lerp, int time)
        {
            AddFlash((slide) => ColorMatrix.Lerp(color, ColorMatrix.Identity, (float)lerp(0, 1, slide)), time);
        }

        public void Update()
        {
            foreach (var effect in Effects)
            {
                effect.Frame += 1;
            }
            Effects.RemoveAll(effect => effect.Frame.Done);
        }
    }

    class ShakeHelper
    {
        static Random Random = new Random();

        class ShakeEffect
        {
            public Slider Frame;
            public Func<float, Vector2> ShakeFunction;

            public ShakeEffect(Func<float, Vector2> shakeFunction, int time)
            {
                ShakeFunction = shakeFunction;
                Frame = new Slider(time);
            }
        }

        List<ShakeEffect> Effects = new List<ShakeEffect>();
        public Vector2 Offset
        {
            get
            {
                Vector2 offset = Vector2.Zero;
                foreach (var effect in Effects)
                {
                    offset += effect.ShakeFunction(effect.Frame.Slide);
                }
                return offset;
            }
        }

        public void AddShake(Func<float, Vector2> shakeFunction, int time)
        {
            Effects.Add(new ShakeEffect(shakeFunction, time));
        }

        public void AddShake(Vector2 offset, int time)
        {
            AddShake((slide) => offset, time);
        }

        public void AddShake(Vector2 offset, LerpHelper.Delegate lerp, int time)
        {
            AddShake((slide) => Vector2.Lerp(offset, Vector2.Zero, (float)lerp(0, 1, slide)), time);
        }

        public void AddShakeRandom(float distance, LerpHelper.Delegate lerp, int time)
        {
            AddShake((slide) => Vector2.Lerp(distance * Util.AngleToVector(Random.NextAngle()), Vector2.Zero, (float)lerp(0, 1, slide)), time);
        }

        public void Update()
        {
            foreach (var effect in Effects)
            {
                effect.Frame += 1;
            }
            Effects.RemoveAll(effect => effect.Frame.Done);
        }
    }

    class BehaviorDrawable : Behavior, IDrawableCurio, ITickable, IColored, IOffseted
    {
        public ICurio Curio;
        public Drawable Drawable;
        public double Priority;
        public FlashHelper FlashHelper = new FlashHelper();
        public ShakeHelper ShakeHelper = new ShakeHelper();
        public Dictionary<object, bool> FadeOut = new Dictionary<object, bool>();

        public ICurio DrawCurio => Curio;
        public double DrawOrder => 0;

        public BehaviorDrawable()
        {
        }

        public BehaviorDrawable(ICurio curio, Drawable drawable, double priority)
        {
            Curio = curio;
            Drawable = drawable;
            Priority = priority;
        }

        public void SetHidden(object key, bool flag)
        {
            FadeOut[key] = flag;
        }

        public void Reveal()
        {
            FadeOut.Clear();
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorDrawable(curio, Drawable, Priority), Curio);
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            var tile = Curio.GetMainTile();
            return tile.GetMap() == scene.Map && scene.IsWithinCamera(cameraPosition, tile.VisualTarget);
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            return Drawable.GetDrawPasses();
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            Drawable.Draw(Curio, scene, pass);
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            Drawable.DrawIcon(Curio, scene, pos);
        }

        public void Tick(SceneGame scene)
        {
            FlashHelper.Update();
            ShakeHelper.Update();
        }

        public ColorMatrix GetColor()
        {
            ColorMatrix color = FlashHelper.ColorMatrix;
            if (FadeOut.Values.Any(x => x))
                color *= ColorMatrix.Transparent;
            return color;
        }

        public double GetColorPriority()
        {
            return 10;
        }

        public Vector2 GetOffset()
        {
            return ShakeHelper.Offset;
        }

        public double GetOffsetPriority()
        {
            return 10;
        }
    }
}
