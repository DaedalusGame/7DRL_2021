using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Systems
{
    abstract class BehaviorSystem
    {
        public static List<BehaviorSystem> AllSystems = new List<BehaviorSystem>();

        protected DeferredList<Behavior> Behaviors = new DeferredList<Behavior>();

        public BehaviorSystem()
        {
            AllSystems.Add(this);
        }

        public abstract bool Accepts(ICurio curio, Behavior behavior);

        public void Add(ICurio curio, Behavior behavior)
        {
            Behaviors.Add(behavior);
        }
    }

    class BehaviorSystemDrawable : BehaviorSystem
    {
        public override bool Accepts(ICurio curio, Behavior behavior)
        {
            return behavior is IDrawable && !(curio is Curio.Templated);
        }

        public IEnumerable<IDrawable> GetDrawables()
        {
            Behaviors.RemoveAll(x => x.Removed);
            return Behaviors.Cast<IDrawable>();
        }
    }

    class BehaviorSystemTickable : BehaviorSystem
    {
        public override bool Accepts(ICurio curio, Behavior behavior)
        {
            return behavior is ITickable && !(curio is Curio.Templated);
        }

        public IEnumerable<ITickable> GetTickables()
        {
            Behaviors.RemoveAll(x => x.Removed);
            return Behaviors.Cast<ITickable>();
        }
    }
}
