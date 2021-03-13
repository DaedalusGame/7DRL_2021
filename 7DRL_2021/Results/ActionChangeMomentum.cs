using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionChangeMomentum : IActionHasOrigin
    {
        public ICurio Origin { get; set; }
        public int Momentum;
        public bool Done => true;

        public ActionChangeMomentum(ICurio origin, int momentum)
        {
            Origin = origin;
            Momentum = momentum;
        }

        public void Run()
        {
            var player = Origin.GetBehavior<BehaviorPlayer>();
            if (player != null)
            {
                player.Momentum.Amount = Math.Max(0, player.Momentum.Amount + Momentum);
            }
        }
    }
}
