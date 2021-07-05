using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorGib : Behavior, ITickable
    {
        public ICurio Curio;

        public BehaviorGib()
        {
        }

        public BehaviorGib(ICurio curio)
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
            Apply(new BehaviorGib(curio), Curio);
        }

        public void Tick(SceneGame scene)
        {
            Curio.Destroy();
        }
    }
}
