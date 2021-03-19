using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionGripCancel : IActionHasOrigin
    {
        public ICurio Origin { get; set; }
        public Vector2 Direction;
        public bool Done => true;

        public ActionGripCancel(ICurio origin, Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public void Run()
        {
            var player = Origin.GetBehavior<BehaviorPlayer>();
            var grapple = Origin.GetBehavior<BehaviorGrapplingHook>();
            var orientable = Origin.GetBehavior<BehaviorOrientable>();

            grapple.GripDirection = null;
            orientable.OrientTo(Util.VectorToAngle(Direction));
            if (player != null)
            {
                player.Momentum.Direction = Direction;
                player.Momentum.Amount = 0;
            }
        }
    }
}
