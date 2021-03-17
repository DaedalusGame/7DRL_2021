using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionParrySword : IWeaponHit
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        public bool Done => true;

        public static SoundReference Parry = SoundLoader.AddSound("content/sound/parry.wav");

        public ActionParrySword(ICurio origin, ICurio target)
        {
            Origin = origin;
            Target = target;
        }

        public void Run()
        {
            var sword = Origin.GetBehavior<BehaviorSword>();
            var world = Target.GetWorld();

            var position = Enumerable.Range(-3, 7).Except(new[] { sword.Position }).Shuffle(Random).First();
            sword.OrientTo(position, LerpHelper.ExponentialIn, new SliderScene(world, 5));

            Vector2 center = Origin.GetVisualTarget();
            for (int i = 0; i < 15; i++)
            {
                var emit = center + Util.AngleToVector(Random.NextAngle()) * Random.NextFloat(4, 16);
                var velocity = Vector2.Normalize(emit - center);
                int time = Random.Next(10, 30);
                new SmokeParticle(world, SpriteLoader.Instance.AddSprite("content/effect_cosmic"), emit, (time * time) / 10)
                {
                    StartVelocity = velocity * Random.Next(50, 100),
                    EndVelocity = velocity * Random.Next(50, 100),
                    Size = 0.5f,
                    StartTime = 2f,
                    EndTime = 1,
                    StartVelocityLerp = LerpHelper.QuadraticIn,
                    EndVelocityLerp = LerpHelper.QuadraticOut,
                    FlickerTime = Random.Next(10, 50),
                    DrawPass = DrawPass.EffectAdditive,
                };
            }
            Parry.Play(0.5f, Random.NextFloat(-0.5f, +0.5f), 0.0f);
        }
    }
}
