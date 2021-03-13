using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionSetMomentum : IActionHasOrigin
    {
        public bool Done => true;

        public ICurio Origin
        {
            get;
            set;
        }
        public Vector2 Angle;

        public ActionSetMomentum(ICurio origin, Vector2 angle)
        {
            Origin = origin;
            Angle = angle;
        }

        public void Run()
        {
            var player = Origin.GetBehavior<BehaviorPlayer>();
            if (player != null)
                player.Momentum.Direction = Angle;
        }
    }
}
