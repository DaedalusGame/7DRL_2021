using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorLevelStart : Behavior
    {
        public ICurio Curio;
        public Point Direction;

        public BehaviorLevelStart()
        {
        }

        public BehaviorLevelStart(ICurio curio, Point direction)
        {
            Curio = curio;
            Direction = direction;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorLevelStart(curio, Direction), Curio);
        }
    }
}
