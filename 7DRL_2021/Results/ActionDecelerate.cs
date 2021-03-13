using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionDecelerate : IActionHasOrigin, ITickable
    {
        public ICurio Origin { get; set; }
        public int Momentum;
        private Slider Frame;

        public bool Done => Frame.Done;

        public ActionDecelerate(ICurio origin, int momentum, float time)
        {
            Origin = origin;
            Momentum = momentum;
            Frame = new Slider(time);
        }

        public void Run()
        {
            var player = Origin.GetBehavior<BehaviorPlayer>();
            if(player != null)
            {
                player.Momentum.Amount = Math.Max(0, player.Momentum.Amount + Momentum);
            }
        }

        public void Tick(SceneGame scene)
        {
            Frame += scene.TimeMod;
        }
    }
}
