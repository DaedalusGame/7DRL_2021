using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorFollowCamera : Behavior
    {
        public ICurio Curio;
        public ICurio Camera;

        public BehaviorFollowCamera()
        {
        }

        public BehaviorFollowCamera(ICurio curio, ICurio camera)
        {
            Curio = curio;
            Camera = camera;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            Apply(new BehaviorFollowCamera(mapper.Map(Curio), mapper.Map(Camera)));
        }
    }
}
