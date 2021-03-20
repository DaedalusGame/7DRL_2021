using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Results;
using Microsoft.Xna.Framework;

namespace _7DRL_2021.Behaviors
{
    class BehaviorGrappleHeart : Behavior, IGrappleTarget
    {
        public ICurio Curio;

        public BehaviorGrappleHeart()
        {
        }

        public BehaviorGrappleHeart(ICurio curio)
        {
            Curio = curio;
        }

        public void AddGrappleAction(List<ActionWrapper> wrappers, ICurio origin, Vector2 direction)
        {
            wrappers.Add(new ActionGrappleHeart(origin, Curio, direction, 10, 5).InSlot(ActionSlot.Active));
            wrappers.Add(new ActionKeepMoving(origin).InSlot(ActionSlot.Active));
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorGrappleHeart(curio), Curio);
        }
    }
}
