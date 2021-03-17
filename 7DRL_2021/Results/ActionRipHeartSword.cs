using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionRipHeartSword : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        public static SoundReference Blood = SoundLoader.AddSound("content/sound/big_splat.wav");

        public ActionRipHeartSword(ICurio origin, ICurio target)
        {
            Origin = origin;
            Target = target;
        }

        public bool Done => true;

        public void Run()
        {
            Random random = new Random();
            var world = Origin.GetWorld();
            var alive = Target.GetBehavior<BehaviorAlive>();
            var sword = Origin.GetBehavior<BehaviorSword>();
            Target.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 20);
            Target.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.QuadraticOut, 30);

            if (!Target.IsHeartless())
            {
                alive.SetDamage(alive.HP);
                SkillUtil.CreateSpatter(world, Target.GetVisualTarget(), 5, Util.AngleToVector(sword.Angle) * 24, 2, Random);

                if (sword != null)
                {
                    sword.HasBlood = true;
                    sword.HasHeart = true;
                }
                if (Origin == world.PlayerCurio)
                    world.HeartsRipped += 1;
                Blood.Play(1, 0.5f, 0);
                Behavior.Apply(new BehaviorHeartless(Target));
            }
        }
    }
}
