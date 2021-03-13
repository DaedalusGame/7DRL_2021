using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionRipHeartGrapple : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        public ActionRipHeartGrapple(ICurio origin, ICurio target)
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
            var grapple = Origin.GetBehavior<BehaviorGrapplingHook>();
            Target.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 20);
            Target.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.QuadraticOut, 30);

            if (!Target.IsHeartless())
            {
                var delta = Vector2.Normalize(Origin.GetVisualTarget() - Target.GetVisualTarget());
                alive.SetDamage(alive.HP);
                SkillUtil.CreateSpatter(world, Target.GetVisualTarget(), 5, delta * 24, 2, Random);

                if (grapple != null)
                {
                    grapple.HasHeart = true;
                }
                if (Origin == world.PlayerCurio)
                    world.HeartsRipped += 1;
                Behavior.Apply(new BehaviorHeartless(Target));
            }
        }
    }
}
