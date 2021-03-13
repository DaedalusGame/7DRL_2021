using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorMace : Behavior, ITickable
    {
        public ICurio Curio;
        public Slider Upswing = new Slider(1, 1);
        public Slider MaceReturn = new Slider(1, 1);
        public Vector2 MacePosition;
        public float UpswingAngle;

        public BehaviorMace()
        {
        }

        public BehaviorMace(ICurio curio)
        {
            Curio = curio;
        }

        public void Mace(Vector2 target, float time)
        {
            MacePosition = target;
            MaceReturn = new Slider(time);
        }

        public bool IsInArea(ICurio target)
        {
            var delta = target.GetVisualTarget() - Curio.GetVisualTarget();
            var distance = delta.Length();
            return distance < 48f;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorMace(curio));
        }

        public void Tick(SceneGame scene)
        {
            if (Curio.IsAlive())
            {
                Upswing += scene.TimeMod;
                UpswingAngle += scene.TimeMod * (float)LerpHelper.QuadraticOut(MathHelper.TwoPi * 0.01, MathHelper.TwoPi * 0.2, Upswing.Slide);
            }
            else
                Upswing.Time = Upswing.EndTime;
            if (!MaceReturn.Done)
            {
                MaceReturn += scene.TimeMod;
            }
        }
    }
}
