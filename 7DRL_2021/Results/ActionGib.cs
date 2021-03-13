using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionGib : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        int Score;

        public ActionGib(ICurio origin, ICurio target, int score)
        {
            Origin = origin;
            Target = target;
            Score = score;
        }

        public bool Done => true;

        public void Run()
        {
            Random random = new Random();
            var world = Target.GetWorld();
            SkillUtil.CreateBloodCircle(world, Target.GetVisualTarget(), 60, 64, Random);
            SkillUtil.CreateSpatter(world, Target.GetVisualTarget(), 5, Vector2.Zero, 1, Random);
            if (Score > 0)
                world.AddWorldScore(Score, Target.GetVisualTarget(), ScoreType.Small);
            if (Origin == world.PlayerCurio)
                world.Gibs += 1;
            Target.Destroy();
        }
    }

    class ActionCorpseGib : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        int Score;
        float Radius;
        int Particles;

        public ActionCorpseGib(ICurio origin, ICurio target, int score, int particles = 30, float radius = 32)
        {
            Origin = origin;
            Target = target;
            Score = score;
            Radius = radius;
            Particles = particles;
        }

        public bool Done => true;

        public void Run()
        {
            Random random = new Random();
            var world = Target.GetWorld();
            SkillUtil.CreateBloodCircle(world, Target.GetVisualTarget(), Particles, Radius, Random);
            if (Score > 0)
                world.AddWorldScore(Score, Target.GetVisualTarget(), ScoreType.Small);
            Target.Destroy();
        }
    }

    class ActionRatGib : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        int Score;

        public ActionRatGib(ICurio origin, ICurio target, int score)
        {
            Origin = origin;
            Target = target;
            Score = score;
        }

        public bool Done => true;

        public void Run()
        {
            Random random = new Random();
            var world = Target.GetWorld();
            var lastSeen = Target.GetBehavior<BehaviorLastSeen>();
            var canBeSeen = lastSeen.CanSee(Origin);
            SkillUtil.CreateBloodCircle(world, Target.GetVisualTarget(), 60, 64, Random);
            if (canBeSeen)
            {
                SkillUtil.CreateSpatter(world, Target.GetVisualTarget(), 5, Vector2.Zero, 1, Random);
                if (Score > 0)
                    world.AddWorldScore(Score, Target.GetVisualTarget(), ScoreType.Small);
                if (Origin == world.PlayerCurio)
                    world.Gibs += 1;
            }
            Target.Destroy();
        }
    }
}
