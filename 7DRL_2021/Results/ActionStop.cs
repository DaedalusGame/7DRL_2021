using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionStop : IActionHasOrigin
    {
        public bool Done => true;

        public ICurio Origin
        {
            get;
            set;
        }

        public ActionStop(ICurio origin)
        {
            Origin = origin;
        }

        public void Run()
        {
            var player = Origin.GetBehavior<BehaviorPlayer>();
            if (player != null)
                player.Momentum.Amount = 0;
        }
    }
}
