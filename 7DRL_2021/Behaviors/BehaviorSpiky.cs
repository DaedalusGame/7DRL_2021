﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorSpiky : Behavior
    {
        public ICurio Curio;

        public BehaviorSpiky()
        {
        }

        public BehaviorSpiky(ICurio curio)
        {
            Curio = curio;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            Apply(new BehaviorSpiky(curio), Curio);
        }
    }
}
