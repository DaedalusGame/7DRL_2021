using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionDestroyArmor : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        int Damage;
        int Score;

        public ActionDestroyArmor(ICurio origin, ICurio target, int damage, int score)
        {
            Origin = origin;
            Target = target;
            Damage = damage;
            Score = score;
        }

        public bool Done => true;

        public void Run()
        {
            var world = Target.GetWorld();
            var alive = Target.GetBehavior<BehaviorAlive>();
            var delta = Vector2.Normalize(Target.GetVisualTarget() - Origin.GetVisualTarget());

            alive.Armor = Math.Max(alive.Armor - Damage, 0);

            Vector2 center = Target.GetVisualTarget();
            for (int i = 0; i < 60; i++)
            {
                var emit = center + Util.AngleToVector(Random.NextAngle()) * Random.NextFloat(4, 16);
                var velocity = Vector2.Normalize(emit - (center - delta * 12));
                int time = Random.Next(10, 30);
                new SmokeParticle(world, SpriteLoader.Instance.AddSprite("content/effect_shard"), emit, (time * time) / 10)
                {
                    StartVelocity = velocity * Random.Next(50, 100),
                    EndVelocity = velocity * Random.Next(50, 100),
                    Size = 0.5f,
                    StartTime = 2f,
                    EndTime = 1,
                    StartVelocityLerp = LerpHelper.QuadraticIn,
                    EndVelocityLerp = LerpHelper.QuadraticOut,
                    FlickerTime = Random.Next(10, 50),
                    DrawPass = DrawPass.Effect,
                };
            }

            world.AddWorldScore(Score, center, ScoreType.Small);
        }
    }
}
