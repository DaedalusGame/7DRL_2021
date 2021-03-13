using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionHitVisual : IActionHasOrigin, IActionHasTarget
    {
        static Random Random = new Random();

        public ICurio Origin { get; set; }
        public ICurio Target { get; set; }

        public ActionHitVisual(ICurio origin, ICurio target)
        {
            Origin = origin;
            Target = target;
        }

        public bool Done => true;

        public void Run()
        {
            Random random = new Random();
            var world = Origin.GetWorld();
            var sword = Origin.GetBehavior<BehaviorSword>();
            Target.GetFlashHelper()?.AddFlash(ColorMatrix.Flat(Color.White), 20);
            Target.GetShakeHelper()?.AddShakeRandom(3, LerpHelper.QuadraticOut, 30);
            new HitStop(world, 0, 5);

            if (sword != null)
            {
                sword.HasBlood = true;
            }
        }
    }
}
