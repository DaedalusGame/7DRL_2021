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
        SoundReference Splat;

        public ActionGib(ICurio origin, ICurio target, int score, SoundReference splat)
        {
            Origin = origin;
            Target = target;
            Score = score;
            Splat = splat;
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
                world.RunStats.Gibs += 1;
            Splat.Play(1.0f, Random.NextFloat(-0.5f, +0.5f), 0);
            Behavior.Apply(new BehaviorGib(Target));
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
        SoundReference Splat;

        public ActionCorpseGib(ICurio origin, ICurio target, int score, SoundReference splat, int particles = 30, float radius = 32)
        {
            Origin = origin;
            Target = target;
            Score = score;
            Radius = radius;
            Particles = particles;
            Splat = splat;
        }

        public bool Done => true;

        public void Run()
        {
            Random random = new Random();
            var world = Target.GetWorld();
            SkillUtil.CreateBloodCircle(world, Target.GetVisualTarget(), Particles, Radius, Random);
            if (Score > 0)
                world.AddWorldScore(Score, Target.GetVisualTarget(), ScoreType.Small);
            Splat.Play(1.0f, Random.NextFloat(-0.5f, +0.5f), 0);
            Behavior.Apply(new BehaviorGib(Target));
        }
    }

    class ActionRatGib : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        int Score;
        SoundReference Splat;
        SoundReference SoundRat = SoundLoader.AddSound("content/sound/rat_death.wav");

        public ActionRatGib(ICurio origin, ICurio target, int score, SoundReference splat)
        {
            Origin = origin;
            Target = target;
            Score = score;
            Splat = splat;
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
                SoundRat.Play(1, Random.NextFloat(-0.5f, +0.5f), 0);
                SkillUtil.CreateSpatter(world, Target.GetVisualTarget(), 5, Vector2.Zero, 1, Random);
                if (Score > 0)
                    world.AddWorldScore(Score, Target.GetVisualTarget(), ScoreType.Small);
                if (Origin == world.PlayerCurio)
                {
                    world.RunStats.Gibs += 1;
                    world.RunStats.RatsHunted += 1;
                }
            }
            Splat.Play(1.0f, Random.NextFloat(-0.5f, +0.5f), 0);
            Behavior.Apply(new BehaviorGib(Target));
        }
    }
}
