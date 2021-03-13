using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorGrapplingHook : Behavior, IDrawable
    {
        ICurio Curio;
        public int Position;
        public float Angle => GetAngle(Position);

        public double DrawOrder => 0;

        public Func<float> VisualAngle = () => 0;
        public Func<Vector2> VisualTip = () => Vector2.Zero;
        public Func<float> VisualAmplitude = () => 0;
        public bool HasHeart;
        public Point? GripDirection;

        public bool ShouldRender => GripDirection.HasValue || VisualTip() != GetBase();

        public BehaviorGrapplingHook()
        {
        }

        public BehaviorGrapplingHook(ICurio curio, int position)
        {
            Curio = curio;
            Position = position;
            SetPositionVisual(Position);
            ReelIn();
        }

        public Vector2 GetBase()
        {
            var center = Curio.GetVisualPosition() + new Vector2(8, 8);
            var angle = Curio.GetVisualAngle();
            return center + Util.AngleToVector(angle + VisualAngle()) * 8;
        }

        public Vector2 GetTip()
        {
            return VisualTip();
        }

        public static float GetAngle(int position)
        {
            return position * MathHelper.PiOver4;
        }

        public void SetPosition(int position)
        {
            Position = MathHelper.Clamp(position, -3, 3);
        }

        public void SetPositionVisual(int position)
        {
            SetAngleVisual(GetAngle(position));
        }

        public void SetAngleVisual(float newAngle)
        {
            VisualAngle = () => newAngle;
        }

        public void OrientVisual(float newAngle, LerpHelper.Delegate lerp, ISlider slider)
        {
            var oldAngle = VisualAngle();
            VisualAngle = () =>
            {
                float slide = MathHelper.Clamp(slider.Slide, 0, 1);
                return (float)lerp(oldAngle, newAngle, slide);
            };
        }

        public void OrientTo(int position)
        {
            OrientTo(position, null, null);
        }

        public void OrientTo(int position, LerpHelper.Delegate lerp, ISlider slider)
        {
            SetPosition(position);
            var angle = GetAngle(Position);
            if (slider == null)
                SetAngleVisual(angle);
            else
                OrientVisual(angle, lerp, slider);
        }

        public void Connect(Func<Vector2> position)
        {
            VisualTip = position;
        }

        public void ReelIn()
        {
            VisualTip = GetBase;
        }

        public void ReelIn(Vector2 position, LerpHelper.Delegate lerp, ISlider slider)
        {
            VisualTip = () =>
            {
                float slide = MathHelper.Clamp(slider.Slide, 0, 1);
                return Vector2.Lerp(position, GetBase(), (float)lerp(0, 1, slide));
            };
        }

        public void ReelTowards(Vector2 position, ISlider slider)
        {
            VisualTip = () =>
            {
                if (slider.Slide >= 1)
                    return GetBase();
                return position;
            };
        }

        public void Straight()
        {
            VisualAmplitude = () => 0;
        }

        public void Wave(float start, float end, LerpHelper.Delegate lerp, ISlider slider)
        {
            VisualAmplitude = () =>
            {
                float slide = MathHelper.Clamp(slider.Slide, 0, 1);
                return (float)lerp(start, end, slide);
            };
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            Apply(new BehaviorGrapplingHook(mapper.Map(Curio), Position));
        }

        public bool ShouldDraw(SceneGame scene)
        {
            return scene.Map == Curio.GetMap();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.EffectLow;
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var heart = SpriteLoader.Instance.AddSprite("content/heart");
            var basePos = GetBase();
            var tipPos = GetTip();

            scene.FlushSpriteBatch();
            scene.DrawGrappleLine(basePos, tipPos, VisualAmplitude(), scene.Frame * 0.1f, 8, 100, LerpHelper.QuarticOut, new Color(255, 128, 128), scene.NonPremultiplied);
            if (HasHeart)
                scene.DrawSpriteExt(heart, 0, tipPos - heart.Middle, heart.Middle, 0, SpriteEffects.None, 0);
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }
    }
}
