using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionConsumeBlood : IActionHasOrigin
    {
        public ICurio Origin { get; set; }
        public int Heal;

        public bool Done => true;

        protected bool HasBlood => Origin.GetBehavior<BehaviorSword>().HasBlood;

        public static SoundReference Eat = SoundLoader.AddSound("content/sound/eat.wav");

        public ActionConsumeBlood(ICurio origin, int heal)
        {
            Origin = origin;
            Heal = heal;
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var sword = Origin.GetBehavior<BehaviorSword>();
            var alive = Origin.GetBehavior<BehaviorAlive>();
            if (HasBlood && alive != null)
            {
                alive.HealDamage(Heal);
                sword.HasBlood = false;
                Origin.GetFlashHelper().AddFlash(ColorMatrix.Translate(Color.Lime), 4);
                Eat.Play(1, 0, 0);
            }
        }
    }
}
