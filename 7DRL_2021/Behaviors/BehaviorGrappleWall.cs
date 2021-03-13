using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Results;
using Microsoft.Xna.Framework;

namespace _7DRL_2021.Behaviors
{
    class BehaviorGrappleWall : Behavior, IGrappleTarget
    {
        public ICurio Curio;

        public BehaviorGrappleWall()
        {
        }

        public BehaviorGrappleWall(ICurio curio)
        {
            Curio = curio;
        }

        public void AddGrappleAction(List<ActionWrapper> wrappers, ICurio origin, Vector2 direction)
        {
            wrappers.Add(new ActionGrappleWall(origin, Curio, direction, 10, 5).InSlot(ActionSlot.Active));
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorGrappleWall(curio));
        }
    }
}
