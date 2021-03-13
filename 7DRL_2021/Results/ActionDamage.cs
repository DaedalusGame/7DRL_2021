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

        int Damage;
        int Score;

        public ActionDamage(ICurio origin, ICurio target, int damage, int score)
        {
            Origin = origin;
            Target = target;
            Damage = damage;
            Score = score;
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
            SkillUtil.CreateSpatter(world, Target.GetVisualTarget(), 3, Vector2.Zero, 1, Random);
            if(Score > 0)
                world.AddWorldScore(Score, Target.GetVisualTarget(), ScoreType.Small);

            SkillUtil.CreateBloodCircle(world, Target.GetVisualTarget(), 30, 32, Random);

            alive.TakeDamage(Damage);
            if (alive.CurrentDead && Origin == world.PlayerCurio)
                world.Kills += 1;

            if (sword != null)
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

        public ActionPlayerDamage(ICurio origin, ICurio target, int damage)
        {
            Origin = origin;
            Target = target;
            Damage = damage;
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

            alive.TakeDamage(Damage);
        }
    }
}
