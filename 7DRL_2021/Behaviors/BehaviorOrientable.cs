using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorOrientable : Behavior
    {
        public ICurio Curio;
        public float Angle;
        public Func<float> VisualAngle = () => 0;

        public BehaviorOrientable()
        {
        }

        public BehaviorOrientable(ICurio curio, float angle)
        {
            Curio = curio;
            Angle = angle;
        }

        public float SnapAngle(float angle)
        {
            return angle;
        }

        public void SetAngle(float angle)
        {
            Angle = angle;
        }

        public void OrientVisual(float newAngle, LerpHelper.Delegate lerp, ISlider slider)
        {
            var oldAngle = VisualAngle();
            VisualAngle = () =>
            {
                float slide = MathHelper.Clamp(slider.Slide, 0, 1);
                return Util.AngleLerp(oldAngle, newAngle, (float)lerp(0, 1, slide));
            };
        }

        public void TurnVisual(float newAngle, LerpHelper.Delegate lerp, ISlider slider)
        {
            var oldAngle = VisualAngle();
            VisualAngle = () =>
            {
                float slide = MathHelper.Clamp(slider.Slide, 0, 1);
                return (float)lerp(oldAngle, newAngle, slide);
            };
        }

        public void SetAngleVisual(float newAngle)
        {
            VisualAngle = () => newAngle;
        }

        public void OrientTo(float angle)
        {
            OrientTo(angle, null, null);
        }

        public void OrientTo(float angle, LerpHelper.Delegate lerp, ISlider slider)
        {
            SetAngle(angle);
            if (slider == null)
                SetAngleVisual(angle);
            else
                OrientVisual(angle, lerp, slider);
        }

        public void TurnTo(float angle, LerpHelper.Delegate lerp, ISlider slider)
        {
            SetAngle(angle);
            if (slider == null)
                SetAngleVisual(angle);
            else
                TurnVisual(angle, lerp, slider);
        }

        public void Turn(float addAngle, LerpHelper.Delegate lerp, ISlider slider)
        {
            TurnTo(Angle + addAngle, lerp, slider);
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            Apply(new BehaviorOrientable(mapper.Map(Curio), Angle));
        }

        public override string ToString()
        {
            return $"Orientable {MathHelper.ToDegrees(Angle)}°";
        }
    }
}
