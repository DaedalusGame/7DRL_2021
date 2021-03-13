using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorTemplate : Behavior
    {
        public ICurio Curio;
        public Template Template;

        public BehaviorTemplate()
        {
        }

        public BehaviorTemplate(ICurio curio, Template template)
        {
            Curio = curio;
            Template = template;
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            //NOT CLONED
        }

        public override string ToString()
        {
            return $"Template";
        }
    }
}
