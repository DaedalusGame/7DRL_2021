using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    abstract class Behavior
    {
        public IOrigin Origin;
        public bool Removed = false;

        public abstract void Apply();

        public abstract void Clone(ICurioMapper mapper);

        public virtual void Remove()
        {
            Removed = true;
        }

        public static void Apply(Behavior behavior)
        {
            behavior.Apply();
        }

        public static void Apply(Behavior behavior, IOrigin origin)
        {
            behavior.Origin = origin;
            Apply(behavior);
        }

        public static void Apply(Behavior behavior, ICurio original)
        {
            IOrigin origin;
            var template = original.GetBehavior<BehaviorTemplate>();
            if (template != null)
                origin = new OriginCopyTemplate(template.Template);
            else
                origin = new OriginCopy(original);
            Apply(behavior, origin);
        }
    }
}
