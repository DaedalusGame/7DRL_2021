using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionDamage : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        public int Damage;
        public int Score;
        public int Blood;

        public SoundReference HitSound;
        public SoundReference DeathSound = SoundLoader.AddSound("content/sound/kill.wav");

        public ActionDamage(ICurio origin, ICurio target, int damage, int score, int blood, SoundReference hitSound)
        {
            Origin = origin;
            Target = target;
            Damage = damage;
            Score = score;
            Blood = blood;
            HitSound = hitSound;
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
            new HitStop(world, 0, 5);
            HitSound.Play(1f, Random.NextFloat(-0.3f, +0.3f), 0);
            SkillUtil.CreateSpatter(world, Target.GetVisualTarget(), Blood, Vector2.Zero, 1, Random);
            if(Score > 0)
                world.AddWorldScore(Score, Target.GetVisualTarget(), ScoreType.Small);

            if(Blood > 0)
                SkillUtil.CreateBloodCircle(world, Target.GetVisualTarget(), 30, 32, Random);

            alive.TakeDamage(Damage);
            
            if(alive.CurrentDead)
            {
                //DeathSound.Play(1, 0, 0);
                if (Origin == world.PlayerCurio)
                    world.Kills += 1;
            }

            if (sword != null && Blood > 0)
            {
                sword.HasBlood = true;
            }
        }
    }

    class ActionPlayerDamage : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        int Damage;

        public SoundReference HitSound;

        public ActionPlayerDamage(ICurio origin, ICurio target, int damage, SoundReference hitSound)
        {
            Origin = origin;
            Target = target;
            Damage = damage;
            HitSound = hitSound;
        }

        public bool Done => true;

        public void Run()
        {
            Random random = new Random();
            var world = Origin.GetWorld();
            var alive = Target.GetBehavior<BehaviorAlive>();
            Target.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 20);
            Target.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.QuadraticOut, 30);
            new HitStop(world, 0, 5);
            new ScreenShakeRandom(world, 10, 10, LerpHelper.QuadraticOut);
            new ScreenFlashSimple(world, ColorMatrix.Greyscale() * ColorMatrix.Tint(new Color(215, 63, 36)), LerpHelper.QuadraticOut, 10);
            HitSound.Play(1, Random.NextFloat(-0.5f, +0.5f), 0);

            alive.TakeDamage(Damage);
        }
    }
}
