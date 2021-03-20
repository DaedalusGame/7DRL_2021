using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorLevelEnd : Behavior
    {
        public ICurio Curio;
        public Point Direction;

        public BehaviorLevelEnd()
        {
        }

        public BehaviorLevelEnd(ICurio curio, Point direction)
        {
            Curio = curio;
            Direction = direction;
        }

        public bool CanEscape()
        {
            return Manager.GetBehaviors().OfType<BehaviorKillTarget>().All(x => x.IsCompleted());
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorLevelEnd(curio, Direction), Curio);
        }
    }
}
