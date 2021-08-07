using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public float MaxDistance;

        static SoundReference SoundSwish = SoundLoader.AddSound("content/sound/swish.wav");

        public BehaviorMace()
        {
        }

        public BehaviorMace(ICurio curio, float maxDistance)
        {
            Curio = curio;
            MaxDistance = maxDistance;
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
            return distance < MaxDistance;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorMace(curio, MaxDistance), Curio);
        }

        public void Tick(SceneGame scene)
        {
            if (Curio.IsAlive() && !Upswing.Done)
            {
                Upswing += scene.TimeModCurrent;
                var lastAngle = UpswingAngle;
                UpswingAngle += scene.TimeModCurrent * (float)LerpHelper.QuadraticOut(MathHelper.TwoPi * 0.01, MathHelper.TwoPi * 0.2, Upswing.Slide);
                var deltaSwing = Math.Abs(Math.Floor(UpswingAngle / MathHelper.Pi) - Math.Floor(lastAngle / MathHelper.Pi));
                if (deltaSwing >= 1)
                {
                    SoundSwish.Play(1, Upswing.Slide - 0.5f, 0);
                }
            }
            //else
            //    Upswing.Time = Upswing.EndTime;
            if (!MaceReturn.Done)
            {
                MaceReturn += scene.TimeModCurrent;
            }
        }

        public virtual void DrawMace(SceneGame scene, Vector2 pos, Vector2 offset, int chains)
        {
            var mace = SpriteLoader.Instance.AddSprite("content/mace");
            var chain = SpriteLoader.Instance.AddSprite("content/mace_chain");

            for (int i = 0; i < chains; i++)
            {
                scene.DrawSpriteExt(chain, 0, pos + offset * ((float)i / chains) - chain.Middle, chain.Middle, 0, new Vector2(1), SpriteEffects.None, Color.White, 0);
            }
            scene.DrawSpriteExt(mace, 0, pos + offset - mace.Middle, mace.Middle, 0, new Vector2(1), SpriteEffects.None, Color.White, 0);
        }
    }

    class BehaviorMaceGore : BehaviorMace
    {
        public BehaviorMaceGore() : base()
        {
        }

        public BehaviorMaceGore(ICurio curio, float maxDistance) : base(curio, maxDistance)
        {
        }      

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorMaceGore(curio, MaxDistance), Curio);
        }

        public override void DrawMace(SceneGame scene, Vector2 pos, Vector2 offset, int chains)
        {
            var mace = SpriteLoader.Instance.AddSprite("content/mace_gore");
            var chain = SpriteLoader.Instance.AddSprite("content/mace_chain");
            var angle = Util.VectorToAngle(offset);

            for (int i = 0; i < chains; i++)
            {
                scene.DrawSpriteExt(chain, 0, pos + offset * ((float)i / chains) - chain.Middle, chain.Middle, 0, new Vector2(1), SpriteEffects.None, Color.White, 0);
            }
            scene.DrawSpriteExt(mace, 0, pos + offset - mace.Middle, mace.Middle, angle, new Vector2(1), SpriteEffects.None, Color.White, 0);
        }
    }
}
