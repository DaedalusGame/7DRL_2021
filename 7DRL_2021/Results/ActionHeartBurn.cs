using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionHeartBurn : IActionHasOrigin, IActionHasTarget, ITickable
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        public int Score;
        public Slider ExplosionTime = new Slider(10);
        public Slider Time = new Slider(10);

        public SoundReference DeathSound = SoundLoader.AddSound("content/sound/kill.wav");

        public ActionHeartBurn(ICurio origin, ICurio target, int score)
        {
            Origin = origin;
            Target = target;
            Score = score;
        }

        public bool Done => Time.Done;

        public void Run()
        {
            Random random = new Random();
            var world = Origin.GetWorld();
            var alive = Target.GetBehavior<BehaviorAlive>();
            var sword = Origin.GetBehavior<BehaviorSword>();
            var bloodfire = Origin.GetBehavior<BehaviorSkillBloodfireBlade>();

            bloodfire.Extinguish();

            new TimeFade(world, 0.05f, LerpHelper.ExponentialIn, 40);

            Target.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 5);
            Target.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.QuadraticOut, 5);
            new HitStop(world, 0, 5);

            alive.SetDamage(alive.HP);

            Target.GetFlashHelper()?.AddFlash(ColorMatrix.Translate(new Color(255, 64, 16)), LerpHelper.Invert(LerpHelper.QuadraticOut), (int)ExplosionTime.EndTime, false);
            Target.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.Invert(LerpHelper.QuadraticOut), (int)ExplosionTime.EndTime, false);

            if (alive.CurrentDead)
            {
                if (Origin == world.PlayerCurio)
                    world.Kills += 1;
            }

            if (sword != null)
            {
                sword.HasBlood = false;
            }
        }

        public void Tick(SceneGame scene)
        {
            var exploded = ExplosionTime.Done;
            var active = Origin.GetActionHolder(ActionSlot.Active);
            var passive = Origin.GetActionHolder(ActionSlot.Passive);

            ExplosionTime += scene.TimeModCurrent;

            if(ExplosionTime.Done)
            {
                if (!exploded)
                {
                    SkillUtil.CreateSpatter(scene, Target.GetVisualTarget(), 7, Vector2.Zero, 2, Random);

                    new TimeFade(scene, 0.05f, LerpHelper.ExponentialIn, 40);

                    for (float angle = 0; angle <= MathHelper.TwoPi; angle += MathHelper.TwoPi * 0.02f)
                    {
                        var particle = new ExplosionParticle(scene, SpriteLoader.Instance.AddSprite("content/effect_explosion"), Target.GetVisualTarget() + Util.AngleToVector(angle) * 8, Random.Next(5, 15))
                        {
                            Angle = angle + Random.NextFloat(-0.3f, +0.3f),
                            Color = Color.White,
                            DrawPass = DrawPass.EffectAdditive,
                        };
                        particle.Size.Set(Random.NextFloat(1.0f, 1.5f));
                    }

                    var actions = new List<ActionWrapper>();
                    actions.Add(new ActionGib(Origin, Target, Score, SoundLoader.AddSound("content/sound/big_splat.wav")).InSlot(ActionSlot.Active));
                    actions.Apply(Target);

                    var actionsOrigin = new List<ActionWrapper>()
                    {
                        new ActionKeepMoving(Origin).InSlot(ActionSlot.Active),
                    };
                    actionsOrigin.Apply(Origin);
                }

                Time += scene.TimeModCurrent;
            }
            else
            {
                active.CurrentActions.RemoveAll(x => x is ActionKeepMoving);
            }
        }
    }
}
