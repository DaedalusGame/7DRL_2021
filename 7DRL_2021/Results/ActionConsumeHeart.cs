using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    abstract class ActionConsumeHeart : IActionHasOrigin
    {
        public ICurio Origin { get; set; }
        public int Score;
        public int Heal;

        public bool Done => true;

        protected abstract bool HasHeart { get; }

        public ActionConsumeHeart(ICurio origin, int score, int heal)
        {
            Origin = origin;
            Score = score;
            Heal = heal;
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var sword = Origin.GetBehavior<BehaviorSword>();
            var alive = Origin.GetBehavior<BehaviorAlive>();
            if (HasHeart && alive != null)
            {
                alive.HealDamage(Heal);
                world.AddWorldScore(Score, Origin.GetVisualTarget(), ScoreType.Small);
                RemoveHeart();
                if (Origin == world.PlayerCurio)
                    world.HeartsEaten += 1;
            }
        }

        protected abstract void RemoveHeart();
    }

    class ActionConsumeHeartSword : ActionConsumeHeart
    {
        public ActionConsumeHeartSword(ICurio origin, int score, int heal) : base(origin, score, heal)
        {
        }

        protected override bool HasHeart => Origin.GetBehavior<BehaviorSword>()?.HasHeart ?? false;

        protected override void RemoveHeart()
        {
            var sword = Origin.GetBehavior<BehaviorSword>();
            sword.HasHeart = false;
        }
    }

    class ActionConsumeHeartGrapple : ActionConsumeHeart
    {
        public ActionConsumeHeartGrapple(ICurio origin, int score, int heal) : base(origin, score, heal)
        {
        }

        protected override bool HasHeart => Origin.GetBehavior<BehaviorGrapplingHook>()?.HasHeart ?? false;

        protected override void RemoveHeart()
        {
            var sword = Origin.GetBehavior<BehaviorGrapplingHook>();
            sword.HasHeart = false;
        }
    }
}
