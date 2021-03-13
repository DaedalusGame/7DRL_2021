using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorIdentity : Behavior
    {
        public ICurio Curio;
        public string ID;

        public BehaviorIdentity()
        {
        }

        public BehaviorIdentity(ICurio curio, string id)
        {
            Curio = curio;
            ID = id;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            Apply(new BehaviorIdentity(mapper.Map(Curio), ID), Curio);
        }
    }
}
