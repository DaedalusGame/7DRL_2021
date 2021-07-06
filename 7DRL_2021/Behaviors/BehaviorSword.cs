using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorSword : Behavior, ITickable, IMoveTickable
    {
        ICurio Curio;
        public int Position;
        public float Angle => GetAngle(Position);
        public Func<float> VisualAngle = () => 0;
        public Func<float> VisualScale = () => 0;
        public bool HasBlood;
        public bool HasHeart;
        public List<ICurio> StabTargets = new List<ICurio>();

        public Slider SparkFrame = new Slider(0.5f);

        public BehaviorSword()
        {
        }

        public BehaviorSword(ICurio curio, int position)
        {
            Curio = curio;
            Position = position;
            SetPositionVisual(Position);
            SetScale(0);
        }

        public Vector2 GetTip()
        {
            var center = Curio.GetVisualPosition() + new Vector2(8, 8);
            var angle = Curio.GetVisualAngle();
            return center + Util.AngleToVector(angle + VisualAngle()) * (8 + 16 * VisualScale());
        }

        public Vector2 GetBlade(float bladeLength)
        {
            var center = Curio.GetVisualPosition() + new Vector2(8, 8);
            var angle = Curio.GetVisualAngle();
            return center + Util.AngleToVector(angle + VisualAngle()) * (8 + bladeLength * VisualScale());
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

        public void SetScale(float newScale)
        {
            VisualScale = () => newScale;
        }

        public void SetScale(float newScale, LerpHelper.Delegate lerp, ISlider slider)
        {
            var oldScale = VisualScale();
            VisualScale = () =>
            {
                float slide = MathHelper.Clamp(slider.Slide, 0, 1);
                return (float)lerp(oldScale, newScale, slide);
            };
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

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            Apply(new BehaviorSword(mapper.Map(Curio), Position), Curio);
        }

        public void Tick(SceneGame scene)
        {
            var active = Curio.GetActionHolder(ActionSlot.Active);
            if (StabTargets.Any())
                active.CurrentActions.RemoveAll(x => x is ActionKeepMoving);
            StabTargets.RemoveAll(x => x.Removed);
        }

        Random Random = new Random();

        public void MoveTick(Vector2 direction)
        {
            SparkFrame += Curio.GetWorld().TimeMod;
            if (SparkFrame.Done)
            {
                SparkFrame.Reset();

                if (Position == 0)
                    return;

                var tile = Curio.GetMainTile();
                var angle = Curio.GetAngle() + Position * MathHelper.PiOver4;
                var offset = Util.AngleToVector(angle).ToTileOffset();
                var neighbor = tile.GetNeighborOrNull(offset.X, offset.Y);
                if (neighbor != null && neighbor.IsSolid())
                {
                    Vector2 sparkDirection = direction;
                    if (Position < 0)
                        sparkDirection = sparkDirection.TurnLeft();
                    if (Position > 0)
                        sparkDirection = sparkDirection.TurnRight();
                    Vector2 tip = GetBlade(8);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 randomOffset = Util.AngleToVector(Random.NextAngle()) * 20;
                        new SparkParticle(Curio.GetWorld(), SpriteLoader.Instance.AddSprite("content/effect_cinder"), tip, 10)
                        {
                            Velocity = sparkDirection * Random.Next(24, 48) + randomOffset,
                            Color = Color.White,
                            Size = Random.NextFloat(),
                            VelocityLerp = LerpHelper.QuadraticIn,
                            DrawPass = DrawPass.EffectCreatureAdditive,
                        };
                    }
                }
            }
        }
    }
}
