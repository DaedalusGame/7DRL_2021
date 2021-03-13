using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorDagger : Behavior, ITickable
    {
        public ICurio Curio;
        public Slider Upswing = new Slider(1,1);

        public BehaviorDagger()
        {
        }

        public BehaviorDagger(ICurio curio)
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
            Apply(new BehaviorDagger(curio));
        }

        public void Tick(SceneGame scene)
        {
            if (Curio.IsAlive())
                Upswing += scene.TimeMod;
            else
                Upswing.Time = Upswing.EndTime;
        }
    }
}
